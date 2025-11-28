// モジュール概要:
// Source プレハブを ObjectPool で管理し、音声再生時に効率的な取得と返却を行う。
// 依存関係: UnityEngine.Pool.ObjectPool、Source コンポーネント。
// 使用例: AudioPlayerBase が SoundType ごとにプールを生成し、GetSource() で Source を取得して再生する。

using UnityEngine;
using UnityEngine.Pool;

namespace Common.Audio
{
    /// <summary>
    /// <see cref="AudioSourcePool"/> を利用する側のインターフェイス。
    /// </summary>
    public interface IAudioSourcePoolUser
    {
        /// <summary>
        /// プールから Source を取得します。
        /// </summary>
        Source GetSource();

        /// <summary>
        /// プールをクリアして全インスタンスを破棄します。
        /// </summary>
        void Clear();
    }
    
    /// <summary>
    /// オーディオソース (<see cref="Source"/>) をプールで管理するクラス。  
    /// <para>
    /// - <see cref="ObjectPool{T}"/> を利用して Source を効率的に再利用します。<br/>
    /// - AudioManager 経由で BGM / SE の再生に使用されます。<br/>
    /// - 再生終了時に Source 側から自動で Release されるため、明示的な返却は不要です。
    /// </para>
    /// </summary>
    public class AudioSourcePool : IAudioSourcePoolUser
    {
        private readonly Source _prefab;    // 生成元となるプレハブ
        private readonly Transform _parent; // 親 Transform（Hierarchy 整理用）
        private readonly ObjectPool<Source> _sourcePool;

        /// <summary>
        /// プールを初期化します。
        /// </summary>
        /// <param name="source">生成元となる Source プレハブ。</param>
        /// <param name="parent">生成時の親 Transform。</param>
        /// <param name="capacity">プールの初期容量。</param>
        public AudioSourcePool(Source source, Transform parent, int capacity)
        {
            _prefab = source;
            _parent = parent;

            _sourcePool = new ObjectPool<Source>(
                createFunc: OnCreateSource,
                actionOnGet: OnGetSource,
                actionOnRelease: OnReleaseSource,
                actionOnDestroy: OnDestroySource,
                defaultCapacity: capacity
            );
        }

        /// <summary>
        /// プールから Source を取得します。  
        /// 利用後は Source 側で自動的に Release されます。
        /// </summary>
        public Source GetSource()
        {
            return _sourcePool.Get();
        }

        /// <summary>
        /// プールをクリアして全インスタンスを破棄します。
        /// </summary>
        public void Clear()
        {
            _sourcePool.Clear();
        }

        // =============================
        // ObjectPool 用コールバック群
        // =============================

        /// <summary>
        /// 新しい Source インスタンスを生成します。
        /// </summary>
        private Source OnCreateSource()
        {
            return Object.Instantiate(_prefab, _parent);
        }

        /// <summary>
        /// プールから取得されたときの処理。  
        /// Release 時にプールへ戻すコールバックを設定します。
        /// </summary>
        private void OnGetSource(Source source)
        {
            source.Initialize(() => _sourcePool.Release(source));
        }

        /// <summary>
        /// プールに返却されたときの処理。  
        /// 必要に応じてリセット処理を追加できます。
        /// </summary>
        private void OnReleaseSource(Source source)
        {
            // 特別な処理はなし
        }

        /// <summary>
        /// プールから削除されるときの処理。  
        /// Dispose を呼んでから GameObject を破棄します。
        /// </summary>
        private void OnDestroySource(Source source)
        {
            source.Dispose();
            Object.Destroy(source.gameObject);
        }
    }
}
