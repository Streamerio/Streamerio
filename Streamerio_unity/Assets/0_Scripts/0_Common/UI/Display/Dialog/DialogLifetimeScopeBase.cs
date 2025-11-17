// モジュール概要:
// ダイアログ Presenter 用の VContainer LifetimeScope を構築するための基底クラス。
// 依存関係: DisplayBackgroundPresenter と CommonButtonPresenter を子スコープに配置し、ICommonDialogView をコンポーネントから取得して登録する。
// 使用例: 個別のダイアログ LifetimeScope が本基底を継承し、Presenter とコンテキスト生成を追加登録する。

using Common.UI.Display;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Dialog
{
    /// <summary>
    /// 【目的】ダイアログ Presenter を起動するための共通 LifetimeScope 構成を提供する。
    /// 【理由】背景や閉じるボタンの依存登録を各ダイアログで重複記述しないようにするため。
    /// </summary>
    public abstract class DialogLifetimeScopeBase<TDialog, TPresenter, TView, TContext> : DisplayLifetimeScopeBase<TDialog, TPresenter, TView, TContext>
        where TDialog : IDialog, IAttachable<TContext>
        where TPresenter : TDialog, IStartable
        where TView : IDialogView
        where TContext: DialogContext<TView>
    {
        /// <summary>
        /// 【目的】ダイアログで共通的に必要となる依存登録を実行する。
        /// 【処理概要】コンポーネントから View を取得して登録し、背景 Presenter と共通ボタン Presenter をシングルトンとして登録する。
        /// 【理由】Presenter 登録前に必要なインフラ依存を揃えることで、派生クラスが Presenter 登録のみに集中できるようにする。
        /// </summary>
        /// <param name="builder">【用途】依存登録とコンポーネント登録を行う VContainer のコンテナビルダー。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IDisplayBackground, DisplayBackgroundPresenter>(Lifetime.Singleton);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Close);
            
            base.Configure(builder);
        }
    }
}
