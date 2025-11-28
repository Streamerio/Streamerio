// モジュール概要:
// AudioMixer のパラメータ名を ScriptableObject で管理し、コード側で安全に参照する。
// 依存関係: SerializeDictionary で SoundType ごとのパラメータ名を保持。
// 使用例: AudioLifeTimeScope が VolumeParamDict を渡し、AudioMixerMediator が音量制御に利用する。

using Alchemy.Inspector;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// AudioMixer のパラメータ名を管理する ScriptableObject。
    /// - VolumeType (Master / BGM / SE など) と AudioMixer 側のパラメータ名を紐づけて保持
    /// - コードから直接文字列を書くのを避け、SO 経由で参照することで安全性と可読性を向上
    /// </summary>
    [CreateAssetMenu(fileName = "AudioMixerParameterSO", menuName = "SO/Audio/AudioMixerParameter")]
    public class AudioMixerParameterSO : ScriptableObject
    {
        [SerializeField, LabelText("オーディオミキサーの音量パラメータ名 (VolumeType → string)")]
        private SerializeDictionary<SoundType, string> _volumeParamDict;

        /// <summary>
        /// 読み取り専用の辞書として公開。
        /// - VolumeType ごとのパラメータ名を取得可能
        /// </summary>
        public IReadOnlyDictionary<SoundType, string> VolumeParamDict => _volumeParamDict.ToDictionary();
    }
}
