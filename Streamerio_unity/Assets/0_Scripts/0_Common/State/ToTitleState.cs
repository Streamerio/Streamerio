using System.Threading;
using Common.Scene;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class ToTitleState: ChangeSceneState
    {
        private IWebSocketManager _webSocketManager;
        
        public ToTitleState() : base(SceneType.Title)
        {
        }
        
        [Inject]
        public void Construct(IWebSocketManager webSocketManager)
        {
            _webSocketManager = webSocketManager;
        }
        
        public override async UniTask ExitAsync(CancellationToken ct)
        {
            _webSocketManager.DisconnectWebSocket();
            await UniTask.WaitForEndOfFrame(cancellationToken: ct);
        }
    }
}