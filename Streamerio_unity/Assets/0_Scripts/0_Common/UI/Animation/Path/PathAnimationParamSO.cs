// ============================================================================
// モジュール概要: 経路移動アニメーションの制御点や補間方法を ScriptableObject で管理する。
// 外部依存: Alchemy.Inspector（LabelText）、DG.Tweening、UnityEngine。
// 使用例: PathAnimationComponentParamSO を複数作成し、チュートリアル矢印の動線をアセット差し替えで調整する。
// ============================================================================

using Alchemy.Inspector;
using DG.Tweening;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// パス移動アニメーションの設定パラメータ。
    /// - Path: オブジェクトが通る座標リスト（ローカル座標系）
    /// - PathType: 補間方法（Linear / CatmullRom など）
    /// - DurationSec / Ease: アニメーション速度やイージング（基底から継承）
    /// </summary>
    [CreateAssetMenu(fileName = "PathAnimationSO", menuName = "SO/UI/Animation/Path")]
    public class PathAnimationParamSO : UIAnimationComponentParamSO
    {
        public Vector3 InitialPosition; 
        /// <summary>
        /// 【目的】RectTransform がたどる経路の制御点をローカル座標系で指定する。
        /// </summary>
        [SerializeField, LabelText("オブジェクトが通る点 (先頭から順に)")]
        [Tooltip("RectTransform.localPosition をこの順で補間する制御点リスト。")]
        public Vector3[] Path;

        /// <summary>
        /// 【目的】経路の補間方法を指定し、直線移動か曲線移動かを選択できるようにする。
        /// </summary>
        [SerializeField, LabelText("パスの種類 (直線/曲線)")]
        [Tooltip("PathType.Linear で直線補間、CatmullRom で滑らかな曲線補間。")]
        public PathType PathType = PathType.CatmullRom;
    }
}
