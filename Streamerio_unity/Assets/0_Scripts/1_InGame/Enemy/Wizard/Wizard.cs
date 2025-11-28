// using UnityEngine;

// public class WizardAttackMovement : MonoBehaviour
// {
//     [SerializeField] private GameObject magicProjectilePrefab;
//     [SerializeField] private float attackInterval = 3f;
//     [SerializeField] private float projectileSpeed = 5f;
    
//     private Transform _player;
//     private float _attackTimer = 0f;
//     private EnemyAttackManager _attackManager;
//     private Vector3 _baseScale;
    
//     void Start()
//     {
//         GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//         if (playerObj != null)
//         {
//             _player = playerObj.transform;
//         }
        
//         _attackManager = GetComponent<EnemyAttackManager>();
//         _attackTimer = attackInterval;
//         _baseScale = transform.localScale;
//     }
    
//     void Update()
//     {
//         if (_player == null) return;
        
//         // プレイヤーの方向を向く
//         Vector2 direction = _player.position - transform.position;
//         float sign = direction.x < 0 ? 1f : -1f;
//         transform.localScale = new Vector3(_baseScale.x * sign, _baseScale.y, _baseScale.z);
        
//         // 攻撃タイマー
//         _attackTimer -= Time.deltaTime;
//         if (_attackTimer <= 0f)
//         {
//             ShootMagic();
//             _attackTimer = attackInterval;
//         }
//     }
    
//     private void ShootMagic()
//     {
//         if (magicProjectilePrefab == null || _player == null) return;
        
//         // プレイヤーへの方向
//         Vector2 direction = (_player.position - transform.position).normalized;
        
//         // 魔法弾生成
//         GameObject projectile = Instantiate(magicProjectilePrefab, transform.position, Quaternion.identity);
        
//         // Enemyタグ設定
//         projectile.tag = "Enemy";
        
//         // EnemyAttackManagerを追加してダメージ設定
//         var projectileAttackManager = projectile.GetComponent<EnemyAttackManager>();
//         if (projectileAttackManager == null)
//         {
//             projectileAttackManager = projectile.AddComponent<EnemyAttackManager>();
//         }
        
//         // Rigidbody2Dで移動
//         var rb = projectile.GetComponent<Rigidbody2D>();
//         if (rb == null)
//         {
//             rb = projectile.AddComponent<Rigidbody2D>();
//         }
//         rb.gravityScale = 0f;
//         rb.linearVelocity = direction * projectileSpeed;
        
//         // 回転
//         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
//         projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
//         // 自動削除
//         Destroy(projectile, 5f);
        
//         Debug.Log("Wizard shot magic!");
//     }
// }