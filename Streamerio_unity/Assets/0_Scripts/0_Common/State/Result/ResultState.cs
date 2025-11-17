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
        
        [Inject]
        public void Construct(ILoadingScreen loadingScreen, IWindowService overlayService)
        {
            _loadingScreen = loadingScreen;
            _windowService = overlayService;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _loadingScreen.HideAsync(ct);
            await _windowService.OpenDisplayAsync<IResultWindow>(ct);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _windowService.CloseTopAsync(ct);
        }
    }
}