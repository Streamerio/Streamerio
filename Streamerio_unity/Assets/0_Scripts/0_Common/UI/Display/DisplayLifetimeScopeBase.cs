// ============================================================================
// モジュール概要: Display 系 UI の LifetimeScope 共通処理をまとめ、View 登録と Presenter 配線を標準化する。
// 外部依存: VContainer。
// 使用例: 各 Display 専用 LifetimeScope が本クラスを継承し、CreateContext を実装するだけで DI 設定を共有する。
// ============================================================================

using VContainer;
using VContainer.Unity;

namespace Common.UI.Display
{
    /// <summary>
    /// 【目的】Display 向け LifetimeScope の共通 DI 登録フローを提供する基底クラス。
    /// 【理由】View 登録や Wiring 設定を派生クラスごとに重複させず、設定漏れを防ぐ。
    /// </summary>
    public abstract class DisplayLifetimeScopeBase<TDisplay, TPresenter, TView, TContext>: LifetimeScope
        where TDisplay : IDisplay, IAttachable<TContext>
        where TPresenter : TDisplay, IStartable
        where TView : IDisplayView
    {
        /// <summary>
        /// 【目的】共通の依存登録手順を実行し、派生クラスが追加登録を行えるようにする。
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            var view = GetComponent<TView>();
            builder.RegisterComponent(view)
                .As<TView>()
                .As<IInitializable>();
            
            BindPresenter(builder);
        }

        /// <summary>
        /// 【目的】Presenter の登録と Wiring を統一し、派生クラスでカスタマイズ可能にする。
        /// </summary>
        protected virtual void BindPresenter(IContainerBuilder builder)
        {
            builder.Register<TDisplay, TPresenter>(Lifetime.Singleton)
                .As<TDisplay>()
                .As<IStartable>();
            
            builder.RegisterEntryPoint<Wiring<TDisplay, TContext>>()
                .WithParameter(resolver => resolver.Resolve<TDisplay>())
                .WithParameter(CreateContext);
        }
        
        /// <summary>
        /// 【目的】Display Presenter に渡すコンテキストを生成する。
        /// </summary>
        protected abstract TContext CreateContext(IObjectResolver resolver);
    }
}
