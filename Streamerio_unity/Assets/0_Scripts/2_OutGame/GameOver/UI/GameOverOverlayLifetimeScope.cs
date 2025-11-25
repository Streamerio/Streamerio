using Common.State;
using Common.UI.Display.Overlay;
using Common.UI.Part.Button;
using VContainer;

namespace OutGame.GameOver.UI
{
    public class GameOverOverlayLifetimeScope: OverlayLifetimeScopeBase<IGameOverOverlay, GameOverOverlayPresenter, IGameOverOverlayView, GameOverOverlayContext>
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Restart);
            
            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Title);
            
            base.Configure(builder);
        }

        protected override GameOverOverlayContext CreateContext(IObjectResolver resolver)
        {
            return new GameOverOverlayContext
            {
                View = resolver.Resolve<IGameOverOverlayView>(),
                StateManager = resolver.Resolve<IStateManager>(),
                RestartState = resolver.Resolve<IState>(StateType.Restart),
                ToTitleState = resolver.Resolve<IState>(StateType.ToTitle),
            };
        }
    }
}