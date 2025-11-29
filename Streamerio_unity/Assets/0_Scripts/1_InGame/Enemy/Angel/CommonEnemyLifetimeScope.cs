using System;
using InGame.Enemy.Object;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Enemy
{
    /// <summary>
    /// 敵の共通LifetimeScope: EnemySpawnerとEnemyObjectRepositoryを登録します
    /// </summary>
    public class CommonEnemyLifetimeScope: LifetimeScope
    {
        [SerializeField, Tooltip("Enemyオブジェクトリポジトリ")]
        private EnemyObjectRepositorySO _enemyObjectRepositorySO;
        [SerializeField, Tooltip("Enemyの親オブジェクト")]
        private Transform _parent;
        [SerializeField, Tooltip("EnemySpawnerの親スコープ")]
        private LifetimeScope _parentScope;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            // EnemyObjectRepositoryを登録
            builder.RegisterInstance<IEnemyObjectRepository>(_enemyObjectRepositorySO);

            // EnemySpawnerを登録
            builder.Register<IEnemySpawner, EnemySpawner>(Lifetime.Singleton)
                .WithParameter(_parent)
                .WithParameter(_parentScope)
                .As<IDisposable>();

            // EnemyMediatorを登録
            builder.RegisterEntryPoint<EnemyMediator>();
        }
    }
}