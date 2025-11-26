using Common;
using UnityEngine;
using VContainer.Unity;

namespace InGame.Enemy.Object
{
    [CreateAssetMenu(fileName = "EnemyObjectRepositorySO", menuName = "SO/Enemy/EnemyObjectRepository")]
    public class EnemyObjectRepositorySO: ScriptableObject, IEnemyObjectRepository
    {
        [SerializeField]
        private SerializeDictionary<MasterEnemyType, LifetimeScope> _enemyObjectDictionary;
        
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
        LifetimeScope GetEnemyObject(MasterEnemyType type);
    }
}