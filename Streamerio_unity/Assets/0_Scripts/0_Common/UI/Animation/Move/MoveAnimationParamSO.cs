// ============================================================================
// モジュール概要: UI のスライド演出で使用する座標パラメータを ScriptableObject として管理する。
// 外部依存: UnityEngine（ScriptableObject）。
// 使用例: MoveAnimationComponentParamSO アセットを画面サイズごとに用意し、スライドの終点座標を調整する。
// ============================================================================

using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// 移動アニメーションの設定パラメータ。
    /// - Position: 目標座標（AnchoredPosition）
    /// - DurationSec: アニメーション時間（基底から継承）
    /// - Ease: 補間方法（基底から継承）
    /// </summary>
    [CreateAssetMenu(fileName = "MoveAnimationSO", menuName = "SO/UI/Animation/Move")]
    public class MoveAnimationParamSO : UIAnimationComponentParamSO
    {
        /// <summary>
        /// 【目的】RectTransform.anchoredPosition の目標値を指定し、演出後の位置をデータ側で制御する。
        /// </summary>
        public Vector2 InitialAnchoredPosition;
        [Header("移動先座標")]
        [SerializeField]
        [Tooltip("アニメーション完了時に到達してほしい AnchoredPosition。")]
        public Vector2 AnchoredPosition;
    }
}
