namespace Common.UI.Display.Window.Book.Page
{
    public interface IPagePanel: IDisplay
    {
        
    }
    
    /// <summary>
    /// ページのつなぎ役 (Presenter)。
    /// - PagePanelView とやり取りするためのプレゼンター層
    /// - 現状は追加の処理を持たず、基底クラスの機能をそのまま利用する
    /// - 必要に応じてイベントハンドリングやロジックをここに追加可能
    /// </summary>
    public abstract class PagePanelPresenterBase<TView, TContext> : DisplayPresenterBase<TView, TContext>, IPagePanel
        where TView : IPagePanelView
        where TContext : PagePanelContext<TView>
    {
        protected override void AttachContext(TContext context)
        {
            View = context.View;
        }
    }
    
    public class PagePanelContext<TView>
        where TView : IPagePanelView
    {
        public TView View;
    }
}