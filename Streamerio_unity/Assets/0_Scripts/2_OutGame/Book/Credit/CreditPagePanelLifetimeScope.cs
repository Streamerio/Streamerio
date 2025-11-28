using Common.UI.Display.Window.Book.Page;
using VContainer;

namespace OutGame.Book.Credit
{
    public class CreditPagePanelLifetimeScope: PageLifetimeScopeBase<ICreditPagePanel, CreditPagePanelPresenter, IPagePanelView, CreditPagePanelContext>
    {
        protected override CreditPagePanelContext CreateContext(IObjectResolver resolver)
        {
            return new CreditPagePanelContext()
            {
                View = resolver.Resolve<IPagePanelView>()
            };
        }
    }
}