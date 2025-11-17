using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Common.Audio;
using VContainer;

public class UltBullet : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _damage = 35f;
    [SerializeField] private float _lifetime = 4f;
    [SerializeField] private int _bulletCount = 5;
    [SerializeField] private float _spreadAngle = 30f;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _continuousDamageInterval = 0.2f; // 持続ダメージ間隔(秒)
    [SerializeField] private float _continuousDamage = 10f; // 持続ダメージ量
    
    private Vector2 _direction;
    private bool _isMainBullet = true;
    private Dictionary<GameObject, int> _enemyDamageCounters = new Dictionary<GameObject, int>();
    private int _damageIntervalFrames;
    private GameObject _player;
    
    private IAudioFacade _audioFacade;
    
    [Inject]
    public void Construct(IAudioFacade audioFacade)
    {
        _audioFacade = audioFacade;
    }

    void Awake()
    {
        _player = GameObject.FindWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player object not found in the scene.");
        }
    }

    void Start()
    {
        if (_player != null)
        {
            //playerのy座標から8マス右側に生成
            transform.position = new Vector2(_player.transform.position.x + 2f, _player.transform.position.y);
        }
        if (_isMainBullet)
        {
            CreateBulletSpread();
        }
        else
        {
            // フレームベースでインターバルを計算（子弾のみ）
            _damageIntervalFrames = Mathf.RoundToInt(_continuousDamageInterval / Time.fixedDeltaTime);
        }
        
        //_audioFacade.PlayAsync(SEType.ThunderBullet, this.GetCancellationTokenOnDestroy()).Forget();
    }

    void Update()
    {
        Move();
        if (_lifetime <= 0)
        {
            DestroySkill();
        }
        _lifetime -= Time.deltaTime;
    }

    private void CreateBulletSpread()
    {
        float angleStep = _spreadAngle / (_bulletCount - 1);
        float startAngle = -_spreadAngle / 2;

        for (int i = 0; i < _bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 direction = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            );

            GameObject bullet = Instantiate(gameObject, transform.position, Quaternion.identity);
            UltBullet bulletScript = bullet.GetComponent<UltBullet>();
            bulletScript._isMainBullet = false;
            bulletScript._direction = direction;
        }

        // メイン弾は削除
        Destroy(gameObject);
    }

    private void Move()
    {
        if (!_isMainBullet)
        {
            transform.Translate(_direction * _speed * Time.deltaTime);
        }
    }

    public void DestroySkill()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyHpManager>();
            if (enemy != null)
            {
                //Debug.Log($"UltBullet hit: {collision.gameObject.name}");
                enemy.TakeDamage((int)_damage);
                DestroySkill(); // 弾は敵に当たると消える
            }
        }
    }
}