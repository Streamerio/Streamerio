using UnityEngine;
using VContainer;
using VContainer.Unity;

public class BurningGhoulLifeTimeScope : LifetimeScope
{
    [SerializeField] private BurningGhoulScriptableObject _config;
    public BurningGhoulScriptableObject Config => _config;

    protected override void Configure(IContainerBuilder builder)
    {
        // Prefab に割当てた ScriptableObject を先に登録（Inject で必要）
        if (_config != null)
        {
            builder.RegisterInstance(_config);
        }
        builder.RegisterComponentInHierarchy<BurningGhoulMovement>().As<ITickable>().As<IStartable>();
        builder.RegisterComponentInHierarchy<EnemyHpManager>();
    }
}