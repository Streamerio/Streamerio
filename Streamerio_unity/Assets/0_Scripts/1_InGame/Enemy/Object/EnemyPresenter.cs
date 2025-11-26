using System;
using System.Threading;
using Common;
using R3;
using UnityEngine;
using VContainer;

namespace InGame.Enemy.Object
{
    public interface IEnemy
    {
        float AttackPower { get; }
        
        void OnCreate(MasterEnemyStatus status, Transform playerTransform);
        void Initialize(Action onRelease);
        void TakeDamage(int damage);
        void Disable();
    }
    
    public class EnemyPresenter: MonoBehaviour, IEnemy, IDisposable
    {
        [SerializeField, Tooltip("自身のGameObject")]
        private GameObject _gameObject;
        [SerializeField, Tooltip("高さ")]
        private float _height;
        
        private MasterEnemyStatus _status;
        
        private IEnemyMovement _movement;
        private IDamageable _hp;
        
        private Transform _playerTransform;
        
        private Action _onRelease;
        
        private CancellationTokenSource _cts;

        public float AttackPower => _status.AttackPower;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _gameObject ??= gameObject;
            if (_height <= 0)
            {
                var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    _height = spriteRenderer.bounds.size.y;
                }
            }
        }
#endif
        
        [Inject]
        public void Construct(IEnemyMovement movement, IDamageable hp)
        {
            _movement = movement;
            _hp = hp;
        }

        public void OnCreate(MasterEnemyStatus status, Transform playerTransform)
        {
            _status = status;
            _playerTransform = playerTransform;
        }
        
        public void Initialize(Action onRelease)
        {
            _onRelease = onRelease;
            _cts = new CancellationTokenSource();
            
            _movement.Initialize(RandomSpawnPosition.Get(_playerTransform, _status, _height), _status.Speed);
            _hp.Initialize(_status.HP);
            
            Bind();
            
            _gameObject.SetActive(true);
            _movement.MoveStart();
        }

        private void Bind()
        {
            _movement.PositionProp
                .Select(position => position.x)
                .Where(IsAbleRange)
                .Subscribe(_ =>
                {
                    _onRelease?.Invoke();
                })
                .RegisterTo(_cts.Token);
            
            _hp.IsDeadProp
                .DistinctUntilChanged()
                .Where(isDead => isDead)
                .Subscribe(_ =>
                {
                    _onRelease?.Invoke();
                })
                .RegisterTo(_cts.Token);
        }
     
        public void TakeDamage(int damage)
        {
            _hp.TakeDamage(damage);
        }

        public void Disable()
        {
            _cts.Cancel();
            _movement.MoveStop();
            
            _gameObject.SetActive(false);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            
            Destroy(_gameObject);
        }
        
        private bool IsAbleRange(float positionX)
        {
            return Math.Abs(positionX - _playerTransform.position.x) > _status.AblePlayerDistance;
        }
    }
}