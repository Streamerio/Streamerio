// ============================================================================
// モジュール概要: フェード演出の ScriptableObject パラメータを提供し、アニメーション設定をデータ駆動化する。
// 外部依存: UnityEngine（ScriptableObject）。
// 使用例: FadeAnimationComponentParamSO をアセット化し、Scene ごとに目標アルファ値を差し替える。
// ============================================================================

using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// フェードアニメーションの設定パラメータを ScriptableObject 化したクラス。
    /// - Alpha: 目標の透明度
    /// - DurationSec: アニメーション時間
    /// - Ease: イージング
    /// <para>
    /// 【理由】アセットとして管理することで、ノーコードで演出を調整できるようにするため。
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "FadeAnimationSO", menuName = "SO/UI/Animation/Fade")]
    public class FadeAnimationParamSO : UIAnimationComponentParamSO
    {
        /// <summary>
        /// 【目的】アニメーション完了時の目標透明度を設定し、演出の強度をデザイナーが調整できるようにする。
        /// </summary>
        [Header("透明度")]
        [SerializeField, Range(0f, 1f)]
        public float InitialAlpha;
        [SerializeField, Range(0f, 1f)]
        [Tooltip("フェード後に到達したい CanvasGroup.alpha の値。")]
        public float Alpha;
    }
}
