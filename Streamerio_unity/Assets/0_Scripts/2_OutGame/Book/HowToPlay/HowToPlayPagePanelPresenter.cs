using Common;
using Common.UI.Display.Window.Book.Page;

namespace OutGame.Book.HowToPlay
{
    public interface IHowToPlayPagePanel : IPagePanel, IAttachable<HowToPlayPagePanelContext>
    {
        
    }
    
    public class HowToPlayPagePanelPresenter: PagePanelPresenterBase<IPagePanelView, HowToPlayPagePanelContext>, IHowToPlayPagePanel
    {
        
    }
    
    public class HowToPlayPagePanelContext : PagePanelContext<IPagePanelView>
    {
        
    }
}