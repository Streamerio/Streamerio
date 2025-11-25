using System.Threading;
using Common.Scene;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class ToResultState: ChangeSceneState
    {
        private IWebSocketManager _webSocketManager;

        [Inject]
        public ToResultState(IWebSocketManager webSocketManager) : base(SceneType.ResultScene)
        {
            _webSocketManager = webSocketManager;
        }
        
        public override async UniTask ExitAsync(CancellationToken ct)
        {
            await _webSocketManager.GameEndAsync();
        }
    }
}