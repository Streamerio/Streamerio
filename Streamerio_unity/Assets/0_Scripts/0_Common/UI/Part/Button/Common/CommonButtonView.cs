// モジュール概要:
// 共通ボタンの見た目制御を担当し、押下・解放・ホバー各イベントに応じたアニメーションを再生する View 実装。
// 依存関係: ScaleAnimationComponent/FadeAnimationComponent を利用し、RectTransform と CanvasGroup を操作する。
// 使用例: ButtonLifetimeScope から IButtonView として注入され、CommonButtonPresenter が PlayPointerXXAsync を呼び出す。

using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 【目的】共通ボタンのビジュアル挙動を抽象化し、Presenter からアニメーション操作を受け付ける。
    /// 【理由】各ボタンで同様の演出を共有するため、View 側でアニメーション組み立てを一元化する。
    /// </summary>
    public class CommonButtonView : UIBehaviourBase, IButtonView
    {
        /// <summary>
        /// 【目的】押下時に再生するスケールアニメーションをキャッシュする。
        /// 【理由】毎回生成すると GC が発生し、連打時にパフォーマンスが落ちるため。
        /// </summary>
        private IUIAnimation _pushDownAnim;
        /// <summary>
        /// 【目的】離した瞬間に再生するスケールアニメーションをキャッシュする。
        /// 【理由】押下解除のたびにコンポーネントを作り直さず、スムーズに演出したいため。
        /// </summary>
        private IUIAnimation _pushUpAnim;
        /// <summary>
        /// 【目的】ホバー時に再生するフェードアニメーションをキャッシュする。
        /// 【理由】カーソル移動に即応できるようにし、演出ラグを抑えるため。
        /// </summary>
        private IUIAnimation _enterAnim;
        /// <summary>
        /// 【目的】ホバー解除時に再生するフェードアニメーションをキャッシュする。
        /// 【理由】ホバー状態からの戻りが滑らかになるよう初期化時に準備しておく。
        /// </summary>
        private IUIAnimation _exitAnim;
        
        [Inject]
        public void Construct([Key(AnimationType.PushDown)] IUIAnimation pushDownAnim,
            [Key(AnimationType.PushUp)] IUIAnimation pushUpAnim,
            [Key(AnimationType.Enter)] IUIAnimation enterAnim,
            [Key(AnimationType.Exit)] IUIAnimation exitAnim)
        {
            _pushDownAnim = pushDownAnim;
            _pushUpAnim = pushUpAnim;
            _enterAnim = enterAnim;
            _exitAnim = exitAnim;
        }

        /// <summary>
        /// 【目的】ボタン押下時の縮小アニメーションを再生する。
        /// 【理由】押下フィードバックを即座に視覚へ反映し、操作感を向上させるため。
        /// </summary>
        /// <param name="ct">【用途】押下演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】縮小アニメーションが完了したことを示す UniTask。</returns>
        public virtual async UniTask PlayPointerDownAsync(CancellationToken ct)
        {
            // 倒し込み演出中にボタンが破棄された場合でも確実に停止させるため、Destroy トークンを利用する。
            await _pushDownAnim.PlayAsync(destroyCancellationToken);            
        }

        /// <summary>
        /// 【目的】ボタン解放時にスケールを復元するアニメーションを再生する。
        /// 【理由】押下前のサイズへ戻すことで、連打時も視覚状態をリセットできるようにするため。
        /// </summary>
        /// <param name="ct">【用途】解放演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】復元アニメーションが完了したことを示す UniTask。</returns>
        public async UniTask PlayPointerUpAsync(CancellationToken ct)
        {
            await _pushUpAnim.PlayAsync(ct);
        }

        /// <summary>
        /// 【目的】カーソルがボタン上へ入った際にフェード演出を再生する。
        /// 【理由】ホバー状態を視覚的に示し、ユーザーへ操作可能領域を伝えるため。
        /// </summary>
        /// <param name="ct">【用途】ホバー演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】ホバー演出が完了したことを示す UniTask。</returns>
        public async UniTask PlayPointerEnterAsync(CancellationToken ct)
        {
            await _enterAnim.PlayAsync(ct);
        }

        /// <summary>
        /// 【目的】カーソルがボタンから離れた際にフェードとスケールを元へ戻す。
        /// 【理由】ホバー解除後に視覚状態をリセットし、押下後のスケール残りを防ぐため。
        /// </summary>
        /// <param name="ct">【用途】解除演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】解除演出とスケール復元が完了したことを示す UniTask。</returns>
        public async UniTask PlayPointerExitAsync(CancellationToken ct)
        {
            await UniTask.WhenAll(
                _exitAnim.PlayAsync(ct),
                _pushUpAnim.PlayAsync(ct, false)
            );
        }
        
        public virtual async UniTask PlayPointerClickAsync(CancellationToken ct)
        {
            ResetButtonState();
            await UniTask.CompletedTask;
        }
        
        /// <summary>
        /// 【目的】ボタンの透明度とスケールを既定状態へ戻す。
        /// 【理由】同一ボタンを非表示→再表示した際に演出残りが無い状態から始めるため。
        /// </summary>
        public virtual void ResetButtonState()
        {
            _exitAnim.PlayImmediate();
            _pushUpAnim.PlayImmediate();
        }
    }
}
