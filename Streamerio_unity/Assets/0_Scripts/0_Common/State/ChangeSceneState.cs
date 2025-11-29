using System.Threading;
using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class ChangeSceneState: IState
    {
        protected readonly SceneType ChangeSceneType;

        protected ILoadingScreen LoadingScreen;
        protected ISceneManager SceneManager;

        public ChangeSceneState(SceneType changeSceneType)
        {
            ChangeSceneType = changeSceneType;
        }
        
        [Inject]
        public void Construct(ILoadingScreen loadingScreen, ISceneManager sceneManager)
        {
            LoadingScreen = loadingScreen;
            SceneManager = sceneManager;
        }
        
        public virtual async UniTask EnterAsync(CancellationToken ct)
        {
            await LoadingScreen.ShowAsync(ct);
            await SceneManager.LoadSceneAsync(ChangeSceneType);
        }
        
        public virtual async UniTask ExitAsync(CancellationToken ct)
        {
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}