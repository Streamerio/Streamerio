using System.Threading;
using Common.State;
using Common.UI.Display.Window.Book.Chapter;
using Cysharp.Threading.Tasks;
using R3;

namespace Common.UI.Display.Window.Book
{
    public interface IBookWindow: IWindow, IAttachable<BookWindowContext>
    {
        
    }
    
    /// <summary>
    /// 共通の「本型ウィンドウ」のプレゼンター基底クラス。
    /// - 「本を開く」ような演出を持つウィンドウの基盤
    /// - 派生クラスで最初に開くチャプターを指定して利用する
    /// - ChapterManager を通じてチャプターを制御
    /// - ChapterManager の状態を監視し、すべてのチャプターが閉じたら自動的にウィンドウも閉じる
    /// </summary>
    public class BookWindowPresenter : WindowPresenterBase<IBookWindowView, BookWindowContext>, IBookWindow
    {
        private IBookWindowModel _bookWindowModel;
        private ChapterType _initialChapterType;
        
        private IBookAnimation _bookAnimation;

        private IStateManager _stateManager;
        private IState _nextState;

        private CancellationTokenSource _windowCts;
        private CancellationTokenSource _chapterCts;

        protected override void Bind()
        {
            base.Bind();
            
            View.CloseButton.OnClickAsObservable
                .Subscribe(_ => { CloseEvent(); })
                .RegisterTo(GetCt());
        }

        protected override void AttachContext(BookWindowContext context)
        {
            base.AttachContext(context);
            _bookWindowModel = context.BookWindowModel;
            _initialChapterType = context.InitialChapterType;
            _bookAnimation = context.BookAnimation;
            _stateManager = context.StateManager;
            _nextState = context.NextState;
        }
        
        protected override void CloseEvent()
        {
            _bookWindowModel.PopTopChapter();   
        }

        public override async UniTask ShowAsync(CancellationToken ct)
        {
            InitializePage();
            await base.ShowAsync(ct);
        }

        public override void Show()
        {
            InitializePage();
            base.Show();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await base.HideAsync(ct);
            DisposeWindowEvent();
            DisposeChapterEvent();
        }

        public override void Hide()
        {
            base.Hide();
            DisposeWindowEvent();
            DisposeChapterEvent();
        }

        /// <summary>
        /// 次のページへ。
        /// 流れ:
        /// 1) 現ページを非表示（非同期）
        /// 2) 右めくり演出
        /// 3) 次ページを表示
        /// 4) 操作再開
        /// </summary>
        private async UniTask OpenNextPage(CancellationToken ct)
        {
            View.SetInteractable(false);
            await _bookWindowModel.CurrentPagePanelIterator.GetCurrentPage().HideAsync(ct);
            await _bookAnimation.PlayTurnRightAsync(ct);
            await _bookWindowModel.CurrentPagePanelIterator.MoveNext().ShowAsync(ct);
            View.SetInteractable(true);
        }

        /// <summary>
        /// 前のページへ。
        /// 流れ:
        /// 1) 現ページを非表示（非同期）
        /// 2) 左めくり演出
        /// 3) 前ページを表示
        /// 4) 操作再開
        /// </summary>
        private async UniTask OpenPrePage(CancellationToken ct)
        {
            View.SetInteractable(false);
            await _bookWindowModel.CurrentPagePanelIterator.GetCurrentPage().HideAsync(ct);
            await _bookAnimation.PlayTurnLeftAsync(ct);
            await _bookWindowModel.CurrentPagePanelIterator.MoveBack().ShowAsync(ct);
            View.SetInteractable(true);
        }
        
        private void InitializePage()
        {
            DisposeWindowEvent();
            DisposeChapterEvent();

            while(!_bookWindowModel.IsEmptyProp.CurrentValue)
            {
                _bookWindowModel.CurrentPagePanelIterator.GetCurrentPage().Hide();
                _bookWindowModel.PopTopChapter();
            }

            _bookWindowModel.PushChapter(_initialChapterType);
            BindWindowEvent();
        }
        
        private void BindWindowEvent()
        {
            _windowCts = CancellationTokenSource.CreateLinkedTokenSource(GetCt());
            
            _bookWindowModel.CurrentPagePanelIteratorProp
                .DistinctUntilChanged()
                .Where(_ => !_bookWindowModel.IsEmptyProp.CurrentValue)
                .SubscribeAwait(async (_, ct) =>
                {
                    View.SetInteractable(false);
                    
                    DisposeChapterEvent();
                    BindChapterEvent();

                    if (_bookWindowModel.PreviousPagePanelIterator != null)
                    {
                        await _bookWindowModel.PreviousPagePanelIterator.GetCurrentPage().HideAsync(ct);
                    }
                    await _bookWindowModel.CurrentPagePanelIterator.GetCurrentPage().ShowAsync(ct);
                    
                    View.SetInteractable(true);
                })
                .RegisterTo(_windowCts.Token);

            _bookWindowModel.IsEmptyProp
                .SkipWhile(isEmpty => isEmpty)
                .DistinctUntilChanged()
                .Where(isEmpty => isEmpty)
                .Subscribe(_ =>
                {
                    _stateManager.ChangeState(_nextState);
                })
                .RegisterTo(_windowCts.Token);
        }
        
        private void DisposeWindowEvent()
        {
            _windowCts?.Cancel();
            _windowCts?.Dispose();
            _windowCts = null;
        }

        private void BindChapterEvent()
        {
            _chapterCts = CancellationTokenSource.CreateLinkedTokenSource(GetCt());
            
            View.NextButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) => { await OpenNextPage(ct); })
                .RegisterTo(_chapterCts.Token);
            
            View.BackButton.OnClickAsObservable
                .SubscribeAwait(async (_, ct) => { await OpenPrePage(ct); })
                .RegisterTo(_chapterCts.Token);
            
            _bookWindowModel.IsFirstPageProp
                .DistinctUntilChanged()
                .Subscribe(isFirst =>
                {
                    View.BackButton.SetActive(!isFirst);
                })
                .RegisterTo(_chapterCts.Token);
            
            _bookWindowModel.IsLastPageProp
                .DistinctUntilChanged()
                .Subscribe(isLast =>
                {
                    View.NextButton.SetActive(!isLast);
                })
                .RegisterTo(_chapterCts.Token);
        }

        private void DisposeChapterEvent()
        {
            _chapterCts?.Cancel();
            _chapterCts?.Dispose();
            _chapterCts = null;
        }
    }
    
    public class BookWindowContext: WindowContext<IBookWindowView>
    {
        public IBookWindowModel BookWindowModel;
        public ChapterType InitialChapterType;
        public IBookAnimation BookAnimation;
        public IStateManager StateManager;
        public IState NextState;
    }
}
