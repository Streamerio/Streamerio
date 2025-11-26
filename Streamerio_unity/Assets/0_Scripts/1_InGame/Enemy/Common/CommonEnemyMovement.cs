using UnityEngine;

namespace InGame.Enemy.Common
{
    public class CommonEnemyMovement: EnemyMovementBase
    {
        protected override Vector2 GetMovePosition()
        {
            return Transform.position - new Vector3(MoveSpeed, 0, 0) * Time.deltaTime;
        }
    }
}