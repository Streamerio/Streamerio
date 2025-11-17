using System.Threading;
using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Common.UI.Display.Window.Book
{
    /// <summary>
    /// 本型ウィンドウのページめくり演出を制御するコンポーネント。
    /// - Animator を用いてページめくりアニメーションを再生
    /// - ページめくり中は RectTransform の位置やサイズを調整して演出を強化
    /// - 非同期メソッドでアニメーションの終了を待機可能
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class BookAnimationComponent: MonoBehaviour, IBookAnimation
    {
        [SerializeField, ReadOnly]
        private Animator _animator;
        [SerializeField]
        private RectTransform _bookWindowRectTransform;
        
        [Header("アニメーションのステート名")]
        [SerializeField, LabelText("アイドルステート名")]
        public string _idleStateName = "Idle";

        [Header("アニメーションのトリガー名")]
        [SerializeField, LabelText("左ページをめくるトリガー名")]
        public string _turnPageLeftTrigger = "TurnPageLeft";
        [SerializeField, LabelText("右ページをめくるトリガー名")]
        public string _turnPageRightTrigger = "TurnPageRight";

        [Header("本をめくる時の位置調整")]
        [SerializeField, LabelText("通常の本の位置"), ReadOnly]
        public float _defaultBookPosY;
        [SerializeField, LabelText("めくっている途中の本の位置")]
        public float _turningBookPosY;

        [Header("本をめくる時のサイズ調整")]
        [SerializeField, LabelText("通常の本の高さ"), ReadOnly]
        public float _defaultBookHeight;
        [SerializeField, LabelText("めくっている途中の本の高さ")]
        public float _turningBookHeight;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();

            if (_bookWindowRectTransform != null)
            {
                _defaultBookPosY = _bookWindowRectTransform.anchoredPosition.y;
                _defaultBookHeight = _bookWindowRectTransform.rect.height;
            }
        }
#endif
        
        /// <summary>
        /// 左ページをめくるアニメーションを再生（完了まで待機）。
        /// </summary>
        public async UniTask PlayTurnLeftAsync(CancellationToken ct)
        {
            await PlayTurnAsync(_turnPageLeftTrigger, ct);
        }

        /// <summary>
        /// 右ページをめくるアニメーションを再生（完了まで待機）。
        /// </summary>
        public async UniTask PlayTurnRightAsync(CancellationToken ct)
        {
            await PlayTurnAsync(_turnPageRightTrigger, ct);
        }

        /// <summary>
        /// 共通：ページめくりアニメーションを再生。
        /// - 指定トリガーを発火
        /// - Idle ステートを抜けるまで待機
        /// </summary>
        private async UniTask PlayTurnAsync(string triggerName, CancellationToken ct)
        {
            _animator.SetTrigger(triggerName);

            // Idle 以外のアニメーションが再生されるまで待機
            await UniTask.WaitWhile(
                () => _animator.GetCurrentAnimatorStateInfo(0).IsName(_idleStateName),
                cancellationToken: ct);
        }

        /// <summary>
        /// ページめくり中の見た目に変更。
        /// - RectTransform の位置と高さを調整
        /// </summary>
        public void SetTurningBookTransform()
        {
            SetBookTransform(_turningBookPosY, _turningBookHeight);
        }

        /// <summary>
        /// 本の見た目をデフォルト状態に戻す。
        /// </summary>
        public void ResetBookTransform()
        {
            SetBookTransform(_defaultBookPosY, _defaultBookHeight);
        }

        /// <summary>
        /// RectTransform の位置と高さをまとめて変更。
        /// </summary>
        private void SetBookTransform(float posY, float height)
        {
            _bookWindowRectTransform.anchoredPosition =
                new Vector2(_bookWindowRectTransform.anchoredPosition.x, posY);

            _bookWindowRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
    
    
    public interface IBookAnimation
    {
        UniTask PlayTurnLeftAsync(CancellationToken ct);
        UniTask PlayTurnRightAsync(CancellationToken ct);
        void SetTurningBookTransform();
        void ResetBookTransform();
    }
}