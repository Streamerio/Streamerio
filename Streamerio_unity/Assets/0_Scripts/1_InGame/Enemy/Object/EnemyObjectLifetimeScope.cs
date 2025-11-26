using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Enemy.Object
{
    [RequireComponent(typeof(EnemyPresenter))]
    public class EnemyObjectLifetimeScope: LifetimeScope
    {
        [SerializeField, Tooltip("EnemyのMovement")]
        private EnemyMovementBase _movement;
        [SerializeField, Tooltip("EnemyのPresenter")]
        private EnemyPresenter _enemyPresenter;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _movement ??= GetComponent<EnemyMovementBase>();
            _enemyPresenter ??= GetComponent<EnemyPresenter>();
        }
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent<IEnemyMovement>(_movement);

            builder.Register<IDamageable, EnemyHP>(Lifetime.Singleton);

            builder.RegisterComponent<IEnemy>(_enemyPresenter);
        }
    }
}