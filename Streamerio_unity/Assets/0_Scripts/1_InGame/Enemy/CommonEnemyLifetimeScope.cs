using System;
using InGame.Enemy.Object;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Enemy
{
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

            builder.RegisterInstance<IEnemyObjectRepository>(_enemyObjectRepositorySO);

            builder.Register<IEnemySpawner, EnemySpawner>(Lifetime.Singleton)
                .WithParameter(_parent)
                .WithParameter(_parentScope)
                .As<IDisposable>();

            builder.RegisterEntryPoint<EnemyMediator>();
        }
    }
}