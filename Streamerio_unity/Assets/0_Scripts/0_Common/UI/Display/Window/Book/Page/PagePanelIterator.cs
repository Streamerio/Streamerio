using R3;

namespace Common.UI.Display.Window.Book.Page
{
    public interface IPagePanelIterator
    {
        ReadOnlyReactiveProperty<bool> IsFirstPageProp { get; }
        ReadOnlyReactiveProperty<bool> IsLastPageProp { get; }
        
        IPagePanel GetCurrentPage();
        IPagePanel MoveNext();
        IPagePanel MoveBack();
        void Reset();
    }
    
    public class PagePanelIterator: IPagePanelIterator
    {
        private readonly IPagePanel[] _pages;
        
        private readonly int _lastPageIndex;
        private int _currentIndex;
        
        private readonly ReactiveProperty<bool> _isFirstPageProp;
        public ReadOnlyReactiveProperty<bool> IsFirstPageProp => _isFirstPageProp;
        
        private readonly ReactiveProperty<bool> _isLastPageProp;
        public ReadOnlyReactiveProperty<bool> IsLastPageProp => _isLastPageProp;
        
        private readonly IPageFactory _factory;
        
        
        public PagePanelIterator(int pageCount, IPageFactory factory)
        {
            _pages = new IPagePanel[pageCount];
            _lastPageIndex = pageCount - 1;
            _currentIndex = 0;
            
            _isFirstPageProp = new ReactiveProperty<bool>(true);
            _isLastPageProp = new ReactiveProperty<bool>(_currentIndex == _lastPageIndex);
            
            _factory = factory;
        }

        public IPagePanel GetCurrentPage()
        {
            return _pages[_currentIndex] ?? (_pages[_currentIndex] = _factory.CreatePage(_currentIndex));
        }
        
        public IPagePanel MoveNext()
        {
            if (_currentIndex < _lastPageIndex)
            {
                _currentIndex++;
                UpdatePageFlags();
            }
            
            return GetCurrentPage();
        }
        
        public IPagePanel MoveBack()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                UpdatePageFlags();
            }
            
            return GetCurrentPage();
        }
        
        public void Reset()
        {
            _currentIndex = 0;
            UpdatePageFlags();
        }
        
        private void UpdatePageFlags()
        {
            _isFirstPageProp.Value = _currentIndex == 0;
            _isLastPageProp.Value = _currentIndex == _lastPageIndex;
        }
    }
}