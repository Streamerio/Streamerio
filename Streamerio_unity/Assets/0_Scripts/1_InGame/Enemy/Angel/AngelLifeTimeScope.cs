using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Angel プレハブ用 LifetimeScope: SO をスコープに登録しコンポーネントを登録します
/// </summary>
public class AngelLifeTimeScope : LifetimeScope
{
    [SerializeField] private AngelScriptableObject config;
    public AngelScriptableObject Config => config;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterInstance<AngelScriptableObject>(config);
        builder.RegisterComponentInHierarchy<AngelMovement>();
        builder.RegisterComponentInHierarchy<EnemyHpManager>();
    }
}