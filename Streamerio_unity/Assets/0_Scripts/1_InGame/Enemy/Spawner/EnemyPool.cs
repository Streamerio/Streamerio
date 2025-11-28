using System;
using Common;
using InGame.Enemy.Object;
using UnityEngine;
using UnityEngine.Pool;
using VContainer;
using VContainer.Unity;

namespace InGame.Enemy.Spawner
{
    /// <summary>
    /// 敵のオブジェクトプール
    /// </summary>
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
                createFunc: OnCreateEnemy,
                actionOnGet: OnGetEnemy,
                actionOnRelease: OnReleaseEnemy,
                defaultCapacity: enemyStatus.PoolSize
            );
        }
        
        /// <summary>
        /// 敵の取得
        /// </summary>
        /// <returns></returns>
        public IEnemy Get()
        {
            return _pool.Get();
        }
        
        /// <summary>
        /// オブジェクトプールで生成するときの処理
        /// </summary>
        /// <returns></returns>
        private IEnemy OnCreateEnemy()
        {
            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                // プレハブをインスタンス化
                var instance = UnityEngine.Object.Instantiate(_enemyPrefab, _parent);

                // インスタンスのコンテナからIEnemyを取得し初期化
                var enemy = instance.Container.Resolve<IEnemy>();
                var playerTr = _parentScope.Container.Resolve<Transform>("Player");
                enemy.OnCreate(_enemyStatus, playerTr);

                return enemy;
            }
        }

        /// <summary>
        /// オブジェクトプールから取得したときの処理
        /// </summary>
        /// <param name="enemy"></param>
        private void OnGetEnemy(IEnemy enemy)
        {
            enemy.Initialize(() => _pool.Release(enemy));
        }
        
        /// <summary>
        /// オブジェクトプールに返却されたときの処理
        /// </summary>
        /// <param name="enemy"></param>
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