// ============================================================================
// モジュール概要: RectTransform を任意の経路に沿って移動させるアニメーションを DOTween で提供する。
// 外部依存: Cysharp.Threading.Tasks（UniTask）、DG.Tweening、UnityEngine。
// 使用例: チュートリアル矢印を PathAnimationComponent で移動させ、視線誘導を演出する。
// ============================================================================

using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 複数の座標を通るパス移動アニメーションコンポーネント。
    /// - DOTween の DOLocalPath を利用
    /// - 指定した Path の座標を順に辿る
    /// - PathType によって直線 / 曲線補間を切り替え可能
    /// - 非同期で再生し、CancellationToken で中断も可能
    /// </summary>
    public class PathAnimation : IUIAnimation
    {
        private readonly RectTransform _rectTransform;
        private readonly PathAnimationParamSO _param;

        /// <summary>
        /// 【目的】対象 RectTransform とパス設定を束ね、再生ロジックから分離する。
        /// </summary>
        /// <param name="rectTransform">【用途】経路移動させたい UI 要素。</param>
        /// <param name="param">【用途】通過点や補間方法を保持する ScriptableObject。</param>
        public PathAnimation(RectTransform rectTransform, PathAnimationParamSO param)
        {
            _rectTransform = rectTransform;
            _param = param;
        }

        /// <summary>
        /// パスに沿って移動アニメーションを再生する。
        /// - Path: 通過する座標の配列
        /// - Duration / Ease / PathType はパラメータで制御
        /// </summary>
        public async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                _rectTransform.localPosition = _param.InitialPosition;
            }
            
            await _rectTransform
                .DOLocalPath(_param.Path, _param.DurationSec, _param.PathType)
                .SetEase(_param.Ease)
                .ToUniTask(cancellationToken: ct);
        }  
        
        public void PlayImmediate()
        {
            if (_param.Path.Length > 0)
            {
                _rectTransform.localPosition = _param.Path[^1];
            }
        }
        
        public void Skip()
        {
            _rectTransform.DOComplete();
        }
    }
}
