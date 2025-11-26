// モジュール概要:
// 音量値を UI 表現(0-100)と AudioMixer 用 dB 表現に変換して保持する値オブジェクト。
// 依存関係: UnityEngine.Mathf を用いてクランプと対数変換を実施。
// 使用例: VolumeMediator が辞書に保持し、AudioMixerMediator へ MixerValue を渡す。

using System;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// 音量データを表すクラス。
    /// - UI などから扱う「0〜100」の値と、AudioMixer に渡す「dB 値」を両方保持する
    /// - 入力値は常に 0〜100 の範囲にクランプされる
    /// - AudioMixer 用には対数変換で dB に変換
    /// </summary>
    [Serializable]
    public class Volume
    {
        /// <summary>音量のデフォルト値 (0〜100)</summary>
        public const float DEFAULT_VALUE = 50;

        /// <summary>UIで扱う最小/最大音量 (0〜100)</summary>
        public const float MIN_VALUE = 0;
        public const float MAX_VALUE = 100;

        /// <summary>AudioMixer に設定可能な最小/最大の dB 値</summary>
        private const float MIN_MIXER_VOLUME = -80f;
        private const float MAX_MIXER_VOLUME = 20f;

        /// <summary>
        /// 実際に AudioMixer に反映する最大値（20dB まで上げると大きすぎるため 0dB を上限にしている）
        /// </summary>
        private const float CUSTOM_MAX_VOLUME = 0f;

        [SerializeField, Header("音量"), Range(MIN_VALUE, MAX_VALUE)]
        private float _value = 0f;

        /// <summary>
        /// 0〜100 の音量値。UIやセーブデータなどで使う。
        /// </summary>
        public float Value => _value;

        private float _mixerValue;
        /// <summary>
        /// AudioMixer に適用する dB 値。
        /// </summary>
        public float MixerValue => _mixerValue;

#if UNITY_EDITOR
        /// <summary>
        /// Inspector 上で値を変更した際に自動で dB 値を更新。
        /// </summary>
        private void OnValidate()
        {
            _mixerValue = ConvertVolume2dB(_value);
        }
#endif

        /// <summary>
        /// コンストラクタ。
        /// - 渡された値を 0〜100 にクランプ
        /// - dB 値に変換して保持
        /// </summary>
        /// <param name="value">0〜100 の音量値</param>
        public Volume(float value)
        {
            _value = Mathf.Clamp(value, MIN_VALUE, MAX_VALUE);
            _mixerValue = ConvertVolume2dB(_value);
        }

        /// <summary>
        /// 音量 (0〜100) を AudioMixer 用の dB 値に変換する。
        /// 対数変換を用いて、人間の聴覚に近い音量変化を再現。
        /// </summary>
        private float ConvertVolume2dB(float volume)
        {
            return Mathf.Clamp(
                MAX_MIXER_VOLUME * Mathf.Log10(ScaleToZeroToOne(volume)),
                MIN_MIXER_VOLUME,
                CUSTOM_MAX_VOLUME
            );
        }

        /// <summary>
        /// 音量 (0〜100) を 0〜1 にスケーリング。
        /// - Log10 に渡すため 0 〜 1 に正規化する
        /// </summary>
        private float ScaleToZeroToOne(float volume)
        {
            return Mathf.Clamp(volume, MIN_VALUE, MAX_VALUE) / MAX_VALUE;
        }
    }
}
