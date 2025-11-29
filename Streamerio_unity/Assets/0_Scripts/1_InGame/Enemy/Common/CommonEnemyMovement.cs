using UnityEngine;

namespace InGame.Enemy.Common
{
    public class CommonEnemyMovement: EnemyMovementBase
    {
        protected override Vector2 GetMovePosition()
        {
            // 左方向に直進する
            return Transform.position - new Vector3(MoveSpeed, 0, 0) * Time.deltaTime;
        }
    }
}