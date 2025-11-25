// モジュール概要:
// BGMType と MusicData を紐付ける ScriptableObject。AudioLifeTimeScope から参照され BGM 再生に利用される。
// 依存関係: MusicScriptableObjectBase を継承し、AutoSetDataScriptableObject により Enum 値を自動管理する。
// 使用例: エディタで BGMType ごとに AudioClip を割り当て、BGMPlayer が辞書として利用する。

using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// BGM 用のデータを管理する ScriptableObject。  
    /// <para>
    /// - <see cref="MusicScriptableObjectBase{TKey}"/>（BGMType）を継承  
    /// - BGMType と AudioClip の対応を保持  
    /// - AudioManager から参照され、BGM 再生に使用される
    /// </para>
    /// </summary>
    [CreateAssetMenu(fileName = "BGM", menuName = "SO/Audio/Music/BGM")]
    public class BGMScriptableObject : MusicScriptableObjectBase<BGMType> { }
}
