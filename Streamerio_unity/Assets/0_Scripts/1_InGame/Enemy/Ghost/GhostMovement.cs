using UnityEngine;
using VContainer;

namespace InGame.Enemy.Ghost
{
    public class GhostMovement : EnemyMovementBase
    {
        private Transform _player;

        [Inject]
        public void Construct([Key("Player")]Transform player)
        {
            _player = player;
        }

        protected override Vector2 GetMovePosition()
        {
            return MoveTowardsPlayer();
        }

        private Vector2 MoveTowardsPlayer()
        {
            // プレイヤーの方向を計算
            Vector2 direction = (_player.transform.position - transform.position).normalized;

            // プレイヤーに向かって移動
            transform.position += (Vector3)(direction * MoveSpeed * Time.deltaTime);

            // スプライトの向きを調整
            if (direction.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }   
            else
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }

            return transform.position;
        }
    }
}