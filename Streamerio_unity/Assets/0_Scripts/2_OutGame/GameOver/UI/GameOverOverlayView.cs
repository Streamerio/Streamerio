using Common.UI.Display.Overlay;
using Common.UI.Part.Button;
using VContainer;

namespace OutGame.GameOver.UI
{
    /// <summary>
    /// ゲームオーバー画面の View。
    /// - Retry ボタン
    /// - Title ボタン
    /// の参照と初期化を担当する
    /// </summary>
    public class GameOverOverlayView : OverlayViewBase, IGameOverOverlayView
    {
        private ICommonButton _retryButton;
        /// <summary>ゲームを再開するためのボタン</summary>
        public ICommonButton RetryButton => _retryButton;
        
        private ICommonButton _titleButton;
        /// <summary>タイトル画面へ戻るためのボタン</summary>
        public ICommonButton TitleButton => _titleButton;
        
        [Inject]
        public void Construct([Key(ButtonType.Restart)]ICommonButton retryButton, [Key(ButtonType.Title)]ICommonButton titleButton)
        {
            _retryButton = retryButton;
            _titleButton = titleButton;
        }
        
        public void SkipShowAnimation()
        {
            ShowAnimation.Skip();
            PartShowAnimation.Skip();
        }
    }

    public interface IGameOverOverlayView : IOverlayView
    {
        ICommonButton RetryButton { get; }
        ICommonButton TitleButton { get; }
        
        void SkipShowAnimation();
    }
}