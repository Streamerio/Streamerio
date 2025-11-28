using Common.UI.Display.Window.Book.Page;
using VContainer;

namespace OutGame.Book.HowToPlay
{
    public class HowToPlayPagePanelLifetimeScope: PageLifetimeScopeBase<IHowToPlayPagePanel, HowToPlayPagePanelPresenter, IPagePanelView, HowToPlayPagePanelContext>
    {
        protected override HowToPlayPagePanelContext CreateContext(IObjectResolver resolver)
        {
            return new HowToPlayPagePanelContext()
            {
                View = resolver.Resolve<IPagePanelView>()
            };
        }
    }
}