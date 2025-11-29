// ============================================================================
// モジュール概要: 複数パーツのフェード演出に必要な遅延時間を ScriptableObject 化し、データ駆動で調整できるようにする。
// 外部依存: Alchemy.Inspector（LabelText）、UnityEngine。
// 使用例: FadePartsAnimationComponentParamSO アセットを作成し、UI チャンクの順次点灯演出で ShowDelaySec を調整する。
// ============================================================================

using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// FadePartsAnimationComponent の設定用パラメータ。
    /// - Alpha / Duration / Ease は FadeAnimationComponentParam から継承
    /// - ShowDelaySec を追加してパーツごとの遅延を指定可能
    /// </summary>
    [CreateAssetMenu(fileName = "FadePartsAnimationSO", menuName = "SO/UI/Animation/FadeParts")]
    public class FadePartsAnimationParamSO : FadeAnimationParamSO
    {
        /// <summary>
        /// 【目的】パーツ間の演出間隔を秒指定で調整し、演出テンポをチューニングできるようにする。
        /// </summary>
        [SerializeField, LabelText("パーツごとの表示ディレイ(秒)"), Min(0.001f)]
        [Tooltip("各 CanvasGroup のフェード後にどれくらい待って次のパーツを再生するか。")]
        public float ShowDelaySec = 0.05f;
    }
}
