// モジュール概要:
// QRCoder ライブラリを利用してテキストから QR コード画像を生成し、Unity の Sprite として扱える形へ変換するファクトリ。
// 依存関係: QRCoder (PngByteQRCode) で PNG バイト列を生成し、Texture2D / Sprite によって Unity へ取り込む。
// 使用例: QRCodeService が URL 文字列から Sprite を取得し、UI.Image などで表示する。

using QRCoder;
using UnityEngine;

namespace Common.QRCode
{
    /// <summary>
    /// 【目的】テキストを QR コード画像へ変換し、Sprite として返却する。
    /// 【理由】外部サービスの URL などをアプリ内で共有する際に、即座に表示可能な形式へ整形するため。
    /// </summary>
    public class QRCodeSpriteFactory
    {
        /// <summary>
        /// 【目的】引数のテキストから QR コードを生成し、Sprite を作成する。
        /// 【理由】UI 表示で扱いやすい Sprite 形式へ変換し、呼び出し側がテクスチャ処理を意識せずに済むようにするため。
        /// </summary>
        /// <param name="qrText">【用途】QR コード化したい文字列。URL や ID など。</param>
        /// <returns>【戻り値】生成した QR コード画像を含む Sprite。</returns>
        public Sprite Create(string qrText)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            byte[] pngData = qrCode.GetGraphic(20);

            // PNG バイト列を Texture2D にデコードし、Unity で扱える形式へ変換する。
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(pngData);

            // Texture2D を中央アンカーの Sprite へ変換して返却する。
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}
