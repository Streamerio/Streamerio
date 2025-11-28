// ============================================================================
// モジュール概要: 複数 CanvasGroup を連続フェードさせる DOTween Sequence コンポーネントを提供する。
// 外部依存: DG.Tweening（DOTween）、UnityEngine。
// 使用例: チュートリアル UI の段階的表示などで FadePartsAnimationComponent を利用し、パーツ単位の演出を統一する。
// ============================================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 複数の UI パーツ(CanvasGroup)をまとめてフェードさせるアニメーションコンポーネント。
    /// - DOTween の Sequence を使って順番にアニメーションを構築
    /// - 各パーツ間にディレイを挿入可能
    /// </summary>
    public class FadePartsAnimation : SequenceAnimationBase
    {
        private readonly CanvasGroup[] _canvasGroups;
        private readonly FadePartsAnimationParamSO _param;
        /// <summary>
        /// 【目的】対象パーツ群とパラメータを受け取り、再利用可能な DOTween Sequence を組み立てる。
        /// 【理由】コンストラクタ内でセットアップしておくことで、PlayAsync 呼び出し時に余計な GC を発生させないため。
        /// </summary>
        public FadePartsAnimation(CanvasGroup[] canvasGroups, FadePartsAnimationParamSO param)
        {
            _canvasGroups = canvasGroups;
            _param = param;
            
            SetSequence();
        }

        public override async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                foreach (var canvasGroup in _canvasGroups)
                {
                    canvasGroup.alpha = _param.InitialAlpha;
                }   
            }
            
            await base.PlayAsync(ct, useInitial);
        }
        
        public override void PlayImmediate()
        {
            foreach (var canvasGroup in _canvasGroups)
            {
                canvasGroup.alpha = _param.Alpha;
            }   
        }

        /// <summary>
        /// 【目的】DOTween の Sequence を構築し、遅延込みで順次フェードする動きを定義する。
        /// - 各 CanvasGroup に対してフェードを追加
        /// - パーツ間にインターバルを挿入して「順番に」アニメーションする
        /// </summary>
        private void SetSequence()
        {
            for(int i = 0; i < _canvasGroups.Length-1; i++)
            {
                Sequence.Append(_canvasGroups[i]
                    .DOFade(_param.Alpha, _param.DurationSec)
                    .SetEase(_param.Ease));
                Sequence.AppendInterval(_param.ShowDelaySec);
            }
            Sequence.Append(_canvasGroups[^1]
                .DOFade(_param.Alpha, _param.DurationSec)
                .SetEase(_param.Ease));
        }
    }
}
