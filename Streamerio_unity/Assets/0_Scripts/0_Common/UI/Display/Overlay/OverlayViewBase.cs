// ============================================================================
// モジュール概要: Overlay UI の基底 View 実装を提供し、フェード演出やパーツ制御を共通化する。
// 外部依存: Alchemy.Inspector、Cysharp.Threading.Tasks、Common.UI.Animation、VContainer。
// 使用例: OverlayViewBase を継承して固有演出を追加しつつ、共通のフェード/パーツ制御を再利用する。
// ============================================================================

using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using System.Threading;
using Common.UI.Display.Background;
using VContainer;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// オーバーレイ UI の View 契約。
    /// <para>
    /// 【理由】Presenter が OverlayViewBase に依存しなくても操作できるよう抽象化する。
    /// </para>
    /// </summary>
    public interface IOverlayView : IDisplayView
    {
        IDisplayBackground Background { get; }
    }
    
    /// <summary>
    /// Overlay 系 UI の見た目をフェードアニメーションで制御する基底クラス。
    /// - 複数のパーツを順番にフェード表示
    /// - 非表示は CanvasGroup をまとめてフェード
    /// - 即時表示/非表示も可能
    /// </summary>
    public abstract class OverlayViewBase : DisplayViewBase, IOverlayView
    {
        private IDisplayBackground _background;
        public IDisplayBackground Background => _background;
        
        protected IUIAnimation ShowAnimation;
        private IUIAnimation _hideAnimation;
        
        protected IUIAnimation PartShowAnimation;
        private IUIAnimation _partHideAnimaiton;

        [Inject]
        public virtual void Construct(
            IDisplayBackground background,
            [Key(AnimationType.Show)] IUIAnimation showAnimation,
            [Key(AnimationType.Hide)] IUIAnimation hideAnimation,
            [Key(AnimationType.ShowParts)] IUIAnimation partShowAnimation,
            [Key(AnimationType.HideParts)] IUIAnimation partHideAnimation)
        {
            _background = background;
            
            ShowAnimation = showAnimation;
            _hideAnimation = hideAnimation;
            
            PartShowAnimation = partShowAnimation;
            _partHideAnimaiton = partHideAnimation;
        }
        
        /// <summary>
        /// アニメーションで表示。
        /// - CanvasGroup を表示状態に設定
        /// - UI パーツを順番にフェードイン
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await ShowAnimation.PlayAsync(ct);
            await PartShowAnimation.PlayAsync(ct);
        }
        
        /// <summary>
        /// 即時表示。
        /// - CanvasGroup を表示状態に設定
        /// - 全 UI パーツの透明度を一括変更
        /// </summary>
        public override void Show()
        {
            PartShowAnimation.PlayImmediate();
            ShowAnimation.PlayImmediate();
        }
        
        /// <summary>
        /// アニメーションで非表示。
        /// - 全体 CanvasGroup をフェードアウト
        /// - パーツの透明度も最終値に揃える
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            _partHideAnimaiton.PlayImmediate();
        }

        /// <summary>
        /// 即時非表示。
        /// - CanvasGroup を非表示状態に設定
        /// - 全パーツの透明度を一括変更
        /// </summary>
        public override void Hide()
        {
            _hideAnimation.PlayImmediate();
            _partHideAnimaiton.PlayImmediate();
        }
    }
}
