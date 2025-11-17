using System;
using System.Collections.Generic;
using Alchemy.Inspector;
using Common.State;
using Common.UI.Display.Window.Book.Chapter;
using Common.UI.Display.Window.Book.Page;
using Common.UI.Part.Button;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZLinq;

namespace Common.UI.Display.Window.Book
{
    [RequireComponent(typeof(BookAnimationComponent))]
    public class BookWindowLifetimeScope: WindowLifetimeScopeBase<IBookWindow, BookWindowPresenter, IBookWindowView, BookWindowContext>
    {
        [SerializeField]
        private ChapterType _initialChapterType;
        
        [SerializeField, ReadOnly]
        private BookAnimationComponent _bookAnimation;
        
        [SerializeField]
        private SerializeDictionary<ChapterType, ChapterData> _chapterPanelDict;

        [SerializeField]
        private StateType _nextStateOnClose;
        
#if UNITY_EDITOR
        protected void OnValidate()
        {
            _bookAnimation ??= GetComponent<BookAnimationComponent>();
        }
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Close);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.NextPage);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.BackPage);
            
            builder.RegisterComponent<IBookAnimation>(_bookAnimation); ;
            
            Dictionary<ChapterType, IPagePanelIterator> pagePanelIteratorDict = _chapterPanelDict.ToDictionary()
                .AsValueEnumerable()
                .ToDictionary(
                    kv => kv.Key,
                    kv =>
                    {
                        var scopes = kv.Value.PageResisterSO.PagePanelLifetimeScopes;
                        var parent = kv.Value.Parent;
                        var factory = new PageFactory(scopes, parent, this);
                        return (IPagePanelIterator)new PagePanelIterator(scopes.Count, factory);
                    });

            var model = new BookWindowModel(pagePanelIteratorDict);
            
            builder.RegisterInstance<IBookWindowModel>(model);
            
            base.Configure(builder);
        }

        protected override BookWindowContext CreateContext(IObjectResolver resolver)
        {
            return new BookWindowContext()
            {
                View = resolver.Resolve<IBookWindowView>(),
                BookWindowModel = resolver.Resolve<IBookWindowModel>(),
                InitialChapterType = _initialChapterType,
                BookAnimation = resolver.Resolve<IBookAnimation>(),
                StateManager = resolver.Resolve<IStateManager>(),
                NextState = resolver.Resolve<IState>(_nextStateOnClose),
            };
        }

        [Serializable]
        private class ChapterData
        {
            public PageResisterSO PageResisterSO;
            public Transform Parent;
        }
    }
}