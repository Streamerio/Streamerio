// ============================================================================
// モジュール概要: RectTransform のアンカー座標を DOTween で補間し、UI のスライド演出を統一提供する。
// 外部依存: Cysharp.Threading.Tasks（UniTask）、DG.Tweening（DOTween）、UnityEngine。
// 使用例: Overlay の進入演出で MoveAnimationComponent を利用し、任意方向へのスライドインを簡潔に実装する。
// ============================================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// RectTransform を指定した座標まで移動させるアニメーションコンポーネント。
    /// - DOTween の DOAnchorPos を利用
    /// - 非同期で再生でき、CancellationToken による中断も可能
    /// </summary>
    public class MoveAnimation : IUIAnimation
    {
        private readonly RectTransform _rectTransform;
        private readonly MoveAnimationParamSO _param;
        
        /// <summary>
        /// 【目的】移動対象とパラメータを関連付け、再生時に毎回依存を探し直さないようにする。
        /// </summary>
        /// <param name="rectTransform">【用途】DOAnchorPos で補間する RectTransform。</param>
        /// <param name="param">【用途】目標座標や補間設定を保持する ScriptableObject。</param>
        public MoveAnimation(RectTransform rectTransform, MoveAnimationParamSO param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }
        
        /// <summary>
        /// 移動アニメーションを再生する。
        /// - 目標座標へ補間移動
        /// - Duration と Ease はパラメータで制御
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            var initialPos = useInitial ? _param.InitialAnchoredPosition : _rectTransform.anchoredPosition;
            
            await _rectTransform
                .DOAnchorPos(_param.AnchoredPosition, _param.DurationSec)
                .From(initialPos)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
        
        public void PlayImmediate()
        {
            _rectTransform.anchoredPosition = _param.AnchoredPosition;
        }
        
        public void Skip()
        {
            _rectTransform.DOComplete();
        }
    }
}
