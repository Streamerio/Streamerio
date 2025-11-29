using Common;
using UnityEngine;

namespace InGame.Enemy.Helper
{
    /// <summary>
    /// 敵のランダムスポーン位置を計算するヘルパー
    /// </summary>
    public static class RandomSpawnPosition
    {
        /// <summary>
        /// ステージの地表を探し始めるY座標
        /// </summary>
        private const float _topY = 1000f;
        /// <summary>
        /// ステージの地表を探す方向
        /// </summary>
        private static readonly Vector2 _directionDown = new Vector2(0, -1);
        /// <summary>
        /// ステージのレイヤーマスク
        /// </summary>
        private static readonly int _layerMask = LayerMask.GetMask("Stage");
        
        /// <summary>
        /// ランダムなスポーン位置を取得する
        /// </summary>
        /// <param name="playerTransform"></param>
        /// <param name="masterData"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Vector2 Get(Transform playerTransform, MasterEnemyStatus masterData, float height)
        {
            // X座標をランダムに決定
            var randomPosX = Random.Range(masterData.MinSpawnPositionX, masterData.MaxSpawnPositionX);
            var posX = playerTransform.position.x + randomPosX;
            
            // 決定したX座標から地表のY座標をレイキャストで取得
            var origin = new Vector2(posX, _topY);
            var hit = Physics2D.Raycast(origin, _directionDown, Mathf.Infinity, _layerMask);

            // 地表が見つからなかった場合は探し始めた位置を返す
            if (hit.collider == null)
            {
                return origin;
            }
            
            //　地表からのY座標をランダムに決定
            var randomPosY = Random.Range(masterData.MinSpawnPositionY, masterData.MaxSpawnPositionY);
            var posY = hit.point.y + randomPosY + height/2;
            
            return new Vector2(posX, posY);
        }
    }
}