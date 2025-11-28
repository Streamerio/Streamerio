// モジュール概要:
// UI 背景のフェード演出を担当する View 実装。Presenter から呼び出され、見た目の制御を集約する。
// 依存関係: FadeAnimationComponentParamSO で演出パラメータを受け取り、CanvasGroup を直接操作する。

using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Background
{
    /// <summary>
    /// 【目的】背景 View の専用インターフェースを表すマーカー。
    /// </summary>
    public interface IDisplayBackgroundView : IDisplayView, IInitializable { }
    
    /// <summary>
    /// 【目的】背景のフェードイン/アウト演出を実装する View。
    /// 【理由】Presenter 側が演出詳細を意識せず、見た目制御のみを委譲できるようにするため。
    /// </summary>
    public class DisplayBackgroundView : DisplayViewBase, IDisplayBackgroundView
    {
        /// <summary>
        /// 【目的】表示演出を再生するアニメーションコンポーネントをキャッシュする。
        /// 【理由】毎回インスタンス生成すると GC が発生し、演出開始が遅れるため。
        /// </summary>
        private IUIAnimation _showAnimation;
        /// <summary>
        /// 【目的】非表示演出を再生するアニメーションコンポーネントをキャッシュする。
        /// 【理由】演出切り替えが高速に行えるよう、初期化時に生成しておく。
        /// </summary>
        private IUIAnimation _hideAnimation;

        [Inject]
        public void Construct(
            [Key(AnimationType.Show)] IUIAnimation showAnimation,
            [Key(AnimationType.Hide)] IUIAnimation hideAnimation)
        {
            _showAnimation = showAnimation;
            _hideAnimation = hideAnimation;
        }

        /// <inheritdoc />
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
        }

        /// <inheritdoc />
        public override void Show()
        {
            _showAnimation.PlayImmediate();
        }

        /// <inheritdoc />
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
        }

        /// <inheritdoc />
        public override void Hide()
        {
            _hideAnimation.PlayImmediate();
        }
    }
}
