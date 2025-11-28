using System.Threading;
using Common.Audio;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.Title.UI;
using VContainer;

namespace Common.State
{
    public class TitleStartState: IState
    {
        private readonly IAudioFacade _audioFacade;
        private readonly ILoadingScreen _loadingScreen;
        
        private readonly ITitleScreen _titleScreen;
        
        private readonly IStateManager _stateManager;
        private readonly IState _nextState;
        
        [Inject]
        public TitleStartState(IAudioFacade audioFacade, ILoadingScreen loadingScreen, ITitleScreen titleScreen, IStateManager stateManager, 
            [Key(StateType.Title)] IState nextState)
        {
            _audioFacade = audioFacade;
            _loadingScreen = loadingScreen;
            
            _titleScreen = titleScreen;
            
            _stateManager = stateManager;
            _nextState = nextState;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            _titleScreen.Hide();
            _audioFacade.PlayAsync(BGMType.kuraituuro, ct).Forget();

            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
            _stateManager.ChangeState(_nextState);
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _loadingScreen.HideAsync(ct);
        }
    }
}