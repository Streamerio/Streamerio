using System.Threading;
using Common.UI.Display.Window;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.Result.UI;
using VContainer;

namespace Common.State
{
    public class ResultState: IState
    {
        private ILoadingScreen _loadingScreen;
        private IWindowService _windowService;
        private IWebSocketManager _webSocketManager;
        
        [Inject]
        public void Construct(ILoadingScreen loadingScreen, IWindowService overlayService, IWebSocketManager webSocketManager)
        {
            _loadingScreen = loadingScreen;
            _windowService = overlayService;
            _webSocketManager = webSocketManager;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await UniTask.WhenAny(UniTask.WaitWhile(() => _webSocketManager.GameEndSummary == null, cancellationToken: ct),
                UniTask.WaitForSeconds(5f, cancellationToken: ct));
            await _loadingScreen.HideAsync(ct);
            await _windowService.OpenDisplayAsync<IResultWindow>(ct);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _loadingScreen.ShowAsync(ct);
        }
    }
}