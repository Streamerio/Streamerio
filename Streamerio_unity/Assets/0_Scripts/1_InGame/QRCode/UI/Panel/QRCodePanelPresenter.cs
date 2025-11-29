using System;
using System.Threading;
using Common.QRCode;
using R3;
using VContainer;
using VContainer.Unity;

namespace InGame.QRCode.UI.Panel
{
    public class QRCodePanelPresenter: IStartable, IDisposable
    {
        /// <summary>
        /// 【目的】QR コード画像を生成・管理するサービスを保持する。
        /// 【理由】SpriteProp を購読して View を更新し、サービス側の生成ロジック再利用を可能にするため。
        /// </summary>
        private readonly IQRCodeService _qrCodeService;

        private readonly IQRCodePanelView _view;

        private CancellationTokenSource _cts;
        
        [Inject]
        public QRCodePanelPresenter(IQRCodeService qrCodeService, IQRCodePanelView view)
        {
            _qrCodeService = qrCodeService;
            _view = view;
            
            _cts = new CancellationTokenSource();
        }
        
        public void Start()
        {
            Bind();
        }
        
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        
        /// <summary>
        /// 【目的】QR コード Sprite の更新を購読し、ビューへ反映する。
        /// 【理由】サービス側で URL 更新が行われた際に UI が即座に切り替わるようにするため。
        /// </summary>
        private void Bind()
        {
            _qrCodeService.SpriteProp
                .DistinctUntilChanged()
                .Where(sprite => sprite != null)
                .Subscribe(sprite =>
                {
                    _view.SetQRCodeSprite(sprite);
                })
                .RegisterTo(_cts.Token);
        }
    }
}