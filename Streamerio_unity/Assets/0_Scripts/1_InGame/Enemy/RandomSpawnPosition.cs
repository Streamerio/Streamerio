using Common;
using UnityEngine;

namespace InGame.Enemy
{
    public static class RandomSpawnPosition
    {
        private const float _topY = 1000f;
        private static readonly Vector2 _directionDown = new Vector2(0, -1);
        private static readonly int _layerMask = LayerMask.GetMask("Stage");
        
        public static Vector2 Get(Transform playerTransform, MasterEnemyStatus masterData, float height)
        {
            var randomPosX = Random.Range(masterData.MinSpawnPositionX, masterData.MaxSpawnPositionX);
            var origin = new Vector2(playerTransform.position.x + randomPosX, _topY);
            var hit = Physics2D.Raycast(origin, _directionDown, Mathf.Infinity, _layerMask);

            if (hit.collider == null)
            {
                return origin;
            }
            
            var randomPosY = Random.Range(masterData.MinSpawnPositionY, masterData.MaxSpawnPositionY);
            return new Vector2(origin.x, hit.point.y + randomPosY + height/2);
        }
    }
}