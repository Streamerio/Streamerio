// モジュール概要:
// QR コード表示ダイアログの Presenter を実装し、生成サービスからの Sprite 更新を受けて View へ反映する。
// 依存関係: Common.QRCode.IQRCodeService で Sprite を生成し、Common.UI.Dialog.DialogPresenterBase の機能で共通ダイアログ挙動を利用する。
// 使用例: QRCodeDialogLifetimeScope が本 Presenter を登録し、表示時に UpdateSprite を呼び出して UI に QR コードを表示する。

using Common;
using Common.QRCode;
using Common.UI.Dialog;
using Common.UI.Dialog.Common;
using R3;
using UnityEngine;

namespace InGame.QRCode.UI
{
    public interface IQRCodeDialog : IDialog, IAttachable<QRCodeDialogContext>
    {
        
    }
    
    /// <summary>
    /// 【目的】QR コード表示ダイアログの View とサービスを接続し、生成された Sprite をリアクティブに適用する。
    /// 【理由】生成ロジックをサービスへ委譲しつつ、UI 更新のみを Presenter で担うことで責務を明確に分離するため。
    /// </summary>
    public class QRCodeDialogPresenter: DialogPresenterBase<IQRCodeDialogView, QRCodeDialogContext>, IQRCodeDialog
    {
        private IQRCodeService _qrCodeService;
        
        protected override void Bind()
        {
            base.Bind();
            
            _qrCodeService.URLProp
                .Subscribe(url =>
                {
                    View.SetUrlText(url);
                })
                .RegisterTo(GetCt());
            
            View.ClipButton.OnClickAsObservable
                .Subscribe(_ =>
                {
                    GUIUtility.systemCopyBuffer = _qrCodeService.URL;
                })
                .RegisterTo(GetCt());
        }

        protected override void AttachContext(QRCodeDialogContext context)
        {
            base.AttachContext(context);
            
            _qrCodeService = context.QRCodeService;
        }
    }
    
    public class QRCodeDialogContext : DialogContext<IQRCodeDialogView>
    {
        public IQRCodeService QRCodeService;
    }
}
