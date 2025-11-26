// ============================================================================
// モジュール概要: CanvasGroup を点滅させるアニメーションを DOTween Sequence で構築し、注意喚起演出を簡潔に提供する。
// 外部依存: DG.Tweening、UnityEngine。
// 使用例: FlashAnimationComponent をバナーの強調表示に組み込み、フェード点滅を共通処理として使い回す。
// ============================================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 点滅アニメーションコンポーネント。
    /// - CanvasGroup の alpha を補間してフェードイン/フェードアウトを繰り返す
    /// - DOTween のシーケンスで制御
    /// </summary>
    public class FlashAnimation : SequenceAnimationBase
    {
        private readonly CanvasGroup _canvasGroup;
        private readonly FlashAnimationParamSO _param;

        /// <summary>
        /// 【目的】点滅対象とパラメータを受け取り、即座に再生可能な Sequence を構築する。
        /// 【理由】PlayAsync 呼び出し前にシーケンスを準備し、実行時 GC を避けるため。
        /// </summary>
        public FlashAnimation(CanvasGroup canvasGroup, FlashAnimationParamSO param)
        {
            _canvasGroup = canvasGroup;
            _param = param;
            
            SetSequence();
        }

        public override async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                _canvasGroup.alpha = _param.InitialAlpha;
            }
            
            await base.PlayAsync(ct, useInitial);
        }
        
        public override void PlayImmediate()
        {
            _canvasGroup.alpha = _param.MaxAlpha;
        }

        public override void Skip()
        {
            Sequence.Pause();
            PlayImmediate();
        }

        /// <summary>
        /// 【目的】DOTween の Sequence を構築し、フェードアウト→フェードインを繰り返す動きを定義する。
        /// </summary>
        private void SetSequence()
        {
            Sequence.Append(_canvasGroup
                .DOFade(_param.MinAlpha, _param.DurationSec)
                .SetEase(_param.Ease));
            
            Sequence.Append(_canvasGroup
                .DOFade(_param.MaxAlpha, _param.DurationSec)
                .SetEase(_param.Ease));
            
            // FlashCount はフェードアウト→フェードインを 1 契機としてカウントするため、DOTween のループ回数とは 2 倍の関係になる。
            Sequence.SetLoops(_param.FlashCount == -1 ? -1 : _param.FlashCount * 2, LoopType.Restart);
        }
    }
}
