using System.Threading;
using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class ChangeSceneState: IState
    {
        private readonly SceneType _changeSceneType;

        private ILoadingScreen _loadingScreen;
        protected ISceneManager SceneManager;

        public ChangeSceneState(SceneType changeSceneType)
        {
            _changeSceneType = changeSceneType;
        }
        
        [Inject]
        public void Construct(ILoadingScreen loadingScreen, ISceneManager sceneManager)
        {
            _loadingScreen = loadingScreen;
            SceneManager = sceneManager;
        }
        
        public virtual async UniTask EnterAsync(CancellationToken ct)
        {
            await _loadingScreen.ShowAsync(ct);
            await SceneManager.LoadSceneAsync(_changeSceneType);
        }
        
        public virtual async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}