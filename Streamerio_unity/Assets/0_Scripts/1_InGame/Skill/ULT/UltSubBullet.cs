using System.Collections.Generic;
using System.Threading;
using Common.Audio;
using R3;
using UnityEngine;

namespace InGame.Skill.ULT
{
    public class UltSubBullet: MonoBehaviour
    {
        [SerializeField] private float _speed = 20f;
        [SerializeField] private float _damage = 35f;
        [SerializeField] private float _initLifetime = 4f;
        [SerializeField] private int _bulletCount = 5;
        [SerializeField] private float _spreadAngle = 30f;
        [SerializeField] private float _continuousDamageInterval = 0.2f; // 持続ダメージ間隔(秒)
        [SerializeField] private float _continuousDamage = 10f; // 持続ダメージ量
    
        private Vector2 _direction;
        private Dictionary<GameObject, int> _enemyDamageCounters = new Dictionary<GameObject, int>();
        private int _damageIntervalFrames;
    
        private float _lifetime;
    
        private CancellationTokenSource _cts;
        
        public void OnCreate(Vector2 direction)
        {
            // フレームベースでインターバルを計算（子弾のみ）
            _damageIntervalFrames = Mathf.RoundToInt(_continuousDamageInterval / Time.fixedDeltaTime);
            
            _direction = direction;
        }
        
        public void Initialize()
        {
            gameObject.SetActive(true);
            Bind();
        }

        private void Bind()
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            _lifetime = _initLifetime;
        
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    Move();
                    if (_lifetime <= 0)
                    {
                        DestroySkill();
                    }
                    _lifetime -= Time.deltaTime;
                })
                .RegisterTo(_cts.Token);
        }
        
        private void Move()
        {
            transform.Translate(_direction * _speed * Time.deltaTime);
        }
        
        public void DestroySkill()
        {
            _cts.Cancel();
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.TryGetComponent<IDamageable>(out var enemy))
            {
                if (enemy != null)
                {
                    //Debug.Log($"UltBullet hit: {collision.gameObject.name}");
                    enemy.TakeDamage((int)_damage);
                    DestroySkill(); // 弾は敵に当たると消える
                }
            }
        }
    }
}