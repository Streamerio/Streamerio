using Common.UI.Dialog;
using Common.UI.Part.Button;
using TMPro;
using UnityEngine;
using VContainer;
using Cysharp.Text;

namespace InGame.QRCode.UI
{
    public class QRCodeDialogView: DialogViewBase, IQRCodeDialogView
    {
        [SerializeField, Tooltip("URLテキスト")]
        private TMP_Text _urlText;
        
        // private ICommonButton _clipButton;
        // public ICommonButton ClipButton => _clipButton;
        
        // [Inject]
        // public void Construct([Key(ButtonType.Default)]ICommonButton clipButton)
        // {
        //     _clipButton = clipButton;
        // }
        
        public void SetUrlText(string url)
        {
            _urlText.SetText(url);
        }
    }
    
    public interface IQRCodeDialogView : IDialogView
    {
        // ICommonButton ClipButton { get; }
        
        void SetUrlText(string url);
    }
}