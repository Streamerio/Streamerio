using System.Threading;
using Common.Scene;
using Cysharp.Threading.Tasks;

namespace Common.State
{
    public class ToInGameState: ChangeSceneState
    {
        public ToInGameState() : base(SceneType.GameScene)
        {
        }
        
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            SceneManager.UpdateRestartFlag(false);
            await SceneManager.LoadSceneAsync(SceneType.GameScene);
        }
    }
}