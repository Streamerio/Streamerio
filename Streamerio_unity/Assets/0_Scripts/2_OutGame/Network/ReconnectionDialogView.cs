using Common.UI.Dialog;
using Common.UI.Part.Button;
using UnityEngine;
using VContainer;

namespace OutGame.Network
{
    public class ReconnectionDialogView : DialogViewBase, IReconnectionDialogView
    {
        private ICommonButton _reconnectButton;
        public ICommonButton ReconnectButton => _reconnectButton;
        
        [Inject]
        public void Construct([Key(ButtonType.Restart)]ICommonButton reconnectButton)
        {
            _reconnectButton = reconnectButton;
        }
    }
    
    public interface IReconnectionDialogView : IDialogView
    {
        ICommonButton ReconnectButton { get; }
    }
}
