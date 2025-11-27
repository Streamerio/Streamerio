using System;
using System.Threading;
using Common;
using InGame.Enemy.Helper;
using R3;
using UnityEngine;
using VContainer;

namespace InGame.Enemy.Object
{
    /// <summary>
    /// 敵オブジェクトのインターフェース
    /// </summary>
    public interface IEnemy: IAttackable, IDamageable
    {
        /// <summary>
        /// オブジェクト生成時の初期化(生成直後に1回だけ呼ばれる)
        /// </summary>
        /// <param name="status"></param>
        /// <param name="playerTransform"></param>
        void OnCreate(MasterEnemyStatus status, Transform playerTransform);
        /// <summary>
        /// 初期化処理(使いたい時に呼ばれる)
        /// </summary>
        /// <param name="onRelease"></param>
        void Initialize(Action onRelease);
        /// <summary>
        /// 無効化処理(使い終わった時に呼ばれる)
        /// </summary>
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
        private IEnemyHP _hp;
        
        private Transform _playerTransform;
        
        private Action _onRelease;
        
        private CancellationTokenSource _cts;

        public float Power => _status.AttackPower;

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
        public void Construct(IEnemyMovement movement, IEnemyHP hp)
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
     
        public void TakeDamage(float damage)
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