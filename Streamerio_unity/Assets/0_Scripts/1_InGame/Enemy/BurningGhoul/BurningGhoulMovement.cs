using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class BurningGhoulMovement : MonoBehaviour, IAttackable, IHealth
{
    private BurningGhoulScriptableObject _config;
    private float _speed;
    private float _detectionRange;
    private float _stopDistance;

    private Transform _player;

    private EnemyHpManager _enemyHpManager;

    public float Power => _config.Power;
    public int Health => _config.Health;

    void Awake()
    {
        _enemyHpManager = GetComponent<EnemyHpManager>();
    }

    [Inject]
    private void Construct(BurningGhoulScriptableObject config, EnemyHpManager enemyHpManager)
    {
        if (config == null) throw new System.ArgumentNullException(nameof(config));
        if (enemyHpManager == null) throw new System.ArgumentNullException(nameof(enemyHpManager));

        _config = config;
        _enemyHpManager = enemyHpManager;

        _speed = _config.Speed;
        _detectionRange = _config.DetectionRange;
        _stopDistance = _config.StopRange;

        _enemyHpManager.Initialize(Health);
    }

    private void EnsureConfigFromScopeFallback()
    {
        if (_config != null) return;

        var scope = GetComponentInParent<BurningGhoulLifeTimeScope>(true);
        if (scope == null) throw new System.InvalidOperationException("BurningGhoulLifeTimeScope not found in parent hierarchy.");

        var cfg = scope.Config;
        if (cfg == null) throw new System.InvalidOperationException("BurningGhoulLifeTimeScope.Config is null.");

        _config = cfg;
        _speed = _config.Speed;
        _detectionRange = _config.DetectionRange;
        _stopDistance = _config.StopRange;
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        EnsureConfigFromScopeFallback();

        // 保険：EnemyHpManager が null なら取得し、必ず初期化する
        if (_enemyHpManager == null) _enemyHpManager = GetComponent<EnemyHpManager>();
        _enemyHpManager.Initialize(Health);

        float randPosX = Random.Range(_config.MinRelativeSpawnPosX, _config.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_config.MinRelativeSpawnPosY, _config.MaxRelativeSpawnPosY);
        transform.position += new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, 0);
    }

    void Update()
    {
        if (_enemyHpManager != null && _enemyHpManager.IsDead) return;
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer <= _detectionRange && distanceToPlayer > _stopDistance)
        {
            Vector2 direction = (_player.position - transform.position).normalized;
            transform.Translate(direction * _speed * Time.deltaTime);

            if (direction.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
    
    public void TakeDamage(int amount)
    {
        if (_enemyHpManager == null) _enemyHpManager = GetComponent<EnemyHpManager>();
        _enemyHpManager.TakeDamage(amount);
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(Mathf.CeilToInt(amount));
    }
}