using Common;
using Common.Scene;
using Common.State;
using Common.UI.Animation;
using Common.UI.Display.Window.Book.Chapter;
using Common.UI.Display.Window.Book.Page;
using Common.UI.Loading;
using R3;

namespace OutGame.Book.Menu
{
    public interface IMenuPagePanel : IPagePanel, IAttachable<MenuPagePanelContext>
    {
    }
    
    /// <summary>
    /// メニューページのプレゼンター。
    /// - MenuPagePanelView と連携し、ボタン操作を購読する
    /// - 「スタート/遊び方/オプション/クレジット」各ボタンのイベントをバインド
    /// - ボタンクリックに応じて、シーン遷移や UI 遷移を制御する
    /// </summary>
    public class MenuPagePanelPresenter : PagePanelPresenterBase<IMenuPagePanelView, MenuPagePanelContext>, IMenuPagePanel
    {
        private IBookWindowModel _bookWindowModel;
        private IStateManager _stateManager;
        private IState _nextState;
        
        protected override void AttachContext(MenuPagePanelContext context)
        {
            base.AttachContext(context);
            _bookWindowModel = context.BookWindowModel;
            _stateManager = context.StateManager;
            _nextState = context.NextState;
        }

        /// <summary>
        /// ボタンイベントのバインド処理。
        /// - 各種ボタンクリックに対応する遷移/処理を登録
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            // ゲーム開始ボタン
            View.StartButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _stateManager.ChangeState(_nextState);
                }).RegisterTo(GetCt());
            
            // 遊び方ボタン
            View.HowToPlayButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _bookWindowModel.PushChapter(ChapterType.HowToPlay);
                }).RegisterTo(GetCt());
            
            // オプションボタン
            View.OptionButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _bookWindowModel.PushChapter(ChapterType.Option);
                }).RegisterTo(GetCt());
            
            // クレジットボタン
            View.CreditButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _bookWindowModel.PushChapter(ChapterType.Credit);
                }).RegisterTo(GetCt());
        }
    }
    
    public class MenuPagePanelContext : PagePanelContext<IMenuPagePanelView>
    {
        public IBookWindowModel BookWindowModel;
        public IStateManager StateManager;
        public IState NextState;
    }
}