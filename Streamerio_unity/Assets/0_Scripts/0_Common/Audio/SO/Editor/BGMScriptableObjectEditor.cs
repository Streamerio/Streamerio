// モジュール概要:
// BGMScriptableObject のインスペクタ UI を拡張し、Enum と AudioClip の自動設定を支援する。
// 依存関係: MusicScriptableObjectEditor を継承し、BGMType 用の設定を適用する。

using UnityEditor;

namespace Common.Audio.Editor
{
    /// <summary>
    /// 【目的】BGMScriptableObject 向けのエディタ拡張を提供する。
    /// 【理由】音源を Enum と同期させる操作を共通実装へ委譲し、BGM 用に適用するため。
    /// </summary>
    [CustomEditor(typeof(BGMScriptableObject))]
    public class BGMScriptableObjectEditor: MusicScriptableObjectEditor<BGMType, BGMScriptableObject>
    {
        
    }
}
