using System;
using System.Collections.Generic;
using Common.Audio;
using UnityEngine;

namespace Common.Save
{
    /// <summary>
    /// 音量データのセーブ／ロード処理を抽象化するためのインターフェイス。
    /// <para>
    /// 実装を差し替えることで、セーブ先を PlayerPrefs 以外（ファイル、クラウドなど）に
    /// 変更できるようにします。
    /// </para>
    /// </summary>
    public interface IVolumeSaveFacade
    {
        /// <summary>
        /// 指定したサウンドタイプの音量をセーブします。
        /// </summary>
        /// <param name="soundType">対象のサウンドタイプ。</param>
        /// <param name="volume">セーブする音量値。</param>
        void SaveAudioVolume(SoundType soundType, Volume volume);

        /// <summary>
        /// セーブされているすべてのサウンドタイプの音量をロードします。
        /// </summary>
        /// <returns>サウンドタイプと音量の対応表。</returns>
        Dictionary<SoundType, Volume> LoadVolumes();
    }
    
    /// <summary>
    /// プレイ状態データのセーブ／ロード処理を抽象化するためのインターフェイス。
    /// <para>
    /// 「一度遊んだことがあるか」などの簡易的な進行状況データを扱います。
    /// </para>
    /// </summary>
    public interface IPlayDataSaveFacade
    {
        /// <summary>
        /// 「一度プレイした」情報をセーブします。
        /// </summary>
        /// <param name="isPlayed">プレイ済みとする場合は true（デフォルト）。</param>
        void SavePlayed(bool isPlayed = true);

        /// <summary>
        /// 「一度プレイした」情報をロードします。
        /// </summary>
        /// <returns>true の場合はプレイ済み。</returns>
        bool LoadPlayed();
    }
    
    /// <summary>
    /// ゲーム内のセーブ／ロード処理をまとめて行うクラス。
    /// <para>
    /// 現在は PlayerPrefs を利用して音量設定や初回プレイ状態などを管理しています。
    /// 将来的にセーブ先を変更したい場合は、各インターフェイスを実装する別クラスに差し替えることで対応可能です。
    /// </para>
    /// </summary>
    public class SaveManager : IVolumeSaveFacade, IPlayDataSaveFacade
    {
        /// <summary>
        /// 「一度プレイしたか」を表す PlayerPrefs のキー。
        /// </summary>
        private const string _playedKey = "Played";

        /// <summary>
        /// リトライ状態を示すフラグ。
        /// </summary>
        /// <remarks>
        /// 現在は一時的な用途のため public フィールドですが、
        /// 将来的にはプロパティ化や別クラスへの分離を検討してください。
        /// </remarks>
        public bool IsRetry = false;
        
        /// <inheritdoc />
        public void SavePlayed(bool isPlayed = true)
        {
            PlayerPrefs.SetInt(_playedKey, isPlayed ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        /// <inheritdoc />
        public bool LoadPlayed()
        {
            return PlayerPrefs.GetInt(_playedKey, 0) == 1;
        }
        
        /// <inheritdoc />
        public void SaveAudioVolume(SoundType soundType, Volume volume)
        {
            PlayerPrefs.SetFloat(soundType.ToString(), volume.Value);
            PlayerPrefs.Save();
        }

        /// <inheritdoc />
        public Dictionary<SoundType, Volume> LoadVolumes()
        {
            var volumes = new Dictionary<SoundType, Volume>();

            foreach (SoundType type in Enum.GetValues(typeof(SoundType)))
            {
                float volumeValue = PlayerPrefs.GetFloat(type.ToString(), Volume.DEFAULT_VALUE);
                volumes[type] = new Volume(volumeValue);
                Debug.Log($"[SaveManager] Load Volume: {type} = {volumeValue}");
            }

            return volumes;
        }
    }
}
