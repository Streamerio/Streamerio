using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class Skeleton : MonoBehaviour, IAttackable
{
    [SerializeField] private SkeletonScriptableObject _skeletonScriptableObject;
    private float _speed;
    private float _startMoveDelay; // 追加: 移動開始までの遅延

    private float _spawnTime;
    private bool _canMove = false;

    private Transform _player;
    
    private IAudioFacade _audioFacade;
    

    public float Power => _skeletonScriptableObject.Power;

    void Awake()
    {
        _speed = _skeletonScriptableObject.Speed;
        _startMoveDelay = _skeletonScriptableObject.StartMoveDelay; // 追加: スクリプタブルオブジェクトから取得

        _spawnTime = Time.time;              // 追加: 出現時間記録
        _canMove = false;   
    }

    void Start()
    {
        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        
        float rand = Random.Range(5f, 8f);
        transform.position += new Vector3(_player.position.x + rand, _player.position.y, 0); // 少し上にずらして生成
        AudioManager.Instance.AudioFacade.PlayAsync(SEType.Monster012, destroyCancellationToken).Forget();
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