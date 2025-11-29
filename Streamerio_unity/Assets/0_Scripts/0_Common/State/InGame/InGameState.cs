using System.Threading;
using Common.Audio;
using Cysharp.Threading.Tasks;
using InGame.Setting;
using VContainer;
using ITimer = InGame.UI.Timer.ITimer;

namespace Common.State
{
    public class InGameState: IState
    {
        private readonly IAudioFacade _audioFacade;

        private readonly ITimer _timer;
        
        private readonly IWebSocketManager _webSocketManager;
        
        private readonly IInGameSetting _inGameSetting;
        
        [Inject]
        public InGameState(IAudioFacade audioFacade, ITimer timer, IWebSocketManager webSocketManager, IInGameSetting inGameSetting)
        {
            _audioFacade = audioFacade;
            _timer = timer;
            _webSocketManager = webSocketManager;
            _inGameSetting = inGameSetting;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _webSocketManager.GameStartAsync();
            _timer.StartCountdownTimer();
            _inGameSetting.IsGame = true;
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            _inGameSetting.IsGame = false;
            _timer.StopCountdownTimer();
            _audioFacade.StopBGMAsync(ct).Forget();
            _audioFacade.StopSEAsync(ct).Forget();
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}