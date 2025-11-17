using System.Threading;
using Common.UI.Display.Overlay;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using OutGame.GameOver.UI;
using VContainer;

namespace Common.State
{
    public class GameOverState: IState
    {
        private ILoadingScreen _loadingScreen;
        private IOverlayService _overlayService;
        
        [Inject]
        public void Construct(ILoadingScreen loadingScreen, IOverlayService overlayService)
        {
            _loadingScreen = loadingScreen;
            _overlayService = overlayService;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _loadingScreen.HideAsync(ct);
            await _overlayService.OpenDisplayAsync<IGameOverOverlay>(ct);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _loadingScreen.ShowAsync(ct);
        }
    }
}