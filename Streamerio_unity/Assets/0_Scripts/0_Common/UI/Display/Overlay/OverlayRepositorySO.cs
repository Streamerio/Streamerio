// ============================================================================
// モジュール概要: オーバーレイ UI の Presenter ↔ LifetimeScope 対応表を維持し、DisplayRepositorySOBase を実装する。
// 外部依存: UnityEngine (ScriptableObject)。
// 使用例: Overlay 用 LifetimeScope を登録し、OverlaySpawner が正しいプレハブを引き当てる。
// ============================================================================

using System;
using System.Collections.Generic;
using OutGame.GameOver.UI;
using UnityEngine;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// オーバーレイ UI の型マッピングを提供するリポジトリ。
    /// </summary>
    [CreateAssetMenu(fileName = "OverlayRepository", menuName = "SO/UI/OverlayRepository")]
    public class OverlayRepositorySO : DisplayRepositorySOBase
    {
        protected override Dictionary<Type, Type> CreateTypeMap()
        {
            return new()
            {
                {typeof(IGameOverOverlay), typeof(GameOverOverlayLifetimeScope)},
            };
        }
    }
}
