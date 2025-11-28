using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Skelton : MonoBehaviour, IAttackable, IHealth
{
    private SkeltonScriptableObject _config;
    private float _speed;
    private float _startMoveDelay;

    private float _spawnTime;
    private bool _canMove = false;

    private Transform _player;

    private IAudioFacade _audioFacade;
    private EnemyHpManager _enemyHpManager;

    public float Power => _config.Power;
    public int Health => _config.Health;

    void Awake()
    {
        _spawnTime = Time.time;
        _canMove = false;
    }

    [Inject]
    private void Construct(SkeltonScriptableObject config, EnemyHpManager enemyHpManager)
    {
        _config = config;
        _enemyHpManager = enemyHpManager;

        _audioFacade = null;

        _speed = _config.Speed;
        _startMoveDelay = _config.StartMoveDelay;

        _enemyHpManager.Initialize(_config.Health);
    }

    /// <summary>
    /// コンストラクタで注入されなかった場合に、親の LifetimeScope から設定を取得するフォールバックメソッド
    /// ゲーム中に同時に二体以上のスケルトンが生成されると二体目以降が動作しない問題を回避
    /// </summary>
    /// <exception cref="System.InvalidOperationException"></exception>
    private void EnsureConfigFromScopeFallback()
    {
        if (_config != null) return;

        var scope = GetComponentInParent<SkeltonLifetimeScope>(true);
        if (scope == null) throw new System.InvalidOperationException("SkeltonLifetimeScope not found in parent hierarchy.");

        var cfg = scope.Config;
        if (cfg == null) throw new System.InvalidOperationException("SkeltonLifetimeScope.Config is null.");

        _config = cfg;
        _speed = _config.Speed;
        _startMoveDelay = _config.StartMoveDelay;

        if (_enemyHpManager == null)
        {
            _enemyHpManager = GetComponent<EnemyHpManager>();
            if (_enemyHpManager == null) throw new System.InvalidOperationException("EnemyHpManager not found on Skelton.");
            _enemyHpManager.Initialize(_config.Health);
        }
    }

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        EnsureConfigFromScopeFallback();
        float randPosX = Random.Range(_config.MinRelativeSpawnPosX, _config.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_config.MinRelativeSpawnPosY, _config.MaxRelativeSpawnPosY);
        transform.position = new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, transform.position.z);
    }

    void Update()
    {
        if (!_canMove)
        {
            if (Time.time - _spawnTime >= _startMoveDelay)
            {
                _canMove = true;
            }
            else
            {
                return;
            }
        }
        transform.Translate(Vector2.left * _speed * Time.deltaTime);
    }
}