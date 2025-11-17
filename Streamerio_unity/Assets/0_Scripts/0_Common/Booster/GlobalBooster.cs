using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Common.Booster
{
    public class GlobalBooster: IStartable
    {
        private ILoadingScreen _loadingScreen;
        private ISceneManager _sceneManager;
        
        [Inject]
        public GlobalBooster(ILoadingScreen loadingScreen, ISceneManager sceneManager)
        {
            _loadingScreen = loadingScreen;
            _sceneManager = sceneManager;
        }
        
        public void Start()
        {
            _loadingScreen.Show();
            _sceneManager.LoadSceneAsync(SceneType.Title).Forget();
        }
    }
}