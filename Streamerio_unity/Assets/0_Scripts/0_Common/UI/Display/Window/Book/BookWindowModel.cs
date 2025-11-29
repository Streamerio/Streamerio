using System.Collections.Generic;
using Common.UI.Display.Window.Book.Page;
using R3;

namespace Common.UI.Display.Window.Book.Chapter
{
    public interface IBookWindowModel
    {
        ReadOnlyReactiveProperty<IPagePanelIterator> CurrentPagePanelIteratorProp { get; }
        IPagePanelIterator CurrentPagePanelIterator { get; }
        IPagePanelIterator PreviousPagePanelIterator { get; }
        
        ReadOnlyReactiveProperty<bool> IsFirstPageProp { get; }
        ReadOnlyReactiveProperty<bool> IsLastPageProp { get; }
        ReadOnlyReactiveProperty<bool> IsEmptyProp { get; }
        
        void PushChapter(ChapterType chapterType);
        void PopTopChapter();
    }
    
    public class BookWindowModel: IBookWindowModel
    {
        private readonly IReadOnlyDictionary<ChapterType, IPagePanelIterator> _pagePanelIteratorDict;
        
        private ReactiveProperty<IPagePanelIterator> _currentPagePanelIteratorProp;
        public ReadOnlyReactiveProperty<IPagePanelIterator> CurrentPagePanelIteratorProp => _currentPagePanelIteratorProp;
        public IPagePanelIterator CurrentPagePanelIterator => _currentPagePanelIteratorProp.Value;
        
        private IPagePanelIterator _previousPagePanelIterator;
        public IPagePanelIterator PreviousPagePanelIterator => _previousPagePanelIterator;
        
        public ReadOnlyReactiveProperty<bool> IsFirstPageProp => CurrentPagePanelIterator.IsFirstPageProp;
        public ReadOnlyReactiveProperty<bool> IsLastPageProp => CurrentPagePanelIterator.IsLastPageProp;

        private readonly ReactiveProperty<bool> _isEmptyProp;
        public ReadOnlyReactiveProperty<bool> IsEmptyProp => _isEmptyProp;
        
        private Stack<IPagePanelIterator> _historyStack;
        
        public BookWindowModel(IReadOnlyDictionary<ChapterType, IPagePanelIterator> pagePanelIteratorDict)
        {
            _pagePanelIteratorDict = pagePanelIteratorDict;
            
            _currentPagePanelIteratorProp = new ReactiveProperty<IPagePanelIterator>();
            
            _isEmptyProp = new ReactiveProperty<bool>(true);
            
            _historyStack = new Stack<IPagePanelIterator>();
        }
        
        public void PushChapter(ChapterType chapterType)
        {
            if (!_pagePanelIteratorDict.TryGetValue(chapterType, out var iterator))
            {
                Debug.LogError($"ChapterModel: 指定された ChapterType {chapterType} のページパネルイテレータが存在しません。");
                return;
            }
            
            if(CurrentPagePanelIterator != null)
            {
                _historyStack.Push(CurrentPagePanelIterator);
            }
            
            UpdateChapter(iterator);
            _isEmptyProp.Value = false;
        }
        
        public void PopTopChapter()
        {
            if (_isEmptyProp.Value)
            {
                return;
            }
            
            if(_historyStack.Count > 0)
            {
                UpdateChapter(_historyStack.Pop());
            }
            else
            {
                _isEmptyProp.Value = true;
                _currentPagePanelIteratorProp.Value = null;
                _previousPagePanelIterator = null;
            }
        }

        private void UpdateChapter(IPagePanelIterator iterator)
        {
            _previousPagePanelIterator = CurrentPagePanelIterator;
            
            iterator.Reset();
            _currentPagePanelIteratorProp.Value = iterator;
        }
    }
}