// ============================================================================
// モジュール概要: オーバーレイ UI の Presenter 基盤とコンテキストを提供し、DisplayPresenterBase を拡張する。
// 外部依存: DisplayPresenterBase。
// 使用例: 各オーバーレイ Presenter が OverlayPresenterBase を継承し、共通の Attach 処理を再利用する。
// ============================================================================

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// オーバーレイ UI 共通の契約。
    /// </summary>
    public interface IOverlay : IDisplay
    {
        
    }
    
    /// <summary>
    /// Overlay 系 Display の基盤 Presenter。
    /// - OverlayViewBase を制御対象にする
    /// - クリックイベントを購読し、効果音を再生＆通知する
    /// - Show/Hide のタイミングでイベント購読を管理（生成/解放）
    /// </summary>
    public class OverlayPresenterBase<TView, TContext> : DisplayPresenterBase<TView, TContext>
        where TView : IOverlayView
        where TContext : OverlayContext<TView>
    {
        /// <summary>
        /// 【目的】コンテキストから View を受け取り、基底クラスの View フィールドへ割り当てる。
        /// </summary>
        protected override void AttachContext(TContext context)
        {
            View = context.View;
        }
    }
    
    /// <summary>
    /// オーバーレイ Presenter へ渡す共通コンテキスト。
    /// </summary>
    public class OverlayContext<TView>
        where TView : IOverlayView
    {
        /// <summary>
        /// 【目的】Presenter が操作対象とする View を保持する。
        /// </summary>
        public TView View;
    }
}
