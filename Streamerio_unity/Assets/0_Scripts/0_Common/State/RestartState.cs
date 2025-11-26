using System.Threading;
using Common.Scene;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class RestartState: ChangeSceneState
    {
        public RestartState(SceneType changeSceneType) : base(changeSceneType)
        {
        }
        
        
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            SceneManager.UpdateRestartFlag(true);
            await base.EnterAsync(ct);
        }
    }
}