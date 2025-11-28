// モジュール概要:
// Display スタックを管理し、UI 開閉操作を統一提供するサービス。
// 依存関係: IDisplayCache から Presenter を取得し、R3 の ReactiveProperty で空状態を公開する。
// 使用例: DialogService などが本サービスを継承または利用し、UI 画面のプッシュ/ポップを実装する。

using Cysharp.Threading.Tasks;
using R3;
using System.Collections.Generic;
using System.Threading;

namespace Common.UI.Display
{
    /// <summary>
    /// 【目的】UI Display のスタック操作を外部へ提供する契約。
    /// 【理由】同期/非同期の開閉操作と状態公開を一箇所に集約し、利用側の実装を簡潔にするため。
    /// </summary>
    public interface IDisplayService : IAttachable<DisplayServiceContext>
    {
        /// <summary>
        /// 【目的】Display スタックが空かどうかを購読可能な形で提供する。
        /// </summary>
        ReadOnlyReactiveProperty<bool> IsEmptyProp { get; }

        /// <summary>
        /// 【目的】非同期に Display を開く。
        /// 【理由】アニメーションが完了するまで待機し、呼び出し元で演出完了を把握できるようにするため。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】開きたい Display (Presenter) 型。</typeparam>
        /// <param name="ct">【用途】表示中にキャンセルしたい場合のトークン。</param>
        /// <returns>【戻り値】開いた Display インスタンス。</returns>
        UniTask<TDisplay> OpenDisplayAsync<TDisplay>(CancellationToken ct)
            where TDisplay : class, IDisplay;

        /// <summary>
        /// 【目的】Display を開き、閉じられるまで待機する。
        /// 【理由】モーダル表示など処理継続を UI 操作に委ねたいユースケースをサポートするため。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】待機対象となる Display 型。</typeparam>
        /// <param name="ct">【用途】待機を中断したい場合のトークン。</param>
        /// <returns>【戻り値】閉じ終わるまでの待機を表す UniTask。</returns>
        UniTask OpenAndWaitCloseAsync<TDisplay>(CancellationToken ct)
            where TDisplay : class, IDisplay;

        /// <summary>
        /// 【目的】同期的に Display を開く。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】開きたい Display (Presenter) 型。</typeparam>
        void OpenDisplay<TDisplay>()
            where TDisplay : class, IDisplay;

        /// <summary>
        /// 【目的】最前面の Display を非同期で閉じる。
        /// 【理由】演出完了まで待機することで、背後の画面を安全に再表示するため。
        /// </summary>
        /// <param name="ct">【用途】非表示処理をキャンセルするためのトークン。</param>
        /// <returns>【戻り値】閉じ終えるまでの待機を表す UniTask。</returns>
        UniTask CloseTopAsync(CancellationToken ct);

        /// <summary>
        /// 【目的】最前面の Display を即座に閉じる。
        /// </summary>
        void CloseTop();

        /// <summary>
        /// 【目的】スタックに積まれた Display をすべて閉じる。
        /// </summary>
        void CloseAll();
    }
    
    /// <summary>
    /// 【目的】Display の開閉とスタック管理を実装する標準サービス。
    /// 【理由】UI 表示制御の共通ロジックを集約し、各ドメインで再利用できるようにするため。
    /// </summary>
    public class DisplayService : IDisplayService
    {
        /// <summary>
        /// 【目的】Display インスタンスの生成・再利用を司るキャッシュを保持する。
        /// 【理由】毎回プレハブを Instantiate せず、既存インスタンスを再活用して GC とロード時間を抑えるため。
        /// </summary>
        private IDisplayCache _displayCache;
        /// <summary>
        /// 【目的】表示順序を追跡するスタックを保持する。
        /// 【理由】モーダルや積層 UI の戻り順序を LIFO で制御するため。
        /// </summary>
        private Stack<IDisplay> _displayStack;
        /// <summary>
        /// 【目的】Display が空かどうかをリアクティブに通知するプロパティを保持する。
        /// 【理由】呼び出し側が状態変化を監視し、UI 表示有無で制御を切り替えられるようにするため。
        /// </summary>
        private ReactiveProperty<bool> _isEmptyProp;

        /// <inheritdoc />
        public ReadOnlyReactiveProperty<bool> IsEmptyProp => _isEmptyProp;

        /// <summary>
        /// 【目的】サービスに DisplayCache を紐付け、スタックと状態を初期化する。
        /// 【理由】DI コンテキストからキャッシュを受け取り、以降の操作で再利用するため。
        /// </summary>
        /// <param name="context">【用途】DisplayCache などサービス初期化に必要な依存を束ねたコンテキスト。</param>
        public void Attach(DisplayServiceContext context)
        {
            _displayCache = context.Cache;
            _displayStack = new Stack<IDisplay>();
            _isEmptyProp = new ReactiveProperty<bool>(true);
        }
        
        /// <summary>
        /// 【目的】リソースを解放し、参照をクリアする。
        /// 【理由】LifetimeScope 終了時にスタックや購読が残らないようにするため。
        /// </summary>
        public void Detach()
        {
            _displayCache = null;
            _displayStack = null;
            _isEmptyProp.Dispose();
            _isEmptyProp = null;
        }

        /// <inheritdoc />
        public async UniTask<TDisplay> OpenDisplayAsync<TDisplay>(CancellationToken ct)
            where TDisplay : class, IDisplay
        {
            Debug.Log("DisplayService OpenDisplayAsync");
            _isEmptyProp.Value = false;

            if (_displayStack.TryPeek(out var current))
            {
                await current.HideAsync(ct);
            }

            TDisplay display = GetDisplay<TDisplay>();

            await display.ShowAsync(ct);
            return display;
        }

        /// <inheritdoc />
        public async UniTask OpenAndWaitCloseAsync<TDisplay>(CancellationToken ct)
            where TDisplay : class, IDisplay
        {
            var display = await OpenDisplayAsync<TDisplay>(ct);
            await UniTask.WaitWhile(() => display.IsShow, cancellationToken: ct);
        }

        /// <inheritdoc />
        public virtual void OpenDisplay<TDisplay>()
            where TDisplay : class, IDisplay
        {
            _isEmptyProp.Value = false;

            if (_displayStack.TryPeek(out var current))
            {
                current.Hide();
            }

            GetDisplay<TDisplay>().Show();
        }

        /// <inheritdoc />
        public virtual async UniTask CloseTopAsync(CancellationToken ct)
        {
            if (!_displayStack.TryPop(out var current))
                return;

            await current.HideAsync(ct);
            await ShowTopAsync(ct);

            if (_displayStack.Count == 0) _isEmptyProp.Value = true;
        }

        /// <inheritdoc />
        public virtual void CloseTop()
        {
            if (!_displayStack.TryPop(out var current))
                return;

            current.Hide();
            ShowTop();

            if (_displayStack.Count == 0) _isEmptyProp.Value = true;
        }

        /// <inheritdoc />
        public void CloseAll()
        {
            while (_displayStack.Count > 0)
            {
                var display = _displayStack.Pop();
                display.Hide();
            }

            _isEmptyProp.Value = true;
        }

        /// <summary>
        /// 【目的】スタック上の次の Display を非同期で表示する。
        /// 【理由】上位を閉じた際にその下の UI を再表示し、画面が真っ暗になるのを防ぐ。
        /// </summary>
        /// <param name="ct">【用途】再表示演出を中断する場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】再表示演出の完了を表す UniTask。</returns>
        private async UniTask ShowTopAsync(CancellationToken ct)
        {
            if (!TryGetShowTopDisplay(out var next))
                return;

            await next.ShowAsync(ct);
        }

        /// <summary>
        /// 【目的】スタック上の次の Display を同期的に表示する。
        /// </summary>
        private void ShowTop()
        {
            if (!TryGetShowTopDisplay(out var next))
                return;

            next.Show();
        }

        /// <summary>
        /// 【目的】Display インスタンスを取得してスタックに積む。
        /// 【理由】開く操作のたびに取得とスタック更新を繰り返すため、共通化している。
        /// </summary>
        /// <typeparam name="TDisplay">【用途】取得したい Display (Presenter) 型。</typeparam>
        /// <returns>【戻り値】キャッシュから取得した Display インスタンス。</returns>
        private TDisplay GetDisplay<TDisplay>()
            where TDisplay : class, IDisplay
        {
            TDisplay display = _displayCache.GetDisplay<TDisplay>();
            _displayStack.Push(display);
            
            display.Hide();
            
            return display;
        }

        /// <summary>
        /// 【目的】スタックの最上位を取得しつつ、存在有無を判定する。
        /// 【理由】ShowTop 系メソッドで同じ判定を繰り返すためヘルパーとして切り出す。
        /// </summary>
        /// <param name="display">【用途】最上位 Display を受け取る out 引数。</param>
        /// <returns>【戻り値】Display が存在すれば true。</returns>
        private bool TryGetShowTopDisplay(out IDisplay display)
        {
            if (_displayStack.TryPeek(out var next))
            {
                display = next;
                return true;
            }

            display = null;
            return false;
        }
    }

    /// <summary>
    /// 【目的】DisplayService に渡す初期化コンテキストを定義する。
    /// 【理由】DI からキャッシュを受け渡し、サービス側が依存を取得できるようにするため。
    /// </summary>
    public class DisplayServiceContext
    {
        /// <summary>
        /// 【目的】Display インスタンスを管理するキャッシュを注入する。
        /// 【理由】サービスが生成と再利用の双方をコントロールしやすくするため。
        /// </summary>
        public IDisplayCache Cache;
    }
}
