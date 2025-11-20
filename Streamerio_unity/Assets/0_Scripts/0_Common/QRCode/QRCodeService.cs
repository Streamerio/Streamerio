// モジュール概要:
// QR コード画像の生成と公開を担うサービス。ReactiveProperty を通して最新 Sprite を購読可能にする。
// 依存関係: QRCodeSpriteFactory で Sprite を生成し、R3 の ReactiveProperty で変更を通知する。
// 使用例: UI 層が IQRCodeService を注入し、UpdateSprite(url) を呼んで表示用 Image へバインドする。

using R3;
using UnityEngine;

namespace Common.QRCode
{
    /// <summary>
    /// 【目的】QR コード生成サービスの公開契約を定義する。
    /// 【理由】呼び出し側が ReactiveProperty 経由で Sprite を監視しつつ、更新 API を呼べるようにするため。
    /// </summary>
    public interface IQRCodeService
    {
        ReadOnlyReactiveProperty<string> URLProp { get; }
        
        string URL { get; }
        /// <summary>
        /// 【目的】最新の QR コード Sprite を購読可能な形で公開する。
        /// 【理由】UI 側がリアクティブにバインドし、更新のたびに表示を差し替えられるようにするため。
        /// </summary>
        ReadOnlyReactiveProperty<Sprite> SpriteProp { get; }

        /// <summary>
        /// 【目的】指定した URL 文字列から QR コード Sprite を生成し、公開プロパティへ反映する。
        /// 【理由】利用者が単一メソッドで QR コード更新を完結できるようにするため。
        /// </summary>
        /// <param name="url">【用途】QR コード化する URL やテキスト。</param>
        void UpdateURL(string url);
    }
    
    /// <summary>
    /// 【目的】QRCodeSpriteFactory を用いて Sprite を生成し、ReactiveProperty 経由で公開する標準実装。
    /// 【理由】サービス層で生成ロジックを共通化し、UI 側からは Subscribe するだけで利用できるようにするため。
    /// </summary>
    public class QRCodeService: IQRCodeService
    {
        /// <summary>
        /// 【目的】Sprite 生成を委譲するファクトリを保持する。
        /// 【理由】テキスト→Sprite 変換の詳細をサービス外へ切り出し、テストと再利用性を高めるため。
        /// </summary>
        private QRCodeSpriteFactory _factory;

        private readonly ReactiveProperty<string> _urlProp;
        public ReadOnlyReactiveProperty<string> URLProp => _urlProp;
        public string URL => _urlProp.Value;
        
        /// <summary>
        /// 【目的】最新の QR コード画像を保持し、購読者へ通知する。
        /// 【理由】UpdateSprite 呼び出しごとに UI を更新できるようにするため。
        /// </summary>
        private ReactiveProperty<Sprite> _sprite;
        public ReadOnlyReactiveProperty<Sprite> SpriteProp => _sprite;

        /// <summary>
        /// 【目的】QRCodeService を初期化し、ファクトリと ReactiveProperty を準備する。
        /// 【理由】DI でファクトリを受け取り、初回アクセス時でも空の ReactiveProperty を返せるようにするため。
        /// </summary>
        /// <param name="factory">【用途】テキストから Sprite を生成するファクトリ。</param>
        public QRCodeService(QRCodeSpriteFactory factory)
        {
            _factory = factory;
            _sprite = new ReactiveProperty<Sprite>();
            _urlProp = new ReactiveProperty<string>(string.Empty);
        }

        /// <summary>
        /// 【目的】指定した URL から QR コード Sprite を生成し、ReactiveProperty にセットする。
        /// 【理由】購読者へ即座に最新 Sprite を通知し、UI へ反映させるため。
        /// </summary>
        /// <param name="url">【用途】QR コードへ変換する文字列。</param>
        public void UpdateURL(string url)
        {
            _urlProp.Value = url;
            _sprite.Value = _factory.Create(url);
        }
    }
}
