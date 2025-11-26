using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// BurningGhoul プレハブ用 LifetimeScope: SO をスコープに登録しコンポーネントを登録します
/// </summary>
public class BurningGhoulLifeTimeScope : LifetimeScope
{
    [SerializeField] private BurningGhoulScriptableObject config;
    public BurningGhoulScriptableObject Config => config;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<BurningGhoulScriptableObject>(config);
        builder.RegisterComponentInHierarchy<BurningGhoulMovement>();
        builder.RegisterComponentInHierarchy<EnemyHpManager>();
    }
}