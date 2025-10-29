using UnityEngine;
using Cysharp.Threading.Tasks;

public class NormalBullet : MonoBehaviour
{
    private float _speed;
    private float _damage;

    public float Damage => _damage;
    [SerializeField] private BulletScriptableObject _bulletScriptableObject;
    void Awake()
    {
        _speed = _bulletScriptableObject.Speed;
        _damage = _bulletScriptableObject.Damage;
    }
    void Update()
    {
        Move();
    }
    async void OnEnable()
    {
        await AutoDespawn();
    }

    public void OnSpawn()
    {
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    private async UniTask AutoDespawn()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(_bulletScriptableObject.Lifetime));
        OnDespawn();
    }
    private void Move()
    {
        transform.Translate(Vector2.right * _speed * Time.deltaTime);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Hit Enemy");
            var enemy = collision.gameObject.GetComponent<EnemyHpManager>();
            if (enemy != null)
            {
                enemy.TakeDamage((int)_damage);
                OnDespawn();
            }
        }
    }
}