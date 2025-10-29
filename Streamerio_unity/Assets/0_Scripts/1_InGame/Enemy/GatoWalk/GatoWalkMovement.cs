using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class GatoWalkMovement : MonoBehaviour, IAttackable, IHealth
{
    private GatoWalkScriptableObject _config;
    private float _speed;
    private float _jumpForce;
    private float _jumpInterval;
    private float _attackCoolTime;

    private bool _isGrounded = true;
    private float _jumpTimer = 0f;
    private float _lastAttackTime = -999f;

    private float _detectionRange;
    private float _stopDistance;

    private Transform _player;
    private Rigidbody2D _rigidbody;

    private EnemyHpManager _enemyHpManager;

    public float Power => _config.Power;
    public int Health => _config.Health;

    void Awake()
    {
        _jumpTimer = 0f;
    }

    [Inject]
    private void Construct(GatoWalkScriptableObject config, EnemyHpManager enemyHpManager)
    {
        if (config == null) throw new System.ArgumentNullException(nameof(config));
        if (enemyHpManager == null) throw new System.ArgumentNullException(nameof(enemyHpManager));

        _config = config;
        _enemyHpManager = enemyHpManager;

        _speed = _config.Speed;
        _jumpForce = _config.JumpForce;
        _jumpInterval = _config.JumpInterval;
        _attackCoolTime = _config.AttackCoolTime;

        _detectionRange = _config.DetectionRange;
        _stopDistance = _config.StopRange;

        _jumpTimer = _jumpInterval;

        _enemyHpManager.Initialize(Health);
    }

    private void EnsureConfigFromScopeFallback()
    {
        if (_config != null) return;

        var scope = GetComponentInParent<GatoWalkLifeTimeScope>(true);
        if (scope == null) throw new System.InvalidOperationException("GatoWalkLifeTimeScope not found in parent hierarchy.");

        var cfg = scope.Config;
        if (cfg == null) throw new System.InvalidOperationException("GatoWalkLifeTimeScope.Config is null.");

        _config = cfg;
        _speed = _config.Speed;
        _jumpForce = _config.JumpForce;
        _jumpInterval = _config.JumpInterval;
        _attackCoolTime = _config.AttackCoolTime;
        _detectionRange = _config.DetectionRange;
        _stopDistance = _config.StopRange;
        _jumpTimer = _jumpInterval;
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        EnsureConfigFromScopeFallback();

        // 追加確認・保険：初期化が確実に行われるようにここでも Initialize
        if (_enemyHpManager == null) _enemyHpManager = GetComponent<EnemyHpManager>();
        _enemyHpManager.Initialize(Health);
        if (_player == null) throw new System.InvalidOperationException("Player not found in scene.");

        float randPosX = Random.Range(_config.MinRelativeSpawnPosX, _config.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_config.MinRelativeSpawnPosY, _config.MaxRelativeSpawnPosY);
        transform.position += new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, 0);

        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = 2f;
        _rigidbody.freezeRotation = true;
    }

    void Update()
    {
        FollowPlayer();
        HandleJump();
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

    private void HandleJump()
    {
        _jumpTimer -= Time.deltaTime;
        if (_isGrounded && _jumpTimer <= 0f)
        {
            Jump();
            _jumpTimer = _jumpInterval;
        }
    }

    private void Jump()
    {
        var vel = _rigidbody.linearVelocity;
        vel.y = _jumpForce;
        _rigidbody.linearVelocity = vel;
        _isGrounded = false;
    }
}