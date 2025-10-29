using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

public class GatoWalkMovement : MonoBehaviour, IAttackable
{
    [SerializeField] private GatoWalkScriptableObject _gatoWalkMovementScriptableObject;

    private float _speed;
    private float _jumpForce;
    private float _jumpInterval;
    private float _attackCoolTime;

    private bool _isGrounded = true;
    private float _jumpTimer = 0f;
    private float _lastAttackTime = -999f;

    private Transform _player;
    private Rigidbody2D _rigidbody;

    public float Power => _gatoWalkMovementScriptableObject.Power;

    void Awake()
    {
        _speed = _gatoWalkMovementScriptableObject.Speed;
        _jumpForce = _gatoWalkMovementScriptableObject.JumpForce;
        _jumpInterval = _gatoWalkMovementScriptableObject.JumpInterval;
        _attackCoolTime = _gatoWalkMovementScriptableObject.AttackCoolTime;

        _jumpTimer = _jumpInterval;
    }

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        // プレイヤーを探す
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        // 物理設定
            _rigidbody.gravityScale = 2f;
        _rigidbody.freezeRotation = true;

        float randPosX = Random.Range(_gatoWalkMovementScriptableObject.MinRelativeSpawnPosX, _gatoWalkMovementScriptableObject.MaxRelativeSpawnPosX);
        float randPosY = Random.Range(_gatoWalkMovementScriptableObject.MinRelativeSpawnPosY, _gatoWalkMovementScriptableObject.MaxRelativeSpawnPosY);
        transform.position += new Vector3(_player.position.x + randPosX, _player.position.y + randPosY, 0);

        // AudioManager.Instance.PlayAsync(SEType.Monster012, destroyCancellationToken).Forget();
    }
    
    void Update()
    {
        HandleMovement();
        HandleJump();
    }
    
    private void HandleMovement()
    {
        // 常に左方向に高速移動
        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.x = _speed;
        _rigidbody.linearVelocity = velocity;
    }
    
    private void HandleJump()
    {
        _jumpTimer -= Time.deltaTime;
        
        // 接地時かつジャンプタイマーが0以下の場合ジャンプ
        if (_isGrounded && _jumpTimer <= 0f)
        {
            Jump();
            _jumpTimer = _jumpInterval;
        }
    }
    
    private void Jump()
    {
        Vector2 velocity = _rigidbody.linearVelocity;
        velocity.y = _jumpForce;
        _rigidbody.linearVelocity = velocity;
        _isGrounded = false;
        
        //Debug.Log("GatoWalk jumped!");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 地面判定（下方向の接触）
        if (collision.contacts.Length > 0)
        {
            Vector2 normal = collision.contacts[0].normal;
            if (normal.y > 0.7f) // 上向きの法線 = 地面
            {
                _isGrounded = true;
            }
        }
    }
}