// ============================================================================
// モジュール概要: スケールアニメーションのパラメータを ScriptableObject として管理し、UI 演出の調整を容易にする。
// 外部依存: UnityEngine（ScriptableObject）。
// 使用例: ボタンのホバー演出で ScaleAnimationComponentParamSO を差し替え、拡大率をデザイナーが調整する。
// ============================================================================

using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// スケールアニメーションの設定パラメータ。
    /// - Scale: 最終的に到達する拡縮率 (1 = 元のサイズ)
    /// - DurationSec: 補間時間
    /// - Ease: 補間方法
    /// </summary>
    [CreateAssetMenu(fileName = "ScaleAnimationSO", menuName = "SO/UI/Animation/Scale")]
    public class ScaleAnimationParamSO : UIAnimationComponentParamSO
    {
        /// <summary>
        /// 【目的】アニメーション終了時の拡大率を指定し、クリック演出などで大きさ変化を制御できるようにする。
        /// </summary>
        [Header("最終的な大きさ (1 = 等倍)")]
        public float InitialScale = 1f;
        [SerializeField, Min(0)]
        [Tooltip("アニメーション完了時の RectTransform.localScale 値。1 で等倍、0.5 で半分、2 で 2 倍。")]
        public float Scale = 1f;
    }
}
