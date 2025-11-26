using Common.State;
using Common.UI.Display.Window.Book.Chapter;
using Common.UI.Display.Window.Book.Page;
using Common.UI.Part.Button;
using VContainer;

namespace OutGame.Book.Menu
{
    public class MenuPagePanelLifetimeScope: PageLifetimeScopeBase<IMenuPagePanel, MenuPagePanelPresenter, IMenuPagePanelView, MenuPagePanelContext>
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.GameStart);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.HowToPlay);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Option);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Credit);
            
            base.Configure(builder);
        }

        protected override MenuPagePanelContext CreateContext(IObjectResolver resolver)
        {
            return new MenuPagePanelContext()
            {
                View = resolver.Resolve<IMenuPagePanelView>(),
                BookWindowModel = resolver.Resolve<IBookWindowModel>(),
                StateManager = resolver.Resolve<IStateManager>(),
                NextState = resolver.Resolve<IState>(StateType.TitleEnd),
            };
        }
    }
}