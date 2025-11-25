// モジュール概要:
// 背景クリック用 Presenter を提供し、ダイアログなどの外側タップ処理を共通化する。
// 依存関係: IClickEventBinder でクリック検出を委譲し、DisplayPresenterBase を継承して表示制御を統合。

using Common.UI.Click;
using R3;

namespace Common.UI.Display.Background
{
    public interface IDisplayBackground : IDisplay, IAttachable<DisplayBackgroundContext>
    {
        Observable<Unit> OnClickAsObservable { get; }
    }
    
    /// <summary>
    /// 【目的】背景の表示制御とクリックイベント配信を担当する。
    /// 【理由】各ダイアログで同様の処理を重複記述せず、共通 Presenter として共有するため。
    /// </summary>
    public class DisplayBackgroundPresenter : DisplayPresenterBase<IDisplayBackgroundView, DisplayBackgroundContext>, IDisplayBackground
    {
        /// <summary>
        /// 【目的】背景クリック検出を担うバインダーを保持する。
        /// 【理由】AttachContext 以降もクリックイベントへアクセスし、Detach で確実に破棄するため。
        /// </summary>
        private IClickEventBinder _clickEventBinder;

        /// <summary>
        /// 【目的】背景クリックを購読可能なストリームとして公開する。
        /// 【理由】外側タップで閉じるなどの UI 操作を他 Presenter から扱いやすくするため。
        /// </summary>
        public Observable<Unit> OnClickAsObservable => _clickEventBinder.ClickEvent;

        /// <summary>
        /// 【目的】コンテキストから View と ClickBinder を取得し、Presenter の内部状態を構築する。
        /// 【処理概要】CommonView のセット→Binder の保持→クリックイベントのバインド。
        /// 【理由】Attach 時に依存が揃っていることを保証し、Start 以降の購読漏れを防ぐ。
        /// </summary>
        /// <param name="context">【用途】View と ClickBinder を束ねたコンテキスト。</param>
        protected override void AttachContext(DisplayBackgroundContext context)
        {
            View = context.View;
            _clickEventBinder = context.ClickEventBinder;
            _clickEventBinder.BindClickEvent();
        }

        /// <summary>
        /// 【目的】Presenter 終了時にクリック購読を解除し、リソースを解放する。
        /// 【理由】背景が破棄された後もイベントが飛ばないようにし、メモリリークを回避する。
        /// </summary>
        public override void Detach()
        {
            base.Detach();
            _clickEventBinder.Dispose();
        }
    }

    /// <summary>
    /// 【目的】背景 Presenter が必要とする依存（View と ClickBinder）を束ねる。
    /// 【理由】LifetimeScope からまとめて渡せるようにし、初期化コードを簡潔にする。
    /// </summary>
    public class DisplayBackgroundContext
    {
        /// <summary>
        /// 【目的】背景の表示/非表示を担当する View を提供する。
        /// 【理由】Presenter が DisplayViewBase の共通 API を利用できるようにするため。
        /// </summary>
        public IDisplayBackgroundView View;

        /// <summary>
        /// 【目的】クリック検知と付随処理（SE 再生など）を担う Binder を提供する。
        /// 【理由】背景クリック時の振る舞いを Presenter から切り離し、責務を明確にするため。
        /// </summary>
        public IClickEventBinder ClickEventBinder;
    }
}
