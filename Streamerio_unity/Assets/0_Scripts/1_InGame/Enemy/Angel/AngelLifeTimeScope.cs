using System;
using InGame.Enemy.Object;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// Angel プレハブ用 LifetimeScope: SO をスコープに登録しコンポーネントを登録します
/// </summary>
public class AngelLifeTimeScope : EnemyObjectLifetimeScope
{
    [SerializeField] private AngelScriptableObject config;
    public AngelScriptableObject Config => config;

    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterInstance<AngelScriptableObject>(config);
    }
}