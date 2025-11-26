// モジュール概要:
// 個別に再生される AudioSource ラッパー。プールに対応し、フェード停止やミュート切替などのユーティリティを提供する。
// 依存関係: UnityEngine.AudioSource、Cysharp.Threading.Tasks、DOTween を使用し、プールからの再利用を可能にする。
// 使用例: AudioSourcePool が Source を生成・初期化し、AudioPlayerBase が PlayAsync/Stop を呼び出す。

using System;
using System.Threading;

using Alchemy.Inspector;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Common.Audio
{
    /// <summary>
    /// 個別のオーディオ再生を担当するコンポーネント。
    /// - Unity の AudioSource を内包して制御を簡略化
    /// - 再生完了や停止時に Dispose を呼んでプールへ返却
    /// - フェード付き停止やミュート切替にも対応
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Source : MonoBehaviour, IDisposable
    {
        /// <summary>
        /// 【目的】実際の音再生に使用する AudioSource を参照する。
        /// 【理由】プールから復帰したときに再取得を避け、各再生 API が即座に利用できるようにするため。
        /// </summary>
        [SerializeField, ReadOnly]
        private AudioSource _audioSource;
        
        [SerializeField]
        private float _stopFadeDuration = 1f;

        /// <summary>
        /// 【目的】再生終了時にプールへ返却するコールバック。
        /// 【理由】AudioSourcePool が Release を呼べるようにし、利用側が返却を意識せずに済ませるため。
        /// </summary>
        private Action _onDisable;                // 再生終了時に呼ぶコールバック（プールに返す用）
        /// <summary>
        /// 【目的】破棄や外部キャンセルと連動するトークンを保持する。
        /// 【理由】PlayAsync/StopAsync 実行中に GameObject が破棄された場合でも漏れなく終了させるため。
        /// </summary>
        private LinkedCancellationToken _lct;    // 外部と連動したキャンセルトークン

#if UNITY_EDITOR
        protected void OnValidate()
        {
            // Editor 上で AudioSource を自動取得
            _audioSource ??= GetComponent<AudioSource>();
        }  
#endif

        /// <summary>
        /// 【目的】プールから取り出された際に返却用コールバックとキャンセル連携をセットアップする。
        /// 【理由】再生終了時に Release できるようにし、Reuse までのリソース管理を安定させるため。
        /// </summary>
        /// <param name="onDisable">【用途】Dispose 時に呼び出すプール返却コールバック。</param>
        public void Initialize(Action onDisable)
        {
            _onDisable = onDisable;
            _lct = new LinkedCancellationToken(destroyCancellationToken);
        }

        /// <summary>
        /// 【目的】指定した AudioClip を再生し、終了するまで待機する。
        /// 【理由】BGM/SE プレイヤーが非同期で再生完了を待てるようにするため。
        /// </summary>
        /// <param name="clip">【用途】再生する AudioClip。</param>
        /// <param name="ct">【用途】外部から再生を中断したい場合に使用する CancellationToken。</param>
        public async UniTask PlayAsync(AudioClip clip, CancellationToken ct)
        {
            _audioSource.clip = clip;
            _audioSource.Play();

            // 再生が終了するまで待機（外部キャンセルにも対応）
            await UniTask.WaitUntil(() => !_audioSource.isPlaying, cancellationToken: _lct.GetCancellationToken(ct));

            Dispose();
        }

        /// <summary>
        /// 【目的】再生を即座に停止し、プールへ返却する。
        /// 【理由】SE の打ち切りなど遅延を許容しないケースで即時停止できるようにするため。
        /// </summary>
        public void Stop()
        {
            _audioSource.Stop();
            Dispose();
        }

        /// <summary>
        /// 【目的】指定時間でフェードアウトしながら停止する。
        /// 【理由】BGM の切り替えなど、唐突な音切れを避けたいケースに対応するため。
        /// </summary>
        /// <param name="ct">【用途】フェード途中で停止処理をキャンセルするためのトークン。</param>
        public async UniTask StopAsync(CancellationToken ct)
        {
            await _audioSource
                .DOFade(0, _stopFadeDuration)
                .SetEase(Ease.InOutQuad)
                .ToUniTask(cancellationToken: _lct.GetCancellationToken(ct));

            Stop();
            _audioSource.volume = 1f; // フェード後に音量を元に戻す
        }

        /// <summary>
        /// 【目的】ミュート状態を切り替える。
        /// 【理由】VolumeMediator からのミュート指示に従い、個別 Source の発音を抑制するため。
        /// </summary>
        /// <param name="isMute">【用途】true でミュート、false でミュート解除。</param>
        public void SetMute(bool isMute)
        {
            _audioSource.mute = isMute;
        }

        /// <summary>
        /// 【目的】再生終了や停止後のクリーンアップを行い、プールへ返却する。
        /// 【理由】Source 再利用時に前回のコールバックやトークンが残らないようにするため。
        /// </summary>
        public void Dispose()
        {
            _onDisable?.Invoke();
            _onDisable = null;
            _lct.Dispose();
        }
    }
}
