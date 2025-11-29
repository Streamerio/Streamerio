// モジュール概要:
// BGM/SE 用 ScriptableObject の共通基底実装。Enum と MusicData の自動連携を提供し、容量などのメタ情報も管理する。
// 依存関係: AutoSetDataScriptableObject を継承し、MusicData 構造体と連携する。
// 使用例: BGMScriptableObject / SEScriptableObject が派生し、AudioLifeTimeScope から音源辞書として利用する。

using System;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// Enum をキーにして <see cref="MusicData"/> を自動登録・管理する ScriptableObject の基底クラス。
    /// <para>
    /// - AutoSetDataScriptableObject を基盤としており、インスペクタ上で Enum 値ごとに AudioClip を簡単に登録できます。<br/>
    /// - BGM 用や SE 用など、用途ごとに派生クラスを作成して利用します。<br/>
    /// - Enum 値と AudioClip の対応は、エディタ拡張により自動生成・同期されます。
    /// </para>
    /// </summary>
    /// <typeparam name="TKey">曲を識別する Enum 型（例：<c>BGMType</c>, <c>SEType</c> など）。</typeparam>
    public abstract class MusicScriptableObjectBase<TKey> 
        : AutoSetDataScriptableObject<TKey, MusicData>
        where TKey : Enum
    {
        [SerializeField, Tooltip("MusicData の初期容量。オブジェクトプールなど容量管理を行う場合に使用します。")]
        private int _defaultCapacity = 1;

#if UNITY_EDITOR
        /// <summary>
        /// <see cref="MusicData"/> の初期容量。
        /// オブジェクトプールなど容量管理を行う場合に利用できます。
        /// </summary>
        public int DefaultCapacity => _defaultCapacity;
#endif
    }
    
    /// <summary>
    /// 音楽データを格納する構造体。
    /// <para>
    /// 基本的には AudioClip と、それに関連する再生設定（容量など）を保持します。
    /// </para>
    /// </summary>
    [Serializable]
    public class MusicData
    {
        /// <summary>
        /// 対応する音声ファイル（BGM / SE）。
        /// </summary>
        [Header("対応する AudioClip（BGM / SE）")]
        public AudioClip Clip;

        /// <summary>
        /// オブジェクトプールなどで使用する初期容量。
        /// </summary>
        [Header("オブジェクトプールなどに利用する初期容量")]
        public int Capacity;
    }
}
