using System;
using System.Threading;
using R3;
using UnityEngine;

namespace InGame.Enemy
{
    public interface IEnemyMovement
    {
        ReadOnlyReactiveProperty<Vector2> PositionProp { get; }
        
        void Initialize(Vector2 initialPosition, float moveSpeed);
        void MoveStart();
        void MoveStop();
    }
    
    public abstract class EnemyMovementBase: MonoBehaviour, IEnemyMovement, IDisposable
    {
        [SerializeField, Tooltip("移動対象のTransform")]
        private Transform _transform;
        protected Transform Transform => _transform;
        
        protected float MoveSpeed;
        
        private ReactiveProperty<Vector2> _positionProp = new();
        public ReadOnlyReactiveProperty<Vector2> PositionProp => _positionProp;
        public Vector2 CurrentPosition => _positionProp.Value;
        
        private CancellationTokenSource _cts = new ();

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _transform ??= transform;
        }
#endif
        
        public virtual void Initialize(Vector2 initialPosition, float moveSpeed)
        {
            _positionProp.Value = initialPosition;
            _transform.position = initialPosition;
            
            MoveSpeed = moveSpeed;
        }
        
        public virtual void MoveStart()
        {
            Bind(_cts.Token);
        }

        protected virtual void Bind(CancellationToken ct)
        {
            Observable.EveryUpdate()
                .Subscribe(_ =>
                {
                    _positionProp.Value = GetMovePosition();
                    _transform.position = _positionProp.Value;
                })
                .RegisterTo(ct);
        }
        
        protected abstract Vector2 GetMovePosition();
        
        public virtual void MoveStop()
        {
            _cts.Cancel();
        }
        
        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            
            _positionProp?.Dispose();
        }
    }
}