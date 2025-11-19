using Alchemy.Inspector;
using Cysharp.Text;
using R3;
using UnityEngine;
using Common.Audio;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(PlayerAnimation))]
public class PlayerView : MonoBehaviour
{
    [SerializeField, ReadOnly]
    private SpriteRenderer _spriteRenderer;
    [SerializeField, ReadOnly]
    private Rigidbody2D _rb2D;
    [SerializeField, LabelText("地面判定用コライダー")]
    private PlayerGrounded _groundedCollider;
    [SerializeField, LabelText("アニメーション"), ReadOnly]
    private PlayerAnimation _animation;
    
    [SerializeField] private HpPresenter hpPresenter;
    [SerializeField] private float hitCooldown = 0.4f;     // 連続ヒット抑制
    private float _lastHitTime = -999f;
    
    // private bool _isGrounded = false; // 地面に接地しているかどうかのフラグ
    // public bool IsGrounded => _isGrounded;

#if UNITY_EDITOR
    private void OnValidate()
    {
        _spriteRenderer ??= GetComponent<SpriteRenderer>();
        _animation ??= GetComponent<PlayerAnimation>();
        _rb2D ??= GetComponent<Rigidbody2D>();
        _groundedCollider ??= GetComponentInChildren<PlayerGrounded>();
    }
#endif

    public void Bind()
    {
        _groundedCollider.IsGrounded
            .DistinctUntilChanged()
            .Where(x => x)
            .Subscribe(_ =>
            {
                _animation.PlayJump(false);
            }).RegisterTo(destroyCancellationToken);
    }
    
    public void Move(Vector2 newPosition)
    { 
        //Debug.Log($"PlayerView Move to {newPosition}");
        
        _animation.PlayRun(newPosition.x);
        
        if(newPosition.x != 0)
            _spriteRenderer.flipX = newPosition.x < 0;
        
        gameObject.transform.position += new Vector3(newPosition.x, newPosition.y, 0);
    }

    public void Jump(float force)
    {
        if (!_groundedCollider.IsGrounded.CurrentValue) return;
        // Debug.Log($"PlayerView Jump with force {force}");
        // ジャンプの実装例（Rigidbody2Dがアタッチされている場合）
        AudioManager.Instance.PlayAsync(SEType.PlayerJump, destroyCancellationToken).Forget();
        _animation.PlayJump(true);
        _rb2D.AddForce(new Vector2(0, force), ForceMode2D.Impulse);
    }

    public void Attack(int num)
    {
        _animation.PlayAttack(num);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("触れた");
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("敵");
            // EnemyAttackManager 取得（存在しない敵でもエラーにならないよう防御的に）
            var attackManager = collision.gameObject.GetComponent<EnemyAttackManager>();
            if (attackManager != null)
            {
                // EnemyAttackManager に CurrentDamage プロパティ（または public int）がある想定
                TakeDamage(attackManager.CurrentDamage);
            }
            else
            {
                Debug.LogWarning($"[PlayerView] EnemyAttackManager が {collision.gameObject.name} にありません。");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("敵");
            var attackManager = collision.gameObject.GetComponent<EnemyAttackManager>();
            if (attackManager != null)
            {
                // EnemyAttackManager に CurrentDamage プロパティ（または public int）がある想定
                TakeDamage(attackManager.CurrentDamage);
            }
            else
            {
                Debug.LogWarning($"[PlayerView] EnemyAttackManager が {collision.gameObject.name} にありません。");
            }
        }
    }

    private void TakeDamage(int damage)
    {
        if (hpPresenter == null) return;
        //if (Time.time - _lastHitTime < hitCooldown) return;

        hpPresenter.Decrease(damage);
        //Debug.Log($"Enemy attacked! Player HP: {hpPresenter.Amount}");
        _lastHitTime = Time.time;
    }
}