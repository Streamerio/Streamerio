using System;
using System.Collections.Generic;
using Common;
using InGame.Enemy.Object;
using InGame.Enemy.Spawner;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Enemy
{
    public interface IEnemySpawner
    {
        IEnemy Spawn(MasterEnemyType type);
    }
    
    public class EnemySpawner: IEnemySpawner, IDisposable
    {
        private readonly Transform _parent;
        private readonly LifetimeScope _parentScope;
        
        private IEnemyObjectRepository _enemyObjectRepository;
        private IMasterData _masterData;
        
        private Dictionary<MasterEnemyType, EnemyPool> _enemyPoolDict = new();
        
        public EnemySpawner(Transform parent, LifetimeScope parentScope)
        {
            _parent = parent;
            _parentScope = parentScope;
        }
        
        [Inject]
        public void Construct(IEnemyObjectRepository enemyObjectRepository, IMasterData masterData)
        {
            _enemyObjectRepository = enemyObjectRepository;
            _masterData = masterData;
        }
        
        public IEnemy Spawn(MasterEnemyType type)
        {
            if (!_enemyPoolDict.ContainsKey(type))
            {
                _enemyPoolDict[type] = new EnemyPool(
                    _masterData.EnemyStatusDictionary[type],
                    _enemyObjectRepository.GetEnemyObject(type),
                    _parent,
                    _parentScope
                );
            }

            return _enemyPoolDict[type].Get();
        }
        
        public void Dispose()
        {
            foreach (var pool in _enemyPoolDict.Values)
            {
                pool.Dispose();
            }
            _enemyPoolDict.Clear();
        }
    }
}