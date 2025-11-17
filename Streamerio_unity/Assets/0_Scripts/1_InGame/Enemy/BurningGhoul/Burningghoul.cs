using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;


public class BurningGhoulMovement : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float followSpeed = 1.5f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float stopDistance = 0.5f; // プレイヤーに近づきすぎないための距離
    [SerializeField] private float attackCooldown = 0.8f; // 攻撃間隔
    
    private Transform _player;
    private EnemyAttackManager _attackManager;
    private float _lastAttackTime = -999f;
    
    private IAudioFacade _audioFacade;
    
    [Inject]
    public void Construct(IAudioFacade audioFacade)
    {
        _audioFacade = audioFacade;
    }

    void Start()
    {
        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Player not found for BurningGhoul!");
        }

        _attackManager = GetComponent<EnemyAttackManager>();
        
        float rand = Random.Range(6f, 10f);
        transform.position += new Vector3(_player.position.x + rand, _player.position.y, 0); // 少し上にずらして生成
        AudioManager.Instance.AudioFacade.PlayAsync(SEType.Monster012, destroyCancellationToken).Forget();
    }
    
    void Update()
    {
        if (_player == null) return;
        
        FollowPlayer();
    }
    
    private void FollowPlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        
        // 検出範囲内かつ停止距離より遠い場合のみ移動
        if (distanceToPlayer <= detectionRange && distanceToPlayer > stopDistance)
        {
            // プレイヤーの方向を計算
            Vector2 direction = (_player.position - transform.position).normalized;
            
            // プレイヤーに向かって移動
            transform.Translate(direction * followSpeed * Time.deltaTime);
            
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