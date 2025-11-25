using Common;
using Common.UI.Display.Window.Book.Page;

namespace OutGame.Book.Credit
{
    public interface ICreditPagePanel : IPagePanel, IAttachable<CreditPagePanelContext>
    {
        
    }
    
    public class CreditPagePanelPresenter: PagePanelPresenterBase<IPagePanelView, CreditPagePanelContext>, ICreditPagePanel
    {
        
    }
    
    public class CreditPagePanelContext : PagePanelContext<IPagePanelView>
    {
        
    }
}