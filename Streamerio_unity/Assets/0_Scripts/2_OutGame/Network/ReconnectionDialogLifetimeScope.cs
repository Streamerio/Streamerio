using Common.State;
using Common.UI.Dialog;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using VContainer;

namespace OutGame.Network
{
    public class ReconnectionDialogLifetimeScope: DialogLifetimeScopeBase<IReconnectionDialog, ReconnectionDialogPresenter, IReconnectionDialogView, ReconnectionDialogContext>
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Restart);
            
            base.Configure(builder);
        }

        protected override ReconnectionDialogContext CreateContext(IObjectResolver resolver)
        {
            return new()
            {
                View = resolver.Resolve<IReconnectionDialogView>(),
                Service = resolver.Resolve<IDialogService>(),
                StateManager = resolver.Resolve<IStateManager>(),
                InGameLoadingState = resolver.Resolve<IState>(StateType.InGameLoading),
                TitleStartState = resolver.Resolve<IState>(StateType.TitleStart),
            };
        }
    }
}