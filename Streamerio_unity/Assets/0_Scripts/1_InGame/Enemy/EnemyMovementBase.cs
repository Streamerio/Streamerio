using System;
using System.Threading;
using R3;
using UnityEngine;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵の動作インターフェース
    /// </summary>
    public interface IEnemyMovement
    {
        /// <summary>
        /// 現在地
        /// </summary>
        ReadOnlyReactiveProperty<Vector2> PositionProp { get; }
        
        /// <summary>
        /// 初期化(使用前に呼ぶ)
        /// </summary>
        /// <param name="initialPosition"></param>
        /// <param name="moveSpeed"></param>
        void Initialize(Vector2 initialPosition, float moveSpeed);
        /// <summary>
        /// 移動開始
        /// </summary>
        void MoveStart();
        /// <summary>
        /// 移動を止める
        /// </summary>
        void MoveStop();
    }
    
    /// <summary>
    /// 敵の移動の基底クラス
    /// </summary>
    public abstract class EnemyMovementBase: MonoBehaviour, IEnemyMovement, IDisposable
    {
        [SerializeField, Tooltip("移動対象のTransform")]
        private Transform _transform;
        protected Transform Transform => _transform;
        
        protected float MoveSpeed;
        
        private ReactiveProperty<Vector2> _positionProp = new();
        /// <inheritdoc/>
        public ReadOnlyReactiveProperty<Vector2> PositionProp => _positionProp;
        
        private CancellationTokenSource _cts = new ();

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _transform ??= transform;
        }
#endif
        
        /// <inheritdoc/>
        public virtual void Initialize(Vector2 initialPosition, float moveSpeed)
        {
            _positionProp.Value = initialPosition;
            _transform.position = initialPosition;
            
            MoveSpeed = moveSpeed;
        }
        
        /// <inheritdoc/>
        public virtual void MoveStart()
        {
            Bind(_cts.Token);
        }

        /// <summary>
        /// 移動処理のバインド
        /// </summary>
        /// <param name="ct"></param>
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
        
        /// <summary>
        /// 移動量を取得する
        /// </summary>
        /// <returns></returns>
        protected abstract Vector2 GetMovePosition();
        
        /// <inheritdoc/>
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