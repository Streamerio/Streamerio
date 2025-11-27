// ============================================================================
// モジュール概要: UI のクリック入力を Rx ストリームへ変換し、SE 再生や連打制御を一括管理する。
// 外部依存: R3（Reactive Extensions 互換実装）、Cysharp.Threading.Tasks、Common.Audio。
// 使用例: ボタン View が ClickEventBinder を経由して Presenter にイベントを伝搬させる。
// ============================================================================

using System;
using System.Threading;
using Common.Audio;
using Cysharp.Threading.Tasks;
using R3;

namespace Common.UI.Click
{
    /// <summary>
    /// クリックイベントを外部へ配信するためのインターフェース。
    /// <para>
    /// 【理由】View が内部実装に依存せず、Observable ベースで Presenter へ通知できるようにする。
    /// </para>
    /// </summary>
    public interface IClickEventBinder : IDisposable
    {
        /// <summary>
        /// クリックイベントを購読可能な Observable。
        /// </summary>
        Observable<Unit> ClickEvent { get; }

        /// <summary>
        /// 指定された Observable をクリックイベントとしてバインドする。
        /// <para>
        /// 【理由】クリック入力のソースを抽象化し、Presenter 側がドラッグなど他の入力に切り替えても再利用できるようにする。
        /// </para>
        /// </summary>
        void BindClickEvent();
    }

    /// <summary>
    /// UI のクリック入力を仲介し、クリックイベントを配信するクラス。
    /// - クリック Observable を購読してイベント化
    /// - Throttle 処理による連打防止
    /// - クリック時に SE 再生
    /// </summary>
    public class ClickEventBinder<T> : IClickEventBinder
    {
        /// <summary>
        /// クリック連打を抑制する間隔。0.1 秒未満の入力は無視して意図しない多重実行を防ぐ。
        /// </summary>
        private const float _clickIntervalSec = 0.2f;

        // クリック時の効果音再生を委譲するファサード。UI 毎に実装を変えず再生方法を統一する。
        private readonly IAudioFacade _audioFacade;
        // 再生する SE の種類。ボタン種別に応じて差し替えられるよう外部から渡す。
        private readonly SEType _seType;
        
        // View 側の入力ストリーム。Throttle 等を適用して外部へ配信する。
        private Observable<T> _clickObservable;

        // バインドした購読をまとめて破棄するための CancellationTokenSource。
        private CancellationTokenSource _cts;

        private readonly Subject<Unit> _clickEvent;
        /// <inheritdoc/>
        public Observable<Unit> ClickEvent => _clickEvent;

        /// <summary>
        /// コンストラクタ。内部イベントストリームを初期化する。
        /// </summary>
        /// <param name="clickObservable">クリック操作を通知する Observable。例えば Button.onClick のラップを想定。</param>
        /// <param name="audioFacade">SE 再生を委譲するファサード。</param>
        /// <param name="seType">クリック時に再生する SE の種類。</param>
        public ClickEventBinder(Observable<T> clickObservable, IAudioFacade audioFacade, SEType seType)
        {
            _clickEvent = new Subject<Unit>();
            _clickObservable = clickObservable;
            
            _audioFacade = audioFacade;
            _seType = seType;
        }

        /// <inheritdoc/>
        public void BindClickEvent()
        {
            _cts = new CancellationTokenSource();

            _clickObservable
                .ThrottleFirst(TimeSpan.FromSeconds(_clickIntervalSec)) // 連打防止
                .Subscribe(_ =>
                {
                    // 効果音は非同期再生だが、UI 応答をブロックしないため Forget() で fire-and-forget にする。
                    _audioFacade.PlayAsync(_seType, _cts.Token).Forget();
                    // Presenter 側へ通知して後続処理をトリガーする。
                    _clickEvent.OnNext(Unit.Default);
                })
                .RegisterTo(_cts.Token);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // 購読をまとめて停止し、クリックイベントの多重通知を防ぐ。
            _cts?.Cancel();
            _cts?.Dispose();
            _clickEvent?.Dispose();
        }
    }
}
