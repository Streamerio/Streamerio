using Common;
using Common.State;
using Common.UI.Dialog;
using R3;

namespace OutGame.Network
{
    public interface IReconnectionDialog : IDialog, IAttachable<ReconnectionDialogContext>
    {
        
    }

    public class ReconnectionDialogPresenter: DialogPresenterBase<IReconnectionDialogView, ReconnectionDialogContext>, IReconnectionDialog
    {
        private IStateManager _stateManager;
        private IState _inGameLoadingState;
        private IState _titleStartState;

        protected override void AttachContext(ReconnectionDialogContext context)
        {
            base.AttachContext(context);
            
            _stateManager = context.StateManager;
            _inGameLoadingState = context.InGameLoadingState;
            _titleStartState = context.TitleStartState;
        }

        protected override void Bind()
        {
            base.Bind();
            
            View.ReconnectButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    _stateManager.ChangeState(_inGameLoadingState);
                })
                .RegisterTo(GetCt());
        }

        protected override void CloseEvent()
        {
            _stateManager.ChangeState(_titleStartState);
        }
    }

    public class ReconnectionDialogContext: DialogContext<IReconnectionDialogView>
    {
        public IStateManager StateManager;
        public IState InGameLoadingState;
        public IState TitleStartState;
    }
}