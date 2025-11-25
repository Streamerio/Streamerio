using System.Threading;
using Common.UI.Dialog;
using Cysharp.Threading.Tasks;
using InGame.QRCode.UI;
using VContainer;

namespace Common.State
{
    public class PlayFromTitleState: IState
    {
        private readonly IDialogService _dialogService;
        
        private readonly IStateManager _stateManager;
        private readonly IState _nextState;
        
        [Inject]
        public PlayFromTitleState(IDialogService dialogService, IStateManager stateManager, [Key(StateType.InGame)] IState nextState)
        {
            _dialogService = dialogService;
            
            _stateManager = stateManager;
            _nextState = nextState;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _dialogService.OpenAndWaitCloseAsync<IQRCodeDialog>(ct);
            _stateManager.ChangeState(_nextState);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}