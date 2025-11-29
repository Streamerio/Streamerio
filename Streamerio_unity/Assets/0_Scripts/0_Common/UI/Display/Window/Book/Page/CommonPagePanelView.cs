using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.UI.Display.Window.Book.Page
{
    /// <summary>
    /// ページの見た目（View）を管理するクラス。
    /// - 内部の UI パーツグループ（CommonUIPartGroup）を使って表示/非表示を制御
    /// - CanvasGroup の透明度を直接制御して、全体の表示状態を管理
    /// - アニメーションと即時表示/非表示の両方に対応
    /// </summary>
    public class CommonPagePanelView : DisplayViewBase, IPagePanelView
    {
        private IUIAnimation _showAnim;
        private IUIAnimation _hideAnim;

        [Inject]
        public void Construct(
            [Key(AnimationType.ShowParts)] IUIAnimation showAnim,
            [Key(AnimationType.HideParts)] IUIAnimation hideAnim)
        {
            _showAnim = showAnim;
            _hideAnim = hideAnim;
        }

        /// <summary>
        /// アニメーションで表示。
        /// - 全体の CanvasGroup の透明度を設定
        /// - 内部のパーツを順次表示アニメーション
        /// </summary>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
            await _showAnim.PlayAsync(ct);
        }

        /// <summary>
        /// 即時表示。
        /// - CanvasGroup を即時透明度変更
        /// - 内部のパーツを即時表示
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
            _showAnim.PlayImmediate();
        }

        /// <summary>
        /// アニメーションで非表示。
        /// - 内部のパーツを順次非表示アニメーション
        /// - 完了後に CanvasGroup を非表示状態にする
        /// </summary>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            _hideAnim.PlayImmediate();
            CanvasGroup.alpha = UIUtil.DEFAULT_HIDE_ALPHA;
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }

        /// <summary>
        /// 即時非表示。
        /// - 内部のパーツを即時非表示
        /// - CanvasGroup を非表示状態にする
        /// </summary>
        public override void Hide()
        {
            _hideAnim.PlayImmediate();
            CanvasGroup.alpha = UIUtil.DEFAULT_HIDE_ALPHA;
        }
    }
    
    public interface IPagePanelView : IDisplayView
    {
    }
}