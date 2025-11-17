using System.Threading;
using Common.Audio;
using Common.Save;
using Common.Scene;
using Common.UI.Animation;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using InGame.Setting;
using VContainer;

namespace Common.State
{
    public class InGameStartState : IState
    {
        private readonly IPlayDataSaveFacade _playDataSaveFacade;
        
        private readonly ILoadingScreen _loadingScreen;
        private readonly ISceneManager _sceneManager;
        
        private readonly IAudioFacade _audioFacade;
        
        private readonly IInGameSetting _inGameSetting;
        
        private readonly IStateManager _stateManager;
        private readonly IState _firstPlayState;
        private readonly IState _playFromTitleState;
        private readonly IState _nextState;
        
        private readonly IUIAnimation _inGameBackgroundAnimation;
        
        [Inject]
        public InGameStartState(
            IPlayDataSaveFacade playDataSaveFacade,
            ILoadingScreen loadingScreen,
            ISceneManager sceneManager,
            IAudioFacade audioFacade,
            IInGameSetting inGameSetting,
            IStateManager stateManager,
            [Key(StateType.FirstPlay)] IState firstPlayState,
            [Key(StateType.PlayFromTitle)] IState playFromTitleState,
            [Key(StateType.InGame)] IState nextState,
            [Key(AnimationType.InGameBackground)] IUIAnimation inGameBackgroundAnimation)
        {
            _playDataSaveFacade = playDataSaveFacade;
            
            _loadingScreen = loadingScreen;
            _sceneManager = sceneManager;
            
            _audioFacade = audioFacade;
            
            _inGameSetting = inGameSetting;
            
            _stateManager = stateManager;
            _firstPlayState = firstPlayState;
            _playFromTitleState = playFromTitleState;
            _nextState = nextState;
            
            _inGameBackgroundAnimation = inGameBackgroundAnimation;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            _audioFacade.PlayAsync(_inGameSetting.BGM, ct).Forget();
            
            if (!_playDataSaveFacade.LoadPlayed())
            {
                _playDataSaveFacade.SavePlayed();
                _stateManager.ChangeState(_firstPlayState);
            }
            else if(!_sceneManager.IsRestart)
            {
                _stateManager.ChangeState(_playFromTitleState);
            }
            else
            {
                _stateManager.ChangeState(_nextState);
            }
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            _inGameBackgroundAnimation.PlayAsync(ct).Forget();
            await _loadingScreen.HideAsync(ct);
        }
    }
}