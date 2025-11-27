// モジュール概要:
// テキスト付きボタンの演出を管理し、装飾表示とスケールアニメーションを統合する View 実装。
// 依存関係: ScaleAnimationComponent で押下/解放を制御し、Image を直接操作して装飾を切り替える。
// 使用例: ButtonLifetimeScope から IButtonView として注入され、CommonButtonPresenter が Pointer イベントに応じて操作する。

using System.Threading;
using Alchemy.Inspector;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Part.Button
{
    /// <summary>
    /// 【目的】テキストボタンの装飾表示とスケール演出を抽象化し、Presenter から簡潔に制御できるようにする。
    /// 【理由】羽・下線など複数の見た目要素を View 内へ閉じ込め、再利用時の演出差異を減らすため。
    /// </summary>
    public class TextButtonView : UIBehaviourBase, IButtonView
    {
        [Header("装飾画像")]
        /// <summary>
        /// 【目的】ホバー時に点灯させる羽の装飾を参照する。
        /// 【理由】テキストボタンの個性となる装飾表示を View 内で切り替えるため。
        /// </summary>
        [SerializeField, LabelText("羽")]
        [Tooltip("ホバー時に表示する羽アイコン。")]
        private Image _featherImage;

        /// <summary>
        /// 【目的】ホバー時に表示する下線装飾を参照する。
        /// 【理由】カーソルオーバー時の視覚的な強調を提供するため。
        /// </summary>
        [SerializeField, LabelText("下線")]
        [Tooltip("ホバー時に表示する下線装飾。")]
        private Image _lineImage;
        
        /// <summary>
        /// 【目的】押下時に再生するスケールアニメーションをキャッシュする。
        /// 【理由】連打時にコンポーネント生成が発生しないよう初期化時に準備するため。
        /// </summary>
        private IUIAnimation _pushDownAnim;
        /// <summary>
        /// 【目的】解放時に再生するスケールアニメーションをキャッシュする。
        /// 【理由】連続操作時にも滑らかにサイズ復元できるようにするため。
        /// </summary>
        private IUIAnimation _pushUpAnim;
        
        [Inject]
        public void Construct(
            [Key(AnimationType.PushDown)] IUIAnimation pushDownAnim,
            [Key(AnimationType.PushUp)] IUIAnimation pushUpAnim)
        {
            _pushDownAnim = pushDownAnim;
            _pushUpAnim = pushUpAnim;
        }
        
        /// <summary>
        /// 【目的】ボタン押下時に縮小アニメーションを再生する。
        /// 【理由】クリックフィードバックを視覚へ伝え、操作体験を高めるため。
        /// </summary>
        /// <param name="ct">【用途】押下演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】縮小演出が完了したことを示す UniTask。</returns>
        public async UniTask PlayPointerDownAsync(CancellationToken ct)
        {
            await _pushDownAnim.PlayAsync(ct);
        }
        
        /// <summary>
        /// 【目的】ボタン解放時に元のスケールへ戻す。
        /// 【理由】押下前の状態へ復帰させ、次の入力に備えるため。
        /// </summary>
        /// <param name="ct">【用途】復元演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】復元演出が完了したことを示す UniTask。</returns>
        public async UniTask PlayPointerUpAsync(CancellationToken ct)
        {
            await _pushUpAnim.PlayAsync(ct);
        }
        
        /// <summary>
        /// 【目的】ホバー開始時に装飾を表示する。
        /// 【理由】カーソルが乗っている状態を視覚的に示し、操作対象を強調するため。
        /// </summary>
        /// <param name="ct">【用途】（未使用）ホバー演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】装飾切り替え完了を示す UniTask。実処理は同期だがインターフェース整合性のため非同期。</returns>
        public async UniTask PlayPointerEnterAsync(CancellationToken ct)
        {
            _featherImage.enabled = true;
            _lineImage.enabled = true;
            await UniTask.CompletedTask;
        }
        
        /// <summary>
        /// 【目的】ホバー終了時に装飾を消し、スケールをリセットする。
        /// 【理由】ホバーが外れた後の視覚状態を整え、押下後のサイズ残りを解消するため。
        /// </summary>
        /// <param name="ct">【用途】解除演出を中断したい場合に使用する CancellationToken。</param>
        /// <returns>【戻り値】解除演出が完了したことを示す UniTask。</returns>
        public async UniTask PlayPointerExitAsync(CancellationToken ct)
        {
            _featherImage.enabled = false;
            _lineImage.enabled = false;
            await _pushUpAnim.PlayAsync(ct, false);
        }
        
        public async UniTask PlayPointerClickAsync(CancellationToken ct)
        {
            ResetButtonState();
            await UniTask.CompletedTask;
        }
        
        /// <summary>
        /// 【目的】ボタンを初期状態へ戻し、装飾を非表示にする。
        /// 【理由】SetActive(false) → SetActive(true) などの再利用時に、演出残りがない状態から開始するため。
        /// </summary>
        public void ResetButtonState()
        {
            _pushUpAnim.PlayImmediate();
            _featherImage.enabled = false;
            _lineImage.enabled = false;
        }
    }
}
