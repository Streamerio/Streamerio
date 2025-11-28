using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Window.Book.Page
{
    public interface IPageFactory
    {
        IPagePanel CreatePage(int pageIndex);
    }

    public class PageFactory: IPageFactory
    {
        private readonly IReadOnlyList<LifetimeScope> _pagePanelLifetimeScopes;
        private readonly Transform _parent;
        private readonly LifetimeScope _parentScope;

        public PageFactory(IReadOnlyList<LifetimeScope> pagePanelLifetimeScopes, Transform parent,
            LifetimeScope parentScope)
        {
            _pagePanelLifetimeScopes = pagePanelLifetimeScopes;
            _parent = parent;
            _parentScope = parentScope;
        }

        public IPagePanel CreatePage(int pageIndex)
        {
            using (LifetimeScope.EnqueueParent(_parentScope))
            {
                var instance = Object.Instantiate(_pagePanelLifetimeScopes[pageIndex], _parent);

                var display = instance.Container.Resolve<IPagePanel>();
                return display;
            }
        }
    }
}