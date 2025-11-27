// モジュール概要:
// 共通ボタンの Presenter と契約を定義し、 Pointer / Click イベントを購読して View 演出とクリック通知を制御する。
// 依存関係: R3 の Observable/Triggers で入力を監視し、ClickEventBinder が効果音などの副作用を内包する。
// 使用例: ButtonLifetimeScope で Wiring され、他 UI から ICommonButton を通じてボタン操作・クリック購読を行う。

using System.Threading;
using Common.UI.Click;
using R3;
using R3.Triggers;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 【目的】共通ボタンが外部へ提供する最小限の操作・イベント契約を定義する。
    /// 【理由】Presenter の内部実装へ依存せず、他 UI やドメイン層から一貫した API で扱えるようにするため。
    /// </summary>
    public interface ICommonButton: IAttachable<CommonButtonContext>
    {
        /// <summary>
        /// 【目的】ボタンクリックを通知するリアクティブストリームを提供する。
        /// 【理由】他 UI がクリックイベントを購読し、Presenter 内部へ依存せずにボタン反応へフックできるようにするため。
        /// </summary>
        Observable<Unit> OnClickAsObservable { get; }

        /// <summary>
        /// 【目的】ボタンの有効状態を切り替え、同時にビジュアル状態をリセットする。
        /// 【理由】単一の呼び出しで GameObject の表示と演出リセットを完結させ、利用側の手順を簡略化するため。
        /// </summary>
        /// <param name="isActive">【用途】true で表示、false で非表示。</param>
        void SetActive(bool isActive);
    }

    /// <summary>
    /// 共通ボタンの Presenter 実装。
    /// <para>
    /// - Unity UI Button や Pointer イベントを購読し、View へ通知する。<br/>
    /// - クリック処理は IClickEventBinder 経由で管理（SE 再生・クリック判定などを内部に隠蔽）。<br/>
    /// - View（IButtonView）に対して押下／解放／ホバー等の状態変更を指示する。
    /// </para>
    /// </summary>
    /// <remarks>
    /// Attach/Detach は Wiring（DI EntryPoint）から呼ばれ、Presenter は純粋な UI ロジックに専念する。
    /// Button や GameObject、EventTrigger などの Unity 実体への依存は CommonButtonContext に集約。
    /// </remarks>
    /// <summary>
    /// 【目的】Unity UI Button のイベントを購読し、View への演出指示とクリック通知を担う。
    /// 【理由】入力経路と演出・副作用を Presenter に集約することで、View や呼び出し元からロジックを分離する。
    /// </summary>
    public class CommonButtonPresenter : ICommonButton
    {
        /// <summary>
        /// 【目的】実際のクリック操作を受け取る Unity ボタン参照を保持する。
        /// 【理由】OnClick ストリームの発信源として利用しつつ、SetActive 時の GameObject 操作とも整合させるため。
        /// </summary>
        private UnityEngine.UI.Button _button;
        /// <summary>
        /// 【目的】クリック時の副作用（SE 再生など）を一括管理するバインダーを保持する。
        /// 【理由】Presenter から直接副作用を実装すると重複が増えるため、Binder に委譲して再利用性を高める。
        /// </summary>
        private IClickEventBinder _clickEventBinder;
        /// <summary>
        /// 【目的】アニメーションを担当する View を保持し、Pointer イベントへの指示を出す。
        /// 【理由】Presenter が UI の状態遷移を統制し、複数の View 実装にも差し替えられるようにするため。
        /// </summary>
        private IButtonView _view;
        /// <summary>
        /// 【目的】購読解除と非同期処理キャンセルをまとめるトークンソースを保持する。
        /// 【理由】Detach 時に確実に購読を解放し、メモリリークや無効なコールバックを防ぐため。
        /// </summary>
        private CancellationTokenSource _cts;

        /// <inheritdoc />
        public Observable<Unit> OnClickAsObservable => _clickEventBinder.ClickEvent;

        /// <summary>
        /// 【目的】Button/View/Binder/EventTrigger をコンテキストから受け取り、Presenter の動作準備を行う。
        /// 【理由】Unity コンポーネントと抽象 View を疎結合に保ちつつ、DI で一括供給できるようにするため。
        /// </summary>
        /// <param name="context">【用途】ボタン構成要素をまとめたデータコンテナ。</param>
        public void Attach(CommonButtonContext context)
        {
            _button          = context.Button;
            _clickEventBinder= context.ClickEventBinder;
            _clickEventBinder.BindClickEvent();
            _view            = context.View;

            // GameObject が破棄されたときに購読解除される CancellationToken
            _cts = new CancellationTokenSource();

            Bind(context.EventTrigger);
            _view.ResetButtonState();
        }

        /// <summary>
        /// 【目的】クリック購読と CancellationToken を解放し、Presenter をリセットする。
        /// 【理由】スコープ終了後にイベント発火が残留すると例外が発生するため、リソースを確実に破棄する。
        /// </summary>
        public void Detach()
        {
            _clickEventBinder?.Dispose();

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        /// <summary>
        /// 【目的】Pointer イベントと OnClick を購読し、View へ対応するアニメーションを指示する。
        /// 【理由】ユーザー操作をリアクティブに処理し、アニメーションと状態リセットを自動化するため。
        /// </summary>
        /// <param name="eventTrigger">【用途】Pointer 各種イベントを提供する Observable トリガー。</param>
        private void Bind(ObservableEventTrigger eventTrigger)
        {
            eventTrigger.OnPointerDownAsObservable()
                .SubscribeAwait(async (_, ct) => await _view.PlayPointerDownAsync(ct))
                .RegisterTo(_cts.Token);

            eventTrigger.OnPointerUpAsObservable()
                .SubscribeAwait(async (_, ct) => await _view.PlayPointerUpAsync(ct))
                .RegisterTo(_cts.Token);

            eventTrigger.OnPointerEnterAsObservable()
                .SubscribeAwait(async (_, ct) => await _view.PlayPointerEnterAsync(ct))
                .RegisterTo(_cts.Token);

            eventTrigger.OnPointerExitAsObservable()
                .SubscribeAwait(async (_, ct) => await _view.PlayPointerExitAsync(ct))
                .RegisterTo(_cts.Token);

            OnClickAsObservable
                .SubscribeAwait(async (_, ct) => await _view.PlayPointerClickAsync(ct))
                .RegisterTo(_cts.Token);
        }

        /// <inheritdoc />
        public void SetActive(bool isActive)
        {
            _view.GameObject.SetActive(isActive);
            _view.ResetButtonState();
        }
    }

    /// <summary>
    /// 【目的】CommonButtonPresenter に必要な Unity コンポーネントとバインダーをまとめる。
    /// 【理由】Wiring で依存を一括注入できるようにし、Presenter 側の取得ロジックを排除するため。
    /// </summary>
    public class CommonButtonContext
    {
        /// <summary>
        /// 【目的】操作対象の Unity Button を提供する。
        /// 【理由】OnClick ストリームと GameObject アクセスの起点にするため。
        /// </summary>
        public UnityEngine.UI.Button Button;

        /// <summary>
        /// 【目的】Pointer イベントを Observable として提供する。
        /// 【理由】Presenter が押下・ホバーを非同期処理で扱えるようにするため。
        /// </summary>
        public ObservableEventTrigger EventTrigger;

        /// <summary>
        /// 【目的】クリック時に副作用を実行するバインダーを提供する。
        /// 【理由】SE 再生や多重クリック抑制などの処理を Presenter から分離するため。
        /// </summary>
        public IClickEventBinder ClickEventBinder;

        /// <summary>
        /// 【目的】見た目・アニメーションを担当する View を提供する。
        /// 【理由】Presenter から演出指示を出す対象を統一し、差し替え可能にするため。
        /// </summary>
        public IButtonView View;
    }
}
