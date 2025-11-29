// モジュール概要:
// ダイアログ Presenter 用の VContainer LifetimeScope を構築するための基底クラス。
// 依存関係: DisplayBackgroundPresenter と CommonButtonPresenter を子スコープに配置し、ICommonDialogView をコンポーネントから取得して登録する。
// 使用例: 個別のダイアログ LifetimeScope が本基底を継承し、Presenter とコンテキスト生成を追加登録する。

using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using UnityEngine;
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
        [SerializeField]
        private RectTransform _moveRectTransform;
        /// <summary>
        /// 【目的】ダイアログ表示時の移動経路と補間設定を ScriptableObject から受け取る。
        /// 【理由】演出パラメータを Play しやすい形で保持し、デザイナーがパラメータ差し替えできるようにするため。
        /// </summary>
        [SerializeField, LabelText("表示アニメーション")]
        [Tooltip("表示時に辿る PathAnimation 設定。")]
        private PathAnimationParamSO _showAnimationParam;
        /// <summary>
        /// 【目的】ダイアログ非表示時の移動経路と補間設定を保持する。
        /// 【理由】表示と対になる演出を同一ファイルで管理し、挙動差異が発生しないようにするため。
        /// </summary>
        [SerializeField, LabelText("非表示アニメーション")]
        [Tooltip("非表示時に辿る PathAnimation 設定。")]
        private PathAnimationParamSO _hideAnimationParam;
        
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
            
            builder
                .RegisterInstance<IUIAnimation>(new PathAnimation(_moveRectTransform, _showAnimationParam))
                .Keyed(AnimationType.Show);
            builder
                .RegisterInstance<IUIAnimation>(new PathAnimation(_moveRectTransform, _hideAnimationParam))
                .Keyed(AnimationType.Hide);
            
            base.Configure(builder);
        }
    }
}
