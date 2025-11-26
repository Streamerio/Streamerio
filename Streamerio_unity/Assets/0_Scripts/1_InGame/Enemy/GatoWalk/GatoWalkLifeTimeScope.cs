using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GatoWalkLifeTimeScope : LifetimeScope
{
    [SerializeField] private GatoWalkScriptableObject config;
    public GatoWalkScriptableObject Config => config;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<GatoWalkScriptableObject>(config);
        builder.RegisterComponentInHierarchy<GatoWalkMovement>();
        builder.RegisterComponentInHierarchy<EnemyHpManager>();
    }
}