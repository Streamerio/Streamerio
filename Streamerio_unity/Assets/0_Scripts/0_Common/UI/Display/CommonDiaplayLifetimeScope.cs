// モジュール概要:
// Display レイヤー向けの VContainer LifetimeScope を構成し、IDialogService を起動時に準備する。
// 依存関係: DisplayRepositorySOBase から UI プレハブを解決し、DisplaySpawner/DisplayCache と連携させる。
// 使用例: UI ルートに本スコープを配置し、Dialog 系画面を開閉するインフラを提供する。

using System;
using Common.UI.Dialog;
using Common.UI.Display.Overlay;
using Common.UI.Display.Window;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display
{
    /// <summary>
    /// 【目的】Display 系サービスを起動するための DI コンテナ構成を担う。
    /// 【理由】UI プレハブの生成とキャッシュ化をまとめ、IDialogService を一貫した方法で提供するため。
    /// </summary>
    public class CommonDiaplayLifetimeScope : LifetimeScope
    {
        /// <summary>
        /// 【目的】UI プレハブ解決に利用するリポジトリを Inspector で差し替え可能に保持する。
        /// 【理由】テスト用や開発中のリポジトリを容易に切り替え、生成対象を柔軟に管理するため。
        /// </summary>
        [SerializeField]
        private DisplayServiceData _windowServiceData;
        [SerializeField]
        [Tooltip("オーバーレイ UI を生成するためのリポジトリアセットと親 Transform。")]
        private DisplayServiceData _overlayServiceData;
        [SerializeField]
        [Tooltip("ダイアログ UI を生成するためのリポジトリアセットと親 Transform。")]
        private DisplayServiceData _dialogServiceData;

        /// <summary>
        /// 【目的】DI コンテナへ DisplayServiceContext を登録し、IDialogService のエントリポイントを起動する。
        /// 【処理概要】DisplaySpawner を構築→DisplayCache に包む→Wiring の WithParameter でコンテキストを注入。
        /// 【理由】UI ツリー配下に生成した LifetimeScope を親スコープに紐づけ、Presenter の依存解決を可能にするため。
        /// </summary>
        /// <param name="builder">【用途】依存登録を組み立てる VContainer のビルダー。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder
                .RegisterEntryPoint<Wiring<IWindowService, DisplayServiceContext>>()
                .WithParameter(resolver => CreateContext(_windowServiceData));
            
            builder
                .RegisterEntryPoint<Wiring<IOverlayService, DisplayServiceContext>>()
                .WithParameter(resolver => CreateContext(_overlayServiceData));
            
            builder
                .RegisterEntryPoint<Wiring<IDialogService, DisplayServiceContext>>()
                .WithParameter(resolver => CreateContext(_dialogServiceData));
        }
        
        private DisplayServiceContext CreateContext(DisplayServiceData data)
        {
            var spawner = new DisplayFactory(data.Repository, data.ParentTransform, this);

            return new DisplayServiceContext
            {
                Cache = new DisplayCache(spawner)
            };
        }

        [Serializable]
        private class DisplayServiceData
        {
            /// <summary>
            /// 【目的】生成先の Presenter を解決するリポジトリアセット。
            /// </summary>
            public DisplayRepositorySOBase Repository;
            /// <summary>
            /// 【目的】生成した LifetimeScope をぶら下げる Transform。
            /// </summary>
            public Transform ParentTransform;
        }
    }
}
