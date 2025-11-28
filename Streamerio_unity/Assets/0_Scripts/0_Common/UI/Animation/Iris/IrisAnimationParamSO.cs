// ============================================================================
// モジュール概要: イリスアニメーション用のシェーダープロパティ設定を ScriptableObject で管理する。
// 外部依存: Alchemy.Inspector（LabelText）、UnityEngine。
// 使用例: LoadingScreenView が IrisAnimationComponentParamSO を参照し、シーン遷移ごとに半径や中心を切り替える。
// ============================================================================

using Alchemy.Inspector;
using UnityEngine;

namespace Common.UI.Animation
{
    /// <summary>
    /// Irisアニメーションのパラメータ。
    /// - 中心座標
    /// - 半径プロパティ名
    /// - 開閉範囲（Min/Max半径）
    /// </summary>
    [CreateAssetMenu(fileName = "IrisAnimationSO", menuName = "SO/UI/Animation/Iris")]
    public class IrisAnimationParamSO : UIAnimationComponentParamSO
    {
        /// <summary>
        /// 【目的】マテリアル上で円の中心を表すプロパティ名を指定し、カスタムシェーダと連携する。
        /// </summary>
        [SerializeField, LabelText("円の中心のプロパティ名")]
        [Tooltip("マテリアルに存在する円中心 (Vector2) のプロパティ名。例: _CenterUV")]
        public string CenterPropertyName = "_CenterUV";
        
        /// <summary>
        /// 【目的】円の半径を制御するプロパティ名を保持し、Tween 先をアセット側で決定できるようにする。
        /// </summary>
        [SerializeField, LabelText("円の半径のプロパティ名")]
        [Tooltip("マテリアルに存在する円半径 (float) のプロパティ名。例: _Radius")]
        public string RadiusPropertyName = "_Radius";
        
        /// <summary>
        /// 【目的】円の中心位置 (UV) を指定し、クリック位置など演出の起点を調整可能にする。
        /// </summary>
        [SerializeField, LabelText("円の中心位置 (UV座標)")]
        [Tooltip("0-1 の UV 座標で指定する中心位置。0.5,0.5 で画面中央。")]
        public Vector2 Center = new(0.5f, 0.5f);
        
        /// <summary>
        /// 【目的】閉じ演出時に到達したい最小半径を設定し、完全に閉じる距離を決める。
        /// </summary>
        [SerializeField, LabelText("イリスの最小半径")]
        [Tooltip("イリスが最も閉じた状態の半径。0 で完全に閉じる。")]
        public float MinRadius = 0f;
        
        /// <summary>
        /// 【目的】開き演出時に到達する最大半径を設定し、画面外までの開き具合を調整する。
        /// </summary>
        [SerializeField, LabelText("イリスの最大半径")]
        [Tooltip("イリスが開ききったときの半径。画面全体を覆えるよう 1 以上を推奨。")]
        public float MaxRadius = 1.5f;
    }
}
