using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;


public class BurningGhoulMovement : MonoBehaviour, IAttackable
{
    [SerializeField] private BurningGhoulScriptableObject _burningGhoulScriptableObject;

    private float _speed;
    private float _detectionRange;
    private float _stopDistance;

    private Transform _player;

    public float Power => _burningGhoulScriptableObject.Power;

    void Awake()
    {
        _speed = _burningGhoulScriptableObject.Speed;
        _detectionRange = _burningGhoulScriptableObject.DetectionRange;
        _stopDistance = _burningGhoulScriptableObject.StopRange;
    }

    void Start()
    {
        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        _player = playerObj.transform;
        
        float randPosX = Random.Range(_burningGhoulScriptableObject.MinRelativeSpawnPosX, _burningGhoulScriptableObject.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_burningGhoulScriptableObject.MinRelativeSpawnPosY, _burningGhoulScriptableObject.MaxRelativeSpawnPosY);
        transform.position += new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, 0);

        AudioManager.Instance.PlayAsync(SEType.Monster012, destroyCancellationToken).Forget();
    }
    
    void Update()
    {
        FollowPlayer();
    }
    
    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        
        // 検出範囲内かつ停止距離より遠い場合のみ移動
        if (distanceToPlayer <= _detectionRange && distanceToPlayer > _stopDistance)
        {
            // プレイヤーの方向を計算
            Vector2 direction = (_player.position - transform.position).normalized;
            
            // プレイヤーに向かって移動
            transform.Translate(direction * _speed * Time.deltaTime);
            
            // スプライトの向きを調整（オプション）
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


}