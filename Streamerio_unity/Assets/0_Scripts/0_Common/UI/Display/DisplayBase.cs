// モジュール概要:
// UI Display レイヤーで共有するインターフェースと基底クラスを提供し、Presenter ↔ View の責務分離を徹底する。
// 依存関係: Cysharp.Threading.Tasks で非同期開閉を扱い、VContainer のライフサイクル (IStartable / IAttachable) を統合。
// 使用例: 各 UI モジュールの Presenter が DisplayPresenterBase を継承し、IDisplay 契約経由で開閉処理を共通化する。

using Cysharp.Threading.Tasks;
using System.Threading;
using VContainer.Unity;

namespace Common.UI.Display
{
    /// <summary>
    /// 【目的】UI 表示制御の最小契約を定義する。
    /// 【提供機能】表示状態の取得と同期/非同期の表示・非表示 API を一箇所に集約し、Presenter 側の実装ブレを防ぐ。
    /// </summary>
    public interface IDisplay
    {
        /// <summary>
        /// 【目的】UI が表示中かどうかを取得し、Presenter の状態管理に使う。
        /// </summary>
        bool IsShow { get; }

        /// <summary>
        /// 【目的】アニメーション付きで UI を表示する。
        /// 【理由】画面遷移やシーン切替時でも安全に演出を止められるようにするため。
        /// </summary>
        /// <param name="ct">【用途】表示処理を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】表示完了まで待機する UniTask。</returns>
        UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】アニメーション無しで即時に UI を表示する。
        /// 【使用シナリオ】初期化時のレイアウト確認や演出スキップ時に利用する。
        /// </summary>
        void Show();

        /// <summary>
        /// 【目的】アニメーション付きで UI を非表示にする。
        /// 【理由】演出途中でシーンが破棄されるケースでも安全に終了させるため。
        /// </summary>
        /// <param name="ct">【用途】非表示処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】非表示演出の完了を待つ UniTask。</returns>
        UniTask HideAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】アニメーションを使わずに即座に UI を非表示とする。
        /// 【使用シナリオ】緊急停止や高速遷移など演出を待てない状況。
        /// </summary>
        void Hide();
    }

    /// <summary>
    /// 【目的】実際の描画処理を担う View 側の契約を定義する。
    /// 【提供機能】Presenter から呼び出される同期/非同期の表示・非表示 API を統一し、Canvas 操作の一貫性を保つ。
    /// </summary>
    public interface IDisplayView : ICommonUIBehaviour
    {
        /// <summary>
        /// 【目的】アニメーション付きで UI を表示する。
        /// </summary>
        /// <param name="ct">【用途】表示処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】表示演出の完了を表す UniTask。</returns>
        UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】アニメーションを用いずに即時表示する。
        /// </summary>
        void Show();

        /// <summary>
        /// 【目的】アニメーション付きで UI を非表示にする。
        /// </summary>
        /// <param name="ct">【用途】非表示処理を中断するための CancellationToken。</param>
        /// <returns>【戻り値】非表示演出の完了を表す UniTask。</returns>
        UniTask HideAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】アニメーションを用いずに即時非表示にする。
        /// </summary>
        void Hide();
    }

    /// <summary>
    /// 【目的】Presenter 層の共通ロジックを提供し、View への操作とライフサイクルを統合する。
    /// 【提供機能】表示状態管理、イベント初期化、データ購読、ライフサイクル Attach/Detach の枠組み。
    /// </summary>
    /// <typeparam name="TView">対応する View 型（IDisplayView 実装）。</typeparam>
    /// <typeparam name="TContext">表示初期化に必要な依存を束ねたコンテキスト型。</typeparam>
    public abstract class DisplayPresenterBase<TView, TContext> : IDisplay, IAttachable<TContext>, IStartable
        where TView : IDisplayView
    {
        /// <summary>
        /// 【目的】現在の表示状態を追跡し、再表示や多重操作を防ぐ。
        /// </summary>
        protected bool _isShow;

        /// <inheritdoc />
        public bool IsShow => _isShow;

        /// <summary>
        /// 【目的】Presenter が操作対象とする View インスタンスを保持する。
        /// 【理由】派生クラスが直接 View にアクセスできるよう参照を共有する。
        /// </summary>
        protected TView View;

        /// <summary>
        /// 【目的】Presenter 内の非同期処理をまとめてキャンセルするためのトークンソースを保持する。
        /// 【理由】Detach 時に処理を確実に停止し、ライフサイクル終了後のコールバックを防ぐため。
        /// </summary>
        private CancellationTokenSource _cts;

        /// <summary>
        /// 【目的】Presenter 内部で利用する CancellationToken を取得する。
        /// 【理由】複数の非同期処理をまとめて中断できるようにするため。
        /// </summary>
        /// <returns>【戻り値】Presenter が共有で利用する CancellationToken。</returns>
        protected CancellationToken GetCt() => _cts.Token;

        /// <summary>
        /// 【目的】ライフサイクル開始時にコンテキストを展開し、初期化を行う。
        /// 【処理概要】状態初期化→CancellationTokenSource 準備→AttachContext 呼び出し。
        /// 【理由】View/サービス参照を事前にセットし、Start 時のイベント設定へ備える。
        /// </summary>
        /// <param name="context">【用途】Presenter に渡される初期化データ。</param>
        public virtual void Attach(TContext context)
        {
            _isShow = false;
            _cts = new CancellationTokenSource();

            AttachContext(context);
        }

        /// <summary>
        /// 【目的】コンテキストに含まれる View や依存オブジェクトをフィールドへ割り当てる。
        /// 【理由】Start 内のイベント/購読処理より前に、必要な参照を揃えておくため。
        /// </summary>
        /// <param name="context">【用途】Presenter が利用する依存を束ねたデータ。</param>
        protected abstract void AttachContext(TContext context);

        /// <summary>
        /// 【目的】VContainer の IStartable としてイベント登録とデータ購読を開始する。
        /// 【処理概要】SetEvent→Bind の順に呼び出すことで、イベント接続→データ購読の流れを統一。
        /// </summary>
        public void Start()
        {
            SetEvent();
            Bind();
        }

        /// <summary>
        /// 【目的】ユーザー操作や View からのイベントを Presenter へ接続する。
        /// 【拡張性】派生クラスが override して入力系の初期化を実装する。
        /// </summary>
        protected virtual void SetEvent() { }

        /// <summary>
        /// 【目的】モデル層やシグナルからの更新を購読する。
        /// 【拡張性】派生クラスで override し、リアクティブ購読やデータバインディングを実装する。
        /// </summary>
        protected virtual void Bind() { }

        /// <summary>
        /// 【目的】アニメーション付きで View を表示し、操作可能状態へ遷移させる。
        /// 【理由】Presenter の状態フラグと View の表示状態を同期させ、多重起動を防ぐため。
        /// </summary>
        /// <param name="ct">【用途】表示演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】表示演出が完了したことを示す UniTask。</returns>
        public virtual async UniTask ShowAsync(CancellationToken ct)
        {
            _isShow = true;
            await View.ShowAsync(ct);
            View.SetInteractable(true);
        }

        /// <summary>
        /// 【目的】アニメーション無しで表示し、操作可能状態へ遷移させる。
        /// 【理由】演出を挟まずにレイアウト確認やデバッグを行いたいケースに対応するため。
        /// </summary>
        public virtual void Show()
        {
            _isShow = true;
            View.Show();
            View.SetInteractable(true);
        }

        /// <summary>
        /// 【目的】アニメーション付きで非表示にし、操作を無効化する。
        /// 【理由】非表示演出中の誤操作を防ぎ、終了後に状態フラグを確実に更新するため。
        /// </summary>
        /// <param name="ct">【用途】非表示演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】非表示演出が完了したことを示す UniTask。</returns>
        public virtual async UniTask HideAsync(CancellationToken ct)
        {
            View.SetInteractable(false);
            _isShow = false;
            await View.HideAsync(ct);
        }

        /// <summary>
        /// 【目的】アニメーション無しで即時に非表示へ移行する。
        /// 【理由】シーン破棄や高速遷移でも遷移待ちを発生させずにクリーンアップするため。
        /// </summary>
        public virtual void Hide()
        {
            View.SetInteractable(false);
            _isShow = false;
            View.Hide();
        }

        /// <summary>
        /// 【目的】ライフサイクル終了時に非同期処理を中断し、リソースを解放する。
        /// 【理由】シーン遷移やスコープ破棄時に Task が残留することを防ぐ。
        /// </summary>
        public virtual void Detach()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }

    /// <summary>
    /// 【目的】View 側の標準基底を定義し、UIBehaviourBase と IDisplayView を結び付ける。
    /// 【提供機能】表示/非表示メソッドを抽象化し、派生クラスが演出実装に専念できるようにする。
    /// </summary>
    public abstract class DisplayViewBase : UIBehaviourBase, IDisplayView, IInitializable
    {
        public virtual void Initialize()
        {
            
        }
        
        /// <summary>
        /// 【目的】アニメーション付きで表示する処理を派生クラスへ委譲する。
        /// 【理由】表示演出の具体的な実装は各 View ごとに異なるため、抽象メソッドとして表現する。
        /// </summary>
        /// <param name="ct">【用途】表示演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】表示演出が完了したことを示す UniTask。</returns>
        public abstract UniTask ShowAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】アニメーション無しで即時表示する処理を派生クラスへ委譲する。
        /// 【理由】即時表示の具体的な制御方法を View ごとにカスタマイズできるようにするため。
        /// </summary>
        public abstract void Show();

        /// <summary>
        /// 【目的】アニメーション付きで非表示にする処理を派生クラスへ委譲する。
        /// 【理由】非表示演出の仕様が View ごとに異なるケースへ柔軟に対応するため。
        /// </summary>
        /// <param name="ct">【用途】非表示演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】非表示演出が完了したことを示す UniTask。</returns>
        public abstract UniTask HideAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】アニメーション無しで即時非表示にする処理を派生クラスへ委譲する。
        /// 【理由】緊急停止やシーン破棄時に即座に描画を落としたいニーズへ対応するため。
        /// </summary>
        public abstract void Hide();
    }
}
