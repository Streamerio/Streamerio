using UnityEngine;

public class GhostMovement : MonoBehaviour, IAttackable
{
    [SerializeField] private GhostScriptableObject _ghostScriptableObject;

    private float _speed;
    private float _attackCoolTime;
    
    private Transform _player;
    private float _lastAttackTime = -999f;

    public float Power => _ghostScriptableObject.Power;

    void Awake()
    {
        _speed = _ghostScriptableObject.Speed;
        _attackCoolTime = _ghostScriptableObject.AttackCoolTime;
    }
    
    void Start()
    {
        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
        
        // 重力を無効化
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;

        float randPosX = Random.Range(_ghostScriptableObject.MinRelativeSpawnPosX, _ghostScriptableObject.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_ghostScriptableObject.MinRelativeSpawnPosY, _ghostScriptableObject.MaxRelativeSpawnPosY);
        transform.position += new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, 0);
    }
    
    void Update()
    {
        if (_player == null) return;
        MoveTowardsPlayer();
    }
    
    private void MoveTowardsPlayer()
    {
        // プレイヤーの方向を計算
        Vector2 direction = (_player.position - transform.position).normalized;
        
        // プレイヤーに向かって移動
        transform.position += (Vector3)(direction * _speed * Time.deltaTime);
        
        // スプライトの向きを調整
        if (direction.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
    
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            AttackPlayer();
        }
    }
    
    private void AttackPlayer()
    {
        if (Time.time - _lastAttackTime < _attackCoolTime) return;
        
        //Debug.Log($"Ghost attacked player for {_attackManager.CurrentDamage} damage!");
        _lastAttackTime = Time.time;
    }
}