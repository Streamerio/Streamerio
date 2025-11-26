using Common.UI.Part;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Window.Book.Page
{
    public abstract class PageLifetimeScopeBase<TPage, TPresenter, TView, TContext>: DisplayLifetimeScopeBase<TPage, TPresenter, TView, TContext>
        where TPage : IPagePanel, IAttachable<TContext>
        where TPresenter : PagePanelPresenterBase<TView, TContext>, TPage
        where TView : IPagePanelView
        where TContext : PagePanelContext<TView>
    {
        [SerializeField]
        private AnimationPartGroup _animationPartsGroup;
        
        protected override void Configure(IContainerBuilder builder)
        {
            _animationPartsGroup.BindAnimation(builder);
            
            base.Configure(builder);
        }

        protected override void BindPresenter(IContainerBuilder builder)
        {
            builder.Register<TPage, TPresenter>(Lifetime.Singleton)
                .As<TPage>()
                .As<IPagePanel>()
                .As<IStartable>();
            
            builder.RegisterEntryPoint<Wiring<TPage, TContext>>()
                .WithParameter(resolver => resolver.Resolve<TPage>())
                .WithParameter(CreateContext);
        }
    }
}