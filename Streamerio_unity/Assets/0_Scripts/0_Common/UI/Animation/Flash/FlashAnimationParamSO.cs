// ============================================================================
// モジュール概要: 点滅演出の透明度レンジや回数を ScriptableObject として外部化し、シーンごとの演出差分を容易にする。
// 外部依存: Alchemy.Inspector（LabelText）、UnityEngine。
// 使用例: FlashAnimationComponentParamSO をアセット化して警告ポップアップの点滅回数をデザイナーが調整する。
// ============================================================================

using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 点滅アニメーションの設定パラメータ。
    /// - MinAlpha: 最小の透明度
    /// - MaxAlpha: 最大の透明度
    /// - FlashCount: 点滅回数（-1 なら無限）
    /// </summary>
    [CreateAssetMenu(fileName = "FlashAnimationSO", menuName = "SO/UI/Animation/Flash")]
    public class FlashAnimationParamSO : UIAnimationComponentParamSO
    {
        /// <summary>
        /// 【目的】点滅時の最小透明度を指定し、どの程度まで暗くなるかを制御する。
        /// </summary>
        [Header("透明度設定")]
        [Range(0f,1f)]
        public float InitialAlpha = 0f;
        [SerializeField, LabelText("最小の透明度"), Range(0f, 1f)]
        [Tooltip("点滅で最も暗くなるときの CanvasGroup.alpha。0 で完全に消える。")]
        public float MinAlpha = 0f;
        
        /// <summary>
        /// 【目的】点滅時の最大透明度を指定し、どの程度まで明るく戻すかを管理する。
        /// </summary>
        [SerializeField, LabelText("最大の透明度"), Range(0f, 1f)]
        [Tooltip("点滅で最も明るくなるときの CanvasGroup.alpha。1 で完全表示。")]
        public float MaxAlpha = 1f;
        
        /// <summary>
        /// 【目的】点滅回数を制御し、演出時間や視認性を調整する。
        /// </summary>
        [Header("点滅回数 (消えて戻るまでで1回) (-1で無限)")]
        [SerializeField, Min(-1)]
        [Tooltip("点滅の繰り返し回数。-1 を設定すると無限ループで点滅する。")]
        public int FlashCount;
    }
}
