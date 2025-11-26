// モジュール概要:
// AudioMixer と SoundType の対応を保持し、音量変更要求を実際の AudioMixer.SetFloat へ反映するミドルレイヤー。
// 依存関係: UnityEngine.AudioMixer と SoundType→パラメータ名の辞書。
// 使用例: VolumeMediator が ChangeVolume を呼び出し、AudioMixer に音量を適用する。

using System.Collections.Generic;
using UnityEngine.Audio;

namespace Common.Audio
{
    /// <summary>
    /// <see cref="AudioMixer"/> の音量制御を担当するクラス。  
    /// <para>
    /// - <see cref="SoundType"/> と AudioMixer のパラメータ名を対応付けて管理  
    /// - <see cref="IVolumeController"/> を実装し、音量変更処理を統一的に提供  
    /// - 実際の <see cref="AudioMixer.SetFloat(string, float)"/> 呼び出しをカプセル化
    /// </para>
    /// </summary>
    public class AudioMixerMediator : IVolumeController
    {
        /// <summary>
        /// 対象となる AudioMixer。
        /// </summary>
        private readonly AudioMixer _audioMixer; 

        /// <summary>
        /// 各 <see cref="SoundType"/> に対応する AudioMixer パラメータ名のマッピング。
        /// </summary>
        private readonly IReadOnlyDictionary<SoundType, string> _volumeParameterDict;

        /// <summary>
        /// コンストラクタ。AudioMixer とパラメータ名マッピングを受け取り、内部に保持します。
        /// </summary>
        /// <param name="audioMixer">対象の AudioMixer。</param>
        /// <param name="volumeParamDict">SoundType とパラメータ名の対応表。</param>
        public AudioMixerMediator(AudioMixer audioMixer, IReadOnlyDictionary<SoundType, string> volumeParamDict)
        {
            _audioMixer = audioMixer;
            _volumeParameterDict = volumeParamDict;
        }

        /// <inheritdoc />
        public void ChangeVolume(SoundType soundType, Volume volume)
        {
            Debug.Log(_volumeParameterDict[soundType] + " : " + volume.MixerValue);
            _audioMixer.SetFloat(_volumeParameterDict[soundType], volume.MixerValue);
        }
    }
}
