using System.Threading;
using Common;
using Common.State;
using Common.UI.Display.Overlay;
using Cysharp.Threading.Tasks;
using R3;

namespace OutGame.GameOver.UI
{
    public interface IGameOverOverlay: IOverlay, IAttachable<GameOverOverlayContext>
    {
        
    }
    
    /// <summary>
    /// ゲームオーバー時のオーバーレイ Presenter。
    /// - Retry / Title ボタンのイベント処理を担当
    /// - ボタンクリック時にロード画面を表示し、シーン遷移を実行
    /// - リトライ時は SaveManager にフラグを設定
    /// </summary>
    public class GameOverOverlayPresenter : OverlayPresenterBase<IGameOverOverlayView, GameOverOverlayContext>, IGameOverOverlay
    {
        private IStateManager _stateManager;
        private IState _restartState;
        private IState _toTitleState;

        private CancellationTokenSource _skipAnimationCts;

        protected override void AttachContext(GameOverOverlayContext context)
        {
            base.AttachContext(context);

            _stateManager = context.StateManager;
            _restartState = context.RestartState;
            _toTitleState = context.ToTitleState;
        }

        /// <summary>
        /// イベント購読のセットアップ。
        /// - Retry ボタン → ローディング画面を表示 → Retry フラグ設定 → GameScene 再ロード
        /// - Title ボタン → ローディング画面を表示 → Title シーンへ遷移
        /// </summary>
        protected override void Bind()
        {
            base.Bind();
            _skipAnimationCts = CancellationTokenSource.CreateLinkedTokenSource(GetCt());

            View.Background.OnClickAsObservable
                .Subscribe(_ =>
                {
                    View.SkipShowAnimation();
                    _skipAnimationCts.Cancel();
                })
                .RegisterTo(_skipAnimationCts.Token);
            
            // リトライボタンクリック
            View.RetryButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _stateManager.ChangeState(_restartState);
                })
                .RegisterTo(GetCt());
            
            // タイトルボタンクリック
            View.TitleButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _stateManager.ChangeState(_toTitleState);
                })
                .RegisterTo(GetCt());
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            SetActiveButtons(false);
            View.SetInteractable(true);
            
            await base.ShowAsync(ct);
            
            SetActiveButtons(true);
        }
        
        private void SetActiveButtons(bool isActive)
        {
            View.TitleButton.SetActive(isActive);
            View.RetryButton.SetActive(isActive);
        }
    }

    public class GameOverOverlayContext : OverlayContext<IGameOverOverlayView>
    {
        public IStateManager StateManager;
        public IState RestartState;
        public IState ToTitleState;
    }
}