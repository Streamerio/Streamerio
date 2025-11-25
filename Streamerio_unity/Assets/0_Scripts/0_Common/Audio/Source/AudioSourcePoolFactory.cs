// モジュール概要:
// SoundType ごとの Source プレハブ設定から AudioSourcePool を生成するファクトリ。
// 依存関係: SourceData 辞書（プレハブと親 Transform）を参照し、AudioSourcePool を初期化する。
// 使用例: AudioLifeTimeScope がファクトリを作成し、BGM/SE プレイヤーが必要に応じてプールを生成する。

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// <para>SoundType ごとに AudioSourcePool を生成するファクトリクラス。</para>
    /// <para>AudioSource のプレハブと親 Transform を SourceData 辞書から参照し、指定数のプールを構築する。</para>
    /// </summary>
    public class AudioSourcePoolFactory
    {
        /// <summary>
        /// SoundType をキーとして Source プレハブと親 Transform を保持する辞書。
        /// </summary>
        private readonly IReadOnlyDictionary<SoundType, SourceData> _sourceDict;
        
        /// <summary>
        /// ファクトリを初期化します。
        /// </summary>
        /// <param name="sourceDict">SoundType ごとの SourceData 辞書。</param>
        public AudioSourcePoolFactory(IReadOnlyDictionary<SoundType, SourceData> sourceDict)
        {
            _sourceDict = sourceDict;
        }
        
        /// <summary>
        /// 指定した SoundType 用の AudioSourcePool を生成します。
        /// </summary>
        /// <param name="soundType">生成対象のサウンド種別。</param>
        /// <param name="capacity">プールする AudioSource の初期数。</param>
        /// <returns>
        /// 成功時は AudioSourcePool インスタンス、<br/>
        /// 辞書に SoundType が存在しない場合は null を返します。
        /// </returns>
        public AudioSourcePool Create(SoundType soundType, int capacity)
        {
            if (!_sourceDict.TryGetValue(soundType, out var sourceData))
            {
                Debug.LogError($"[AudioSourcePoolFactory] SourceData not found for SoundType: {soundType}");
                return null;
            }

            return new AudioSourcePool(sourceData.Source, sourceData.Parent, capacity);
        }
    }
    
    /// <summary>
    /// AudioSourcePool の生成に必要なプレハブと Transform をまとめたデータ構造。
    /// </summary>
    [Serializable]
    public class SourceData
    {
        [Tooltip("AudioSource のプレハブ。各 SoundType ごとに用意する。")]
        public Source Source; 

        [Tooltip("生成した AudioSource を配置する親 Transform。")]
        public Transform Parent;
    }
}
