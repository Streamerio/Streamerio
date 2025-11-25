using System.Collections.Generic;
using System.Threading;
using Common.Audio;
using Common.QRCode;
using Common.Save;
using Common.Scene;
using Common.UI.Animation;
using Common.UI.Dialog;
using Common.UI.Loading;
using Cysharp.Threading.Tasks;
using InGame.Setting;
using OutGame.Network;
using VContainer;

namespace Common.State
{
    public class InGameLoadingState: IState
    {
        private readonly IMasterData _masterData;
        
        private readonly IQRCodeService _qrCodeService;
        
        private readonly IDialogService _dialogService;
        
        private readonly IWebSocketManager _webSocketManager;
        
        private readonly IStateManager _stateManager;
        private readonly IState _toInGameState;
        
        [Inject]
        public InGameLoadingState(
            IMasterData masterData,
            IQRCodeService qrCodeService,
            IDialogService dialogService,
            IWebSocketManager webSocketManager,
            IStateManager stateManager,
            [Key(StateType.ToInGame)] IState toInGameState)
        {
            _masterData = masterData;
            
            _qrCodeService = qrCodeService;

            _dialogService = dialogService;
            
            _webSocketManager = webSocketManager;
            
            _stateManager = stateManager;
            _toInGameState = toInGameState;
        }
        public async UniTask EnterAsync(CancellationToken ct)
        {
            List<UniTask> tasks = new List<UniTask>();
            // マスターデータ未取得の場合は取得処理を追加
            if (!_masterData.IsDataFetched)
            {
                tasks.Add(_masterData.FetchDataAsync(ct));
            }
            // WebSocket接続処理
            tasks.Add(_webSocketManager.ConnectWebSocketAsync(null, ct));
            
            await UniTask.WhenAll(tasks);
            
            _qrCodeService.UpdateSprite(_webSocketManager.GetFrontUrl());
            
            // 両方成功していればゲーム画面へ遷移、どちらかが失敗していれば再接続ダイアログを表示
            if (_masterData.IsDataFetched && _webSocketManager.IsConnectedProp.CurrentValue)
            {
                _stateManager.ChangeState(_toInGameState);
            }
            else
            {
                _dialogService.OpenDisplayAsync<IReconnectionDialog>(ct).Forget();
            }
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _dialogService.CloseTopAsync(ct);
        }
    }
}