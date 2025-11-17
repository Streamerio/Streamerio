// ...existing code...
using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BurningGhoulMovement : MonoBehaviour, IAttackable, IHealth, IStartable, ITickable
{
    private BurningGhoulScriptableObject _config;
    private float _speed;
    private float _detectionRange;
    private float _stopDistance;

    private Transform _player;

    private EnemyHpManager _enemyHpManager;

    public float Power => _config.Power;
    public int Health => _config.Health;

    [Inject]
    private void Construct(BurningGhoulScriptableObject config, EnemyHpManager enemyHpManager)
    {
        _config = config;
        _enemyHpManager = enemyHpManager;

        _speed = _config.Speed;
        _detectionRange = _config.DetectionRange;
        _stopDistance = _config.StopRange;

        _enemyHpManager.Initialize(Health);
                
    }

    void IStartable.Start()
    {
        EnsureConfigFromScopeFallback();
        _player = PlayerSingleton.Instance.transform;
        InitializePositions();
    }

    void ITickable.Tick()
    {
        FollowPlayer();
    }

    private void InitializePositions()
    {
        float randX = Random.Range(_config.MinRelativeSpawnPosX, _config.MaxRelativeSpawnPosX);
        float randY = Random.Range(_config.MinRelativeSpawnPosY, _config.MaxRelativeSpawnPosY);
        transform.position = new Vector3(_player.position.x + randX, _player.position.y + randY, 0f);
    }

    private void FollowPlayer()
    {
        if (_player == null)
        {
            Debug.LogError($"[BurningGhoul] FollowPlayer: _player is null id:{gameObject?.GetInstanceID().ToString() ?? "null"}");
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        Debug.Log($"[BurningGhoul] FollowPlayer id:{gameObject?.GetInstanceID().ToString() ?? "null"} distance:{distanceToPlayer} detectionRange:{_detectionRange} stopDistance:{_stopDistance}");

        if (distanceToPlayer <= _detectionRange && distanceToPlayer > _stopDistance)
        {
            Vector2 direction = (_player.position - transform.position).normalized;
            transform.Translate(direction * _speed * Time.deltaTime);
            Debug.Log($"[BurningGhoul] Moving id:{gameObject?.GetInstanceID().ToString() ?? "null"} dir:{direction} speed:{_speed}");

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

    private void EnsureConfigFromScopeFallback()
    {
        if (_config != null) return;

        var scope = GetComponentInParent<BurningGhoulLifeTimeScope>(true);
        if (scope == null)
        {
            Debug.LogError($"[BurningGhoul] EnsureConfigFromScopeFallback: BurningGhoulLifeTimeScope not found in parent hierarchy for id:{gameObject?.GetInstanceID().ToString() ?? "null"}");
            throw new System.InvalidOperationException("BurningGhoulLifeTimeScope not found in parent hierarchy.");
        }

        var config = scope.Config;
        if (config == null)
        {
            Debug.LogError($"[BurningGhoul] EnsureConfigFromScopeFallback: BurningGhoulLifeTimeScope.Config is null for id:{gameObject?.GetInstanceID().ToString() ?? "null"}");
            throw new System.InvalidOperationException("BurningGhoulLifeTimeScope.Config is null.");
        }
        _config = config;
        _speed = _config.Speed;
        _detectionRange = _config.DetectionRange;
        _stopDistance = _config.StopRange;
        Debug.Log($"[BurningGhoul] Config fallback assigned id:{gameObject?.GetInstanceID().ToString() ?? "null"} speed:{_speed} detection:{_detectionRange} stop:{_stopDistance}");
    }

    public void TakeDamage(int amount)
    {
        Debug.Log($"[BurningGhoul] TakeDamage called id:{gameObject?.GetInstanceID().ToString() ?? "null"} amount:{amount} beforeHpManagerNull:{_enemyHpManager == null}");
        if (_enemyHpManager == null)
        {
            Debug.LogError($"[BurningGhoul] TakeDamage: _enemyHpManager is null id:{gameObject?.GetInstanceID().ToString() ?? "null"}");
            return;
        }
        _enemyHpManager.TakeDamage(amount);
    }

    public void TakeDamage(float amount)
    {
        TakeDamage(Mathf.CeilToInt(amount));
    }
}
// ...existing code...