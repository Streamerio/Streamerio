using System;
using System.Threading;
using InGame.Enemy;
using R3;
using UnityEngine;

namespace InGame.Enemy.GotoWalk
{
    /// <summary>
    /// 猫の動作
    /// </summary>
    public class GatoWalkMovement : EnemyMovementBase
    {
        [SerializeField] 
        private Rigidbody2D _rb;
    
        [SerializeField, Tooltip("ジャンプ力")]
        private float _jumpForce;
        [SerializeField, Tooltip("ジャンプ間隔(秒)")]
        private float _jumpInterval;
    
        [SerializeField, Tooltip("重力")]
        private float _gravityScale;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _rb ??= GetComponent<Rigidbody2D>();
            // Rigidbody2D の設定
            if (_rb != null)
            {
                _rb.gravityScale = _gravityScale;
                _rb.freezeRotation = true;
            }
        }
#endif
        
        protected override Vector2 GetMovePosition()
        {
            // 左方向に直進する
            return Transform.position - new Vector3(MoveSpeed, 0, 0) * Time.deltaTime;
        }

        protected override void Bind(CancellationToken ct)
        {
            base.Bind(ct);
        
            // 一定間隔でジャンプ
            Observable.Interval(TimeSpan.FromSeconds(_jumpInterval))
                .Subscribe(_ => Jump())
                .RegisterTo(ct);
        }

        private void Jump()
        {
            var vel = _rb.linearVelocity;
            vel.y = _jumpForce;
            _rb.linearVelocity = vel;
        }
    }
}