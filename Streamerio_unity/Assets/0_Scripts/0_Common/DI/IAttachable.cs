// モジュール概要:
// DI 経由で提供されるオブジェクト間の接続契約を定義し、Attach/Detach ライフサイクルで依存を受け渡す。
// 用途: Presenter やサービスが View・コンテキストを受け取り、スコープ終了時にクリーンアップする標準インターフェースを提供する。

namespace Common
{
    /// <summary>
    /// 【目的】特定の <typeparamref name="TContext"/> を用いて依存を受け渡し、接続/切断ライフサイクルを統一する。
    /// 【理由】Presenter やサービスがコンストラクタ外で View・モデルを受け取り、スコープ終了時に確実に解放できるようにするため。
    /// </summary>
    /// <typeparam name="TContext">
    /// 【用途】Attach 時に渡されるコンテキスト情報の型。複数の依存や初期化パラメータを束ね、生成箇所と利用箇所を疎結合にする。
    /// </typeparam>
    public interface IAttachable<TContext>
    {
        /// <summary>
        /// 【目的】指定された <paramref name="context"/> を用いて対象を接続し、初期化を実施する。
        /// 【理由】生成元が保持する依存を受け取り、イベント購読や View バインドなど実行時にしか行えない初期化をまとめるため。
        /// </summary>
        /// <param name="context">【用途】接続に必要なコンテキスト情報。View、サービス、初期データなどをまとめて渡す。</param>
        void Attach(TContext context);

        /// <summary>
        /// 【目的】現在の接続を解除し、占有しているリソースを解放する。
        /// 【理由】スコープ破棄やシーン遷移時にイベント購読や参照を確実にクリアし、リークを防ぐため。
        /// </summary>
        void Detach();
    }
}
