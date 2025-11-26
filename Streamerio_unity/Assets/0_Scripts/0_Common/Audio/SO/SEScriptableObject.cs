// モジュール概要:
// SEType と MusicData を紐付ける ScriptableObject。効果音の辞書として SEPlayer が利用する。
// 依存関係: MusicScriptableObjectBase を継承し、エディタ拡張で Enum と AudioClip の整合性を保つ。
// 使用例: SE 用フォルダで ScriptableObject を作成し、AudioLifeTimeScope が辞書を読み込む。

using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// SE（効果音）用のデータを管理する ScriptableObject。  
    /// <para>
    /// - <see cref="MusicScriptableObjectBase{TKey}"/>（SEType）を継承  
    /// - SEType と AudioClip の対応を保持  
    /// - インスペクタ上で SEType ごとに AudioClip を設定可能
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "SE", menuName = "SO/Audio/Music/SE")]
    public class SEScriptableObject : MusicScriptableObjectBase<SEType> { }
}
