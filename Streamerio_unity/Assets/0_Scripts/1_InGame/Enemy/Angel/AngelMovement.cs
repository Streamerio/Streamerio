using System.Collections.Generic;
using UnityEngine;
using VContainer;
public class AngelMovement : MonoBehaviour
{
    private AngelScriptableObject _config;

    private float verticalSpeed;
    private float horizontalSpeed;
    private float verticalRange;
    private float horizontalRange;
    private float baseLeftSpeed;

    private int _hp;

    private GameObject energyCirclePrefab;
    private float attackInterval;
    private float circleLifetime;
    private float circleRadius;
    private int circleCount;
    private float circleExpandDuration;
    private float circleOrbitSpeedDeg;
    private bool randomizeStartAngle;

    private Vector3 _startPosition;
    private float _verticalTimer;
    private float _horizontalTimer;
    private float _attackTimer;
    private EnemyAttackManager _attackManager;
    private EnemyHpManager _enemyHpManager;
    private List<AngelEnergyCircle> _circlePool = new List<AngelEnergyCircle>();
    private bool _poolInitialized = false;

    void Awake()
    {
        // 保険で取得しておく
        if (_attackManager == null) _attackManager = GetComponent<EnemyAttackManager>();
        if (_enemyHpManager == null) _enemyHpManager = GetComponent<EnemyHpManager>();
    }

    // VContainer による注入メソッド
    [Inject]
    private void Construct(AngelScriptableObject config, EnemyHpManager enemyHpManager)
    {
        if (config == null) throw new System.ArgumentNullException(nameof(config));
        if (enemyHpManager == null) throw new System.ArgumentNullException(nameof(enemyHpManager));

        _config = config;
        _enemyHpManager = enemyHpManager;

        // SO から各値をコピー
        verticalSpeed = _config.verticalSpeed;
        horizontalSpeed = _config.horizontalSpeed;
        verticalRange = _config.verticalRange;
        horizontalRange = _config.horizontalRange;
        baseLeftSpeed = _config.baseLeftSpeed;

        _hp = _config.Health;

        energyCirclePrefab = _config.energyCirclePrefab;
        attackInterval = _config.attackInterval;
        circleLifetime = _config.circleLifetime;
        circleRadius = _config.circleRadius;
        circleCount = _config.circleCount;
        circleExpandDuration = _config.circleExpandDuration;
        circleOrbitSpeedDeg = _config.circleOrbitSpeedDeg;
        randomizeStartAngle = _config.randomizeStartAngle;

        // HP マネージャ初期化
        _enemyHpManager.Initialize(_hp);
    }

    // Scope から SO を取得するフォールバック（BurningGhoul と同様の保険）
    private void EnsureConfigFromScopeFallback()
    {
        if (_config != null) return;

        var scope = GetComponentInParent<AngelLifeTimeScope>(true);
        if (scope == null) return;

        var cfg = scope.Config;
        if (cfg == null) return;

        _config = cfg;

        verticalSpeed = _config.verticalSpeed;
        horizontalSpeed = _config.horizontalSpeed;
        verticalRange = _config.verticalRange;
        horizontalRange = _config.horizontalRange;
        baseLeftSpeed = _config.baseLeftSpeed;

        _hp = _config.Health;

        energyCirclePrefab = _config.energyCirclePrefab;
        attackInterval = _config.attackInterval;
        circleLifetime = _config.circleLifetime;
        circleRadius = _config.circleRadius;
        circleCount = _config.circleCount;
        circleExpandDuration = _config.circleExpandDuration;
        circleOrbitSpeedDeg = _config.circleOrbitSpeedDeg;
        randomizeStartAngle = _config.randomizeStartAngle;
    }

    void Start()
    {
        var player = GameObject.FindGameObjectWithTag("Player").transform;

        EnsureConfigFromScopeFallback();

        _startPosition = transform.position;
        if (_attackManager == null) _attackManager = GetComponent<EnemyAttackManager>();
        if (_enemyHpManager == null) _enemyHpManager = GetComponent<EnemyHpManager>();
        if (_enemyHpManager != null) _enemyHpManager.Initialize(_hp);

        _attackTimer = attackInterval;

        float randPosX = Random.Range(_config.MinRelativeSpawnPosX, _config.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_config.MinRelativeSpawnPosY, _config.MaxRelativeSpawnPosY);
        transform.position += new Vector3(player.position.x + randPosX, player.position.y + randPosY, 0);
        _startPosition = transform.position;
    }

    void Update()
    {
        // 死亡判定等は EnemyHpManager 側で行う想定
        HandleMovement();
        HandleAttack();
    }

    private void HandleMovement()
    {
        _verticalTimer += Time.deltaTime;
        _horizontalTimer += Time.deltaTime * 0.7f;

        float verticalOffset = Mathf.Sin(_verticalTimer * verticalSpeed) * verticalRange;
        float horizontalOffset = Mathf.Sin(_horizontalTimer * horizontalSpeed) * horizontalRange;

        Vector3 newPosition = new Vector3(
            _startPosition.x + horizontalOffset,
            _startPosition.y + verticalOffset,
            _startPosition.z
        );
        transform.position = newPosition;
    }

    private void HandleAttack()
    {
        _attackTimer -= Time.deltaTime;
        if (_attackTimer <= 0f)
        {
            CreateEnergyCircleWave();
            _attackTimer = attackInterval;
        }
    }

    private void CreateEnergyCircleWave()
    {
        if (energyCirclePrefab == null || circleCount <= 0) return;

        // プール初期化（初回のみ）
        if (!_poolInitialized)
        {
            int maxConcurrentWaves = Mathf.Max(1, Mathf.CeilToInt(circleLifetime / Mathf.Max(0.0001f, attackInterval)));
            int initialPoolSize = circleCount * maxConcurrentWaves;

            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject circleObj = Instantiate(energyCirclePrefab, transform.position, Quaternion.identity, this.transform);
                var circle = circleObj.GetComponent<AngelEnergyCircle>() ?? circleObj.AddComponent<AngelEnergyCircle>();
                circleObj.SetActive(false);
                _circlePool.Add(circle);
            }

            _poolInitialized = true;
        }

        float baseAngleOffset = randomizeStartAngle ? Random.Range(0f, 360f) : 0f;
        float angleStep = 360f / circleCount;

        for (int i = 0; i < circleCount; i++)
        {
            float deg = baseAngleOffset + angleStep * i;
            float rad = deg * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

            // 非アクティブなプール要素を探す
            AngelEnergyCircle circle = _circlePool.Find(c => !c.gameObject.activeInHierarchy);
            if (circle == null)
            {
                // 足りなければ追加生成してプールに加える
                GameObject circleObj = Instantiate(energyCirclePrefab, transform.position, Quaternion.identity, this.transform);
                circle = circleObj.GetComponent<AngelEnergyCircle>() ?? circleObj.AddComponent<AngelEnergyCircle>();
                circleObj.SetActive(false);
                _circlePool.Add(circle);
            }

            int damage = _attackManager != null ? _attackManager.CurrentDamage : 10;

            // 再利用: アクティブにして初期化
            circle.gameObject.SetActive(true);
            circle.Initialize(
                damage,
                circleLifetime,
                followTarget: transform,
                direction: dir,
                maxRadius: circleRadius,
                expandDuration: circleExpandDuration,
                orbitSpeedDeg: circleOrbitSpeedDeg
            );
        }

        Debug.Log("[AngelMovement] Created energy circle wave (pooled)");
    }

    public void TakeDamage(int amount)
    {
        if (_enemyHpManager == null) _enemyHpManager = GetComponent<EnemyHpManager>();
        if (_enemyHpManager != null) _enemyHpManager.TakeDamage(amount);
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(Mathf.CeilToInt(amount));
    }
}