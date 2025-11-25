using System;
using System.Collections.Generic;
using Common.UI.Display.Window.Book;
using OutGame.Result.UI;
using UnityEngine;

namespace Common.UI.Display.Window
{
    /// <summary>
    /// Window 用のリポジトリ ScriptableObject。
    /// - プロジェクト内に存在する WindowPresenterBase 派生クラスを保持する
    /// - DisplayManager/DisplayService がここから Window を生成・管理する
    /// - Unity メニューから「SO/UI/WindowRepository」として作成可能
    /// </summary>
    [CreateAssetMenu(fileName = "WindowRepository", menuName = "SO/UI/WindowRepository")]
    public class WindowRepositorySO : DisplayRepositorySOBase
    {
        protected override Dictionary<Type, Type> CreateTypeMap()
        {
            return new Dictionary<Type, Type>
            {
                {typeof(IBookWindow), typeof(BookWindowLifetimeScope)},
                {typeof(IResultWindow), typeof(ResultWindowLifetimeScope)},
            };
        }
    }
}