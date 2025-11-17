using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;
public class SkeltonLifetimeScope : LifetimeScope
{
    [SerializeField] private SkeltonScriptableObject config;
    public SkeltonScriptableObject Config => config;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<SkeltonScriptableObject>(config);
        builder.RegisterComponentInHierarchy<Skelton>();
        builder.RegisterComponentInHierarchy<EnemyHpManager>();
    }
}