using System;
using Common;
using InGame.Enemy.Object;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace InGame.Enemy.Spawner
{
    public class EnemyPool: IDisposable
    {
        private readonly MasterEnemyStatus _enemyStatus;
        private readonly LifetimeScope _enemyPrefab;
        
        private readonly Transform _parent;
        private readonly LifetimeScope _parentScope;
        
        private readonly ObjectPool<IEnemy> _pool;
        
        public EnemyPool(MasterEnemyStatus enemyStatus, LifetimeScope enemyPrefab, Transform parent, LifetimeScope parentScope)
        {
            _enemyStatus = enemyStatus;
            _enemyPrefab = enemyPrefab;
            _parent = parent;
            _parentScope = parentScope;

            _pool = new ObjectPool<IEnemy>(
                createFunc: OnCreateSource,
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                defaultCapacity: enemyStatus.PoolSize
            );
        }
        
        public IEnemy Get()
        {
            return _pool.Get();
        }
        
        private IEnemy OnCreateSource()
        {
            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = UnityEngine.Object.Instantiate(_enemyPrefab, _parent);

                var enemy = instance.Container.Resolve<IEnemy>();

                var playerTr = _parentScope.Container.Resolve<Transform>("Player");
                enemy.OnCreate(_enemyStatus, playerTr);

                return enemy;
            }
        }


        private void OnGetEnemy(IEnemy enemy)
        {
            enemy.Initialize(() => _pool.Release(enemy));
        }
        
        private void OnReleaseEnemy(IEnemy enemy)
        {
            enemy.Disable();
        }
        
        public void Dispose()
        {
            _pool.Clear();
        }
    }
}