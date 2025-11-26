// モジュール概要:
// SEScriptableObject のインスペクタ UI を拡張し、効果音の Enum と AudioClip を自動同期させる。
// 依存関係: MusicScriptableObjectEditor を継承し、SEType 用の設定を適用する。

using UnityEditor;

namespace Common.Audio.Editor
{
    /// <summary>
    /// 【目的】SE 用 ScriptableObject の自動設定エディタを提供する。
    /// 【理由】効果音データ登録の手間を減らし、Enum と AudioClip の整合性を保つため。
    /// </summary>
    [CustomEditor(typeof(SEScriptableObject))]
    public class SEScriptableObjectEditor: MusicScriptableObjectEditor<SEType, SEScriptableObject>
    {
        
    }
}
