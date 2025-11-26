// モジュール概要:
// 共通ダイアログ表示用の View 実装を提供し、背景フェードと移動アニメーションを統一挙動で実行する。
// 依存関係: 背景制御 (Common.UI.Display.Background)、経路アニメーション (Common.UI.Animation)、閉じるボタン (Common.UI.Part.Button)。
// 使用例: Presenter が ICommonDialogView 経由で ShowAsync/HideAsync を呼び、演出完了後にダイアログ固有処理を実行する。

using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display;
using Common.UI.Display.Background;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Common.UI.Dialog
{
    /// <summary>
    /// 【目的】共通ダイアログ View が備えるべき公開契約を定義する。
    /// 【理由】Presenter 層が背景演出や閉じるボタンを一貫した手順で扱えるようにするため。
    /// </summary>
    public interface IDialogView : IDisplayView
    {
        /// <summary>
        /// 【目的】背景のフェード制御とクリック検知を提供する Presenter を公開する。
        /// 【補足】Presenter が ShowAsync/HideAsync を順序制御する際に利用する。
        /// </summary>
        IDisplayBackground Background { get; }

        /// <summary>
        /// 【目的】ユーザーの閉じる操作を受け取る共通ボタン Presenter を公開する。
        /// 【補足】Presenter が OnClick を購読して閉じる処理へ接続する前提。
        /// </summary>
        ICommonButton CloseButton { get; }
    }

    /// <summary>
    /// 【目的】共通ダイアログの表示ロジックをカプセル化し、背景フェードと移動アニメーションを同期させる。
    /// 【理由】各 Presenter が個別に演出を実装すると仕様がぶれやすいため、演出仕様を本クラスで統制する。
    /// </summary>
    public abstract class DialogViewBase : DisplayViewBase, IDialogView
    {
        /// <summary>
        /// 【目的】背景 Presenter の参照を保持し、表示/非表示時に即座に呼び出せるようにする。
        /// 【理由】依存注入を通じて受け取った後もメソッド内で繰り返し解決せずに済ませるため。
        /// </summary>
        private IDisplayBackground _background;
        /// <summary>
        /// 【目的】閉じるボタン Presenter の参照を保持する。
        /// 【理由】Construct 以降でイベント購読を行う際に、解決済みインスタンスへ直接アクセスするため。
        /// </summary>
        private ICommonButton _closeButton;

        /// <summary>
        /// 【目的】背景 Presenter を公開し、Presenter 層がフェード制御やクリック検知を扱えるようにする。
        /// 【補足】Construct 時に注入された参照を返却するだけで追加処理は行わない。
        /// </summary>
        public IDisplayBackground Background => _background;

        /// <summary>
        /// 【目的】閉じるボタン Presenter を公開し、Presenter 層がイベント購読できるようにする。
        /// 【補足】Construct 時に注入された参照を返却するだけで追加処理は行わない。
        /// </summary>
        public ICommonButton CloseButton => _closeButton;

        /// <summary>
        /// 【目的】表示アニメーションの実行コンポーネントをキャッシュする。
        /// 【理由】毎回インスタンス生成せずに済ませ、GC 発生や初回遅延を防ぐため。
        /// </summary>
        private IUIAnimation _showAnimation;
        /// <summary>
        /// 【目的】非表示アニメーションの実行コンポーネントをキャッシュする。
        /// 【理由】Hide のたびに新規生成すると GC が発生し、演出がカクつくため。
        /// </summary>
        private IUIAnimation _hideAnimation;
        /// <summary>
        /// 【目的】背景 Presenter と閉じるボタンを注入し、表示/非表示アニメーションのコンポーネントを初期化する。
        /// 【理由】PathAnimationComponent を事前構築しておくことで、表示タイミングで追加割り当てを発生させずに再生したいため。
        /// </summary>
        /// <param name="background">フェードおよび入力ブロックを担当する背景 Presenter。</param>
        /// <param name="closeButton">ユーザーへ閉じる操作を提供する共通ボタン Presenter。</param>
        [Inject]
        public void Construct(IDisplayBackground background,
            [Key(ButtonType.Close)]ICommonButton closeButton,
            [Key(AnimationType.Show)] IUIAnimation showAnimation,
            [Key(AnimationType.Hide)] IUIAnimation hideAnimation)
        {
            _background = background;
            _closeButton = closeButton;
            
            _showAnimation = showAnimation;
            _hideAnimation = hideAnimation;
        }
        
        /// <summary>
        /// 【目的】ダイアログを非同期アニメーションで表示する。
        /// 【処理概要】経路の始点へ配置 → 背景フェードイン → 移動アニメーション再生。
        /// 【理由】背景を先に暗転させることで、ダイアログが唐突に現れる印象を和らげる。
        /// </summary>
        /// <param name="ct">【用途】画面遷移などで表示処理を中断したい場合に利用するキャンセルトークン。</param>
        /// <returns>【戻り値】アニメーション完了まで待機する UniTask。</returns>
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
            await _background.ShowAsync(ct);
            await _showAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// 【目的】アニメーションを介さずにダイアログを即時表示する。
        /// 【処理概要】経路の終点へ瞬時に配置し、背景も即時表示する。
        /// 【理由】演出をスキップしたいデバッグ時や、再表示時のちらつきを防ぐため。
        /// </summary>
        public override void Show()
        {
            CanvasGroup.alpha = UIUtil.DEFAULT_SHOW_ALPHA;
            _showAnimation.PlayImmediate();
            _background.Show();
        }

        /// <summary>
        /// 【目的】ダイアログを非同期アニメーションで閉じる。
        /// 【処理概要】非表示経路の始点へ配置 → 背景フェードアウト → 移動アニメーション再生。
        /// 【理由】背景フェードを先に終えることで、背後に光が残る残像を防ぐ。
        /// </summary>
        /// <param name="ct">【用途】シーン遷移などで閉じ処理を中断したい場合に利用するキャンセルトークン。</param>
        /// <returns>【戻り値】アニメーション完了を表す UniTask。</returns>
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _background.HideAsync(ct);
            await _hideAnimation.PlayAsync(ct);
        }

        /// <summary>
        /// 【目的】アニメーションをスキップして即時非表示にする。
        /// 【処理概要】背景を即時非表示にした後、RectTransform を経路終点へ配置する。
        /// 【理由】シーン破棄やクイック遷移など時間がない局面でも描画破綻を避けるため。
        /// </summary>
        public override void Hide()
        {
            _background.Hide();
            _hideAnimation.PlayImmediate();
        }
    }
}
