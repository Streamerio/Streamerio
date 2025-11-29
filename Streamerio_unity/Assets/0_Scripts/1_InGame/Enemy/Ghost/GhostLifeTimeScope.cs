using System;
using InGame.Enemy;
using InGame.Enemy.GotoWalk;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using InGame.Enemy.Ghost;

public class GhostLifeTimeScope : LifetimeScope
{
    [SerializeField] private GhostScriptableObject config;
    public GhostScriptableObject Config => config;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<GhostScriptableObject>(config);
        builder.RegisterComponentInHierarchy<GhostMovement>();
        builder.RegisterComponentInHierarchy<EnemyHP>();
    }
}