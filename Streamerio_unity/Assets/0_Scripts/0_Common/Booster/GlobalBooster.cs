using System.Threading;
using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Common.Booster
{
    public class GlobalBooster: IAsyncStartable
    {
        private IMasterData _masterData;
        private ILoadingScreen _loadingScreen;
        private ISceneManager _sceneManager;
        
        [Inject]
        public GlobalBooster(IMasterData masterData, ILoadingScreen loadingScreen, ISceneManager sceneManager)
        {
            _masterData = masterData;
            _loadingScreen = loadingScreen;
            _sceneManager = sceneManager;
        }
        
        public async UniTask StartAsync(CancellationToken ct)
        {
            _loadingScreen.Show();
            _sceneManager.LoadSceneAsync(SceneType.Title).Forget();
            await _masterData.FetchDataAsync(ct);
        }
    }
}