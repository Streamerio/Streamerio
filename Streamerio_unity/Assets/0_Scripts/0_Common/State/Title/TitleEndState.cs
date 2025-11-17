using System.Threading;
using Common.Audio;
using Common.Scene;
using Common.UI.Animation;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class TitleEndState: IState
    {
        private readonly IUIAnimation _titleBackgroundAnimation;
        private readonly ILoadingScreen _loadingScreen;
        private readonly IAudioFacade _audioFacade;
        
        private readonly IStateManager _stateManager;
        private readonly IState _nextState;
        
        [Inject]
        public TitleEndState(
            [Key(AnimationType.TitleBackground)] IUIAnimation titleBackgroundAnimation,
            ILoadingScreen loadingScreen,
            IAudioFacade audioFacade,
            IStateManager stateManager,
            [Key(StateType.InGameLoading)] IState nextState)
        {
            _titleBackgroundAnimation = titleBackgroundAnimation;
            _loadingScreen = loadingScreen;
            _audioFacade = audioFacade;
            
            _stateManager = stateManager;
            _nextState = nextState;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            _titleBackgroundAnimation.PlayAsync(ct).Forget();
            await _loadingScreen.ShowAsync(ct);
            
            await _audioFacade.StopBGMAsync(ct);
            
            _stateManager.ChangeState(_nextState);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}