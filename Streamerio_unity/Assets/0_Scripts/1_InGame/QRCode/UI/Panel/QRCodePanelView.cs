using Alchemy.Inspector;
using Common.UI;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.QRCode.UI.Panel
{
    [RequireComponent(typeof(Image))]
    public class QRCodePanelView: UIBehaviourBase, IQRCodePanelView
    {
        /// <summary>
        /// 【目的】表示対象となる Image コンポーネントを Inspector から設定する。
        /// 【理由】ダイアログのプレハブごとに差し替えられるようにし、コード側での参照取得を不要にするため。
        /// </summary>
        [SerializeField, ReadOnly]
        private Image _qrCodeImage;
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // Editor 上で Image を自動取得
            _qrCodeImage ??= GetComponent<Image>();
        }
#endif
        
        /// <summary>
        /// 【目的】引数で受け取った QR コード Sprite を Image に適用する。
        /// 【理由】Presenter から生成済み Sprite を渡してもらい、View 側では単純な差し替えのみで完結させるため。
        /// </summary>
        /// <param name="sprite">【用途】表示する QR コード画像。null の場合は現状を維持する前提。</param>
        public void SetQRCodeSprite(Sprite sprite)
        {
            _qrCodeImage.sprite = sprite;
        }
    }
    
    public interface IQRCodePanelView
    {
        void SetQRCodeSprite(Sprite sprite);
    }
}