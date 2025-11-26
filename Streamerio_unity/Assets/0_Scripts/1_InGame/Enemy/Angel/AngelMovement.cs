using System.Collections.Generic;
using UnityEngine;

public class AngelMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float verticalSpeed = 2f;
    [SerializeField] private float horizontalSpeed = 1f;
    [SerializeField] private float verticalRange = 3f;
    [SerializeField] private float horizontalRange = 4f;
    [SerializeField] private float baseLeftSpeed = 0.8f;

    [Header("体力設定")]
    [SerializeField] private int _hp = 100;

    [Header("攻撃設定")]
    [SerializeField] private GameObject energyCirclePrefab;
    [SerializeField] private float attackInterval = 4f;
    [SerializeField] private float circleLifetime = 2f;
    [SerializeField, Tooltip("最終(展開後)の半径")] private float circleRadius = 12f;
    [SerializeField, Tooltip("同時生成数(円周配置)")] private int circleCount = 8;
    [SerializeField, Tooltip("半径へ到達するまでの拡張時間")] private float circleExpandDuration = 0.6f;
    [SerializeField, Tooltip("最大半径到達後の角速度(度/秒)")] private float circleOrbitSpeedDeg = 50f;
    [SerializeField, Tooltip("角度をランダム化")] private bool randomizeStartAngle = true;

    private Vector3 _startPosition;
    private float _verticalTimer;
    private float _horizontalTimer;
    private float _attackTimer;
    private EnemyAttackManager _attackManager;
    private EnemyHpManager _enemyHpManager;

    // プーリング関連
    private List<AngelEnergyCircle> _circlePool = new List<AngelEnergyCircle>();
    private bool _poolInitialized = false;

    void Start()
    {
        _startPosition = transform.position;
        _attackManager = GetComponent<EnemyAttackManager>();
        _enemyHpManager = GetComponent<EnemyHpManager>();
        _attackTimer = attackInterval;

        if (_enemyHpManager != null)
        {
            _enemyHpManager.Initialize(_hp);
        }
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
    }

    private void HandleMovement()
    {
        _verticalTimer += Time.deltaTime;
        _horizontalTimer += Time.deltaTime * 0.7f;

        float verticalOffset = Mathf.Sin(_verticalTimer * verticalSpeed) * 0.8f;
        float horizontalOffset = Mathf.Sin(_horizontalTimer * horizontalSpeed) * 1f;

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
            // ここで +1 を削除：重複して二倍生成される問題を解消
            int maxConcurrentWaves = Mathf.Max(1, Mathf.CeilToInt(circleLifetime / Mathf.Max(0.0001f, attackInterval)));
            int initialPoolSize = circleCount * maxConcurrentWaves;

            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject circleObj = Instantiate(energyCirclePrefab, transform.position, Quaternion.identity);
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
                GameObject circleObj = Instantiate(energyCirclePrefab, transform.position, Quaternion.identity);
                circle = circleObj.GetComponent<AngelEnergyCircle>() ?? circleObj.AddComponent<AngelEnergyCircle>();
                circleObj.SetActive(false);
                _circlePool.Add(circle);
            }

            int damage = _attackManager != null ? _attackManager.CurrentDamage : 10;

            // 再利用: アクティブにして初期化（Initialize 内で位置を設定）
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