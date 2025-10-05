using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Skelton : MonoBehaviour, IAttackable, IHealth
{
    [SerializeField] private SkeltonScriptableObject _skeltonScriptableObject;
    private float _speed;
    private float _startMoveDelay; // 追加: 移動開始までの遅延

    private float _spawnTime;
    private bool _canMove = false;

    private Transform _player;
    
    private IAudioFacade _audioFacade;
    

    private EnemyHpManager _enemyHpManager;

    public float Power => _skeltonScriptableObject.Power;
    public int Health => _skeltonScriptableObject.Health;

    void Awake()
    {
        _speed = _skeltonScriptableObject.Speed;
        _startMoveDelay = _skeltonScriptableObject.StartMoveDelay; // 追加: スクリプタブルオブジェクトから取得

        _spawnTime = Time.time;              // 追加: 出現時間記録
        _canMove = false;
        _enemyHpManager = GetComponent<EnemyHpManager>();
    }

    void Start()
    {
        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        _enemyHpManager.Initialize(Health);

        float randPosX = Random.Range(_skeltonScriptableObject.MinRelativeSpawnPosX, _skeltonScriptableObject.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_skeltonScriptableObject.MinRelativeSpawnPosY, _skeltonScriptableObject.MaxRelativeSpawnPosY);
        transform.position += new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, 0);

        AudioManager.Instance.PlayAsync(SEType.Monster012, destroyCancellationToken).Forget();
    }

    void Update()
    {
        if (!_canMove)
        {
            if (Time.time - _spawnTime >= _startMoveDelay)
            {
                _canMove = true;             // 追加: 一度だけ移行
            }
            else
            {
                return;                      // まだ移動しない
            }
        }
        transform.Translate(Vector2.left * _speed * Time.deltaTime);
    }
}