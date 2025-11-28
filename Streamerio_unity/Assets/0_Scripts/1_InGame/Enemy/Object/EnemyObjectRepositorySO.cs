using Common;
using UnityEngine;
using VContainer.Unity;

namespace InGame.Enemy.Object
{
    /// <summary>
    /// 敵のプレファブ管理用
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyObjectRepositorySO", menuName = "SO/Enemy/EnemyObjectRepository")]
    public class EnemyObjectRepositorySO: ScriptableObject, IEnemyObjectRepository
    {
        [SerializeField, Tooltip("敵オブジェクト辞書")]
        private SerializeDictionary<MasterEnemyType, LifetimeScope> _enemyObjectDictionary;
        
        /// <inheritdoc/>
        public LifetimeScope GetEnemyObject(MasterEnemyType type)
        {
            if (_enemyObjectDictionary.ContainsKey(type))
            {
                return _enemyObjectDictionary[type];
            }
            
            Debug.LogError($"EnemyObjectRepositorySO: Enemy Object for type {type} not found.");
            return null;
        }
    }
    
    public interface IEnemyObjectRepository
    {
        /// <summary>
        /// 敵のプレファブを取得する
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        LifetimeScope GetEnemyObject(MasterEnemyType type);
    }
}