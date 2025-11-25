// ============================================================================
// モジュール概要: CanvasGroup の alpha を操作するフェードアニメーションを DOTween ベースで提供する。
// 外部依存: Cysharp.Threading.Tasks（UniTask）、DG.Tweening（DOTween）、UnityEngine。
// 使用例: Display 系 Presenter が FadeAnimationComponent を生成し、UI の表示/非表示演出を統一する。
// ============================================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// UI のフェードアニメーションコンポーネント。
    /// - CanvasGroup の alpha を補間して透明度を変更
    /// - DOTween を利用して非同期でアニメーションを実行
    /// </summary>
    public class FadeAnimation : IUIAnimation
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly FadeAnimationParamSO _param;

        /// <summary>
        /// 【目的】演出対象の CanvasGroup とパラメータを束ね、再生時に都度依存を渡さなくて済むようにする。
        /// </summary>
        /// <param name="canvasGroup">【用途】アルファ値を補間する対象。</param>
        /// <param name="param">【用途】目標アルファ値や再生時間を保持する ScriptableObject。</param>
        public FadeAnimation(CanvasGroup canvasGroup, FadeAnimationParamSO param)
        {
            _canvasGroup = canvasGroup;
            _param = param;
        }
        
        /// <summary>
        /// フェードアニメーションを再生。
        /// - 指定の Alpha 値まで補間
        /// - Duration と Ease はパラメータで制御
        /// - CancellationToken により中断可能
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            var initialAlpha = useInitial ? _param.InitialAlpha : _canvasGroup.alpha;
            
            await _canvasGroup
                .DOFade(_param.Alpha, _param.DurationSec)
                .From(initialAlpha)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }
        
        public void PlayImmediate()
        {
            _canvasGroup.alpha = _param.Alpha;
        }
        
        public void Skip()
        {
            _canvasGroup.DOComplete();
        }
    }
}
