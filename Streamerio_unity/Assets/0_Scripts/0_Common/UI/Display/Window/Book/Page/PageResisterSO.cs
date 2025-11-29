using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;

namespace Common.UI.Display.Window.Book.Page
{
    [CreateAssetMenu(fileName = "PageResisterSO", menuName = "SO/UI/Book/PageResisterSO")]
    public class PageResisterSO: ScriptableObject, IPageResisterSO
    {
        [SerializeField]
        private LifetimeScope[] _pagePanelLifetimeScopes;
        public IReadOnlyList<LifetimeScope> PagePanelLifetimeScopes => _pagePanelLifetimeScopes;
    }
    
    public interface IPageResisterSO
    {
        IReadOnlyList<LifetimeScope> PagePanelLifetimeScopes { get; }
    }
}