// モジュール概要:
// BGM/SE など音声プレイヤーの共通処理を提供する抽象クラス。音源辞書と AudioSourcePool を扱う。
// 依存関係: MusicData 辞書、AudioSourcePoolFactory デリゲート、Cysharp.Threading.Tasks。
// 使用例: BGMPlayer・SEPlayer が継承し、Enum ごとの再生とプール管理を共有する。

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Common.Audio
{
    /// <summary>
    /// BGM / SE など、音声データの再生処理を共通化するための抽象クラス。  
    /// <para>
    /// - Enum（T）ごとの音源データを参照して再生処理を行う  
    /// - AudioSourcePool を使用して再生用 Source を取得・再利用  
    /// - Play / Stop の共通処理を提供し、BGM / SE などで派生クラスを作成して利用する
    /// </para>
    /// </summary>
    /// <typeparam name="T">再生対象を識別する Enum 型（例：BGMType、SEType）。</typeparam>
    public abstract class AudioPlayerBase<T>
    {
        /// <summary>
        /// 再生対象（Enum）ごとの音源データ（AudioClip、容量など）を保持する辞書。
        /// </summary>
        private readonly IReadOnlyDictionary<T, MusicData> _musicDict;
        
        /// <summary>
        /// SoundType と容量を指定して AudioSourcePool を生成するファクトリ。
        /// </summary>
        private readonly Func<SoundType, int, AudioSourcePool> _poolFactory;
        
        /// <summary>
        /// 各 Enum（T）ごとに作成した AudioSourcePool のキャッシュ。
        /// </summary>
        private readonly Dictionary<T, IAudioSourcePoolUser> _audioSourcePoolDict;

        /// <summary>
        /// 再生に使用した Source の記録。Stop 時にまとめて停止するために使用。
        /// </summary>
        private readonly HashSet<Source> _usedSources;
        
        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="musicDict">Enum ごとの音源データ辞書。</param>
        /// <param name="poolFactory">AudioSourcePool を生成するためのファクトリ。</param>
        protected AudioPlayerBase(IReadOnlyDictionary<T, MusicData> musicDict, Func<SoundType, int, AudioSourcePool> poolFactory)
        {
            _musicDict = musicDict;
            _poolFactory = poolFactory;

            _audioSourcePoolDict = new();
            _usedSources = new();
        }

        /// <summary>
        /// 派生クラスで対象の SoundType（BGM / SE など）を返す。
        /// </summary>
        protected abstract SoundType GetSoundType();
        
        /// <summary>
        /// 指定された Enum（T）に対応する音源を非同期で再生します。
        /// </summary>
        /// <param name="music">再生対象の Enum。</param>
        /// <param name="ct">キャンセルトークン。</param>
        public virtual async UniTask PlayAsync(T music, CancellationToken ct)
        {
            await GetSource(music).PlayAsync(_musicDict[music].Clip, ct);
        }

        /// <summary>
        /// 現在使用中のすべての Source を停止します。
        /// </summary>
        public void Stop()
        {
            foreach (var source in _usedSources)
            {
                source.Stop();
            }
        }
        
        public async UniTask StopAsync(CancellationToken ct)
        {
            var stopTasks = new List<UniTask>();
            foreach (var source in _usedSources)
            {
                stopTasks.Add(source.StopAsync(ct));
            }

            await UniTask.WhenAll(stopTasks);
        }

        /// <summary>
        /// 指定された Enum に対応する Source をプールから取得します。  
        /// 必要であれば AudioSourcePool を新規作成してキャッシュします。
        /// </summary>
        private Source GetSource(T music)
        {
            if (!_audioSourcePoolDict.TryGetValue(music, out var pool))
            {
                pool = _poolFactory(GetSoundType(), _musicDict[music].Capacity);
                _audioSourcePoolDict[music] = pool;
            }
            
            var source = pool.GetSource();
            _usedSources.Add(source);

            return source;
        }
    }
}
