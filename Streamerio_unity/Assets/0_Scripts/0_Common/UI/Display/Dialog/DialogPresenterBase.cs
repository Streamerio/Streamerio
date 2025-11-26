// モジュール概要:
// ダイアログ Presenter 向けの共通基盤を提供し、閉じる操作と DI 文脈の受け入れを統一する。
// 依存関係: Common.UI.Display の Presenter 基底実装、ICommonDialogView、IDialogService、R3 のリアクティブ購読。
// 使用例: 具体的なダイアログ Presenter が DialogBase<TContext> を継承し、Bind と CloseEvent の共通処理を利用する。

using Common.UI.Display;
using Cysharp.Threading.Tasks;
using R3;

namespace Common.UI.Dialog
{
    /// <summary>
    /// 【目的】ダイアログ Presenter が備えるべき共通契約を定義する。
    /// 【理由】Display 系の共通 Show/Hide API を共有しつつ、ダイアログ固有の拡張余地を残すため。
    /// </summary>
    public interface IDialog : IDisplay { }

    /// <summary>
    /// 【目的】共通ダイアログの Presenter 基底クラスを提供し、閉じる操作の購読と文脈接続を一元化する。
    /// 【理由】Close ボタンや背景クリックごとに購読処理を繰り返し記述するとバグを生むため、共通化して再利用性を高める。
    /// </summary>
    /// <typeparam name="TContext">Presenter に注入されるダイアログコンテキスト型。</typeparam>
    public abstract class DialogPresenterBase<TView, TContext> : DisplayPresenterBase<TView, TContext>, IDialog
        where TView : IDialogView
        where TContext : DialogContext<TView>
    {
        /// <summary>
        /// 【目的】ダイアログの開閉を制御するサービス参照を保持する。
        /// 【理由】CloseEvent など複数メソッドから同一サービスへアクセスし、スタック操作を一元化するため。
        /// </summary>
        protected IDialogService Service;

        /// <summary>
        /// 【目的】Presenter に必要な View/Service を文脈から取り込み、基底クラスが利用できる状態にする。
        /// 【理由】具体的な Presenter で個別に View 取得ロジックを書かずに済むようにするため。
        /// </summary>
        /// <param name="context">View と Service を内包したダイアログコンテキスト。</param>
        protected override void AttachContext(TContext context)
        {
            View = context.View;
            Service = context.Service;
        }

        /// <summary>
        /// 【目的】閉じるボタンと背景クリックの両方から CloseEvent を呼び出す購読をセットアップする。
        /// 【処理概要】基底 Bind を呼んだ後、各 Observable を CloseEvent へ接続し、ライフタイムに合わせて破棄する。
        /// 【理由】ダイアログ表示中にユーザーが直感的に閉じ操作できる導線を共通化するため。
        /// </summary>
        protected override void Bind()
        {
            base.Bind();

            View.CloseButton.OnClickAsObservable
                .Subscribe(_ => CloseEvent())
                .RegisterTo(GetCt());

            View.Background.OnClickAsObservable
                .Subscribe(_ => CloseEvent())
                .RegisterTo(GetCt());
        }

        /// <summary>
        /// 【目的】ダイアログを閉じる共通処理を提供する。
        /// 【処理概要】DialogService にトップダイアログのクローズを指示し、非同期完了は待たずに Forget する。
        /// 【理由】派生クラスで追加処理が必要な場合にオーバーライドできるよう、最小限の標準挙動を定義する。
        /// </summary>
        protected virtual void CloseEvent()
        {
            Service.CloseTopAsync(GetCt()).Forget();
        }
    }

    /// <summary>
    /// 【目的】DialogBase が必要とする依存をひとまとめにした文脈クラス。
    /// 【拡張性】新たに依存が増えた場合はプロパティを追加することで対応する。
    /// </summary>
    public class DialogContext<TView>
        where TView : IDialogView
    {
        /// <summary>
        /// 【目的】Presenter が操作する共通 View を格納する。
        /// 【理由】ダイアログ生成時に依存をまとめて渡し、各 Presenter が同じ View 契約へアクセスできるようにするため。
        /// </summary>
        public TView View;

        /// <summary>
        /// 【目的】スタック管理を担うダイアログサービスを格納する。
        /// 【理由】Presenter 側で Open/Close を調整する際に、文脈から取得したサービスへ即座にアクセスするため。
        /// </summary>
        public IDialogService Service;
    }
}
