using System.Threading;
using Common.Audio;
using Common.QRCode;
using Common.Save;
using Common.Scene;
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
        
        private readonly IQRCodeService _qrCodeService;
        
        private readonly IStateManager _stateManager;
        private readonly IState _firstPlayState;
        private readonly IState _playFromTitleState;
        private readonly IState _nextState;
        
        [Inject]
        public InGameStartState(
            IPlayDataSaveFacade playDataSaveFacade,
            ILoadingScreen loadingScreen,
            ISceneManager sceneManager,
            IAudioFacade audioFacade,
            IInGameSetting inGameSetting,
            IQRCodeService qrCodeService,
            IStateManager stateManager,
            [Key(StateType.FirstPlay)] IState firstPlayState,
            [Key(StateType.PlayFromTitle)] IState playFromTitleState,
            [Key(StateType.InGame)] IState nextState)
        {
            _playDataSaveFacade = playDataSaveFacade;
            
            _loadingScreen = loadingScreen;
            _sceneManager = sceneManager;
            
            _audioFacade = audioFacade;
            
            _inGameSetting = inGameSetting;
            
            _qrCodeService = qrCodeService;
            
            _stateManager = stateManager;
            _firstPlayState = firstPlayState;
            _playFromTitleState = playFromTitleState;
            _nextState = nextState;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            _qrCodeService.UpdateSprite("https://www.google.com/");
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

            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _loadingScreen.HideAsync(ct);
        }
    }
}