using System.Threading;
using Cysharp.Threading.Tasks;
using InGame.Enemy.Skelton;
using UnityEngine;

namespace InGame.Enemy.Skeleton
{
    [RequireComponent(typeof(SkeletonAnimation))]
    public class SkeletonMovement: EnemyMovementBase
    {
        [SerializeField, Tooltip("スケルトンのアニメーション")]
        private SkeletonAnimation _animation;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _animation ??= GetComponent<SkeletonAnimation>();
        }
        
#endif
        
        protected override Vector2 GetMovePosition()
        {
            // 左方向に直進する
            return Transform.position - new Vector3(MoveSpeed, 0, 0) * Time.deltaTime;
        }
        
        protected override async UniTask WaitMoveAsync(CancellationToken ct)
        {
            await UniTask.WaitUntil(() => _animation.IsEndBornAnim, cancellationToken: ct);
        }
    }
}