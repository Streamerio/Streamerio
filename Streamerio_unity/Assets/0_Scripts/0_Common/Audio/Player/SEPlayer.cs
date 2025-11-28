// モジュール概要:
// 効果音(SE) 再生の具体実装と公開インターフェースを提供する。AudioPlayerBase を継承して SEType を扱う。
// 依存関係: MusicData 辞書、AudioSourcePoolFactory デリゲート、Cysharp.Threading.Tasks。
// 使用例: AudioFacade が ISEPlayer を介して効果音の再生/停止を指示する。

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.Audio
{
    /// <summary>
    /// SE（効果音）再生の公開インターフェース。
    /// </summary>
    public interface ISEPlayer
    {
        /// <summary>
        /// 指定した SE を再生します。
        /// </summary>
        /// <param name="se">再生する SE の種類。</param>
        /// <param name="ct">キャンセル用トークン（任意）。</param>
        UniTask PlayAsync(SEType se, CancellationToken ct = default);

        /// <summary>
        /// 再生中の SE をすべて停止します。
        /// </summary>
        void Stop();
        
        UniTask StopAsync(CancellationToken ct);
    }

    /// <summary>
    /// SE（効果音）の再生を担当するクラス。  
    /// <see cref="AudioPlayerBase{T}"/> を継承して、効果音再生の共通処理を利用します。
    /// </summary>
    public class SEPlayer : AudioPlayerBase<SEType>, ISEPlayer
    {
        /// <summary>
        /// DI 用コンストラクタ。
        /// </summary>
        /// <param name="musicDict">SEType ごとの音源データ。</param>
        /// <param name="poolFactory">SoundType と容量から AudioSourcePool を生成するファクトリ。</param>
        public SEPlayer(
            IReadOnlyDictionary<SEType, MusicData> musicDict,
            Func<SoundType, int, AudioSourcePool> poolFactory
        ) : base(musicDict, poolFactory)
        {
        }

        /// <summary>
        /// このプレイヤーが扱うサウンド種別（SE）を返します。
        /// </summary>
        protected override SoundType GetSoundType() => SoundType.SE;

        /// <inheritdoc />
        public async UniTask PlayAsync(SEType se, CancellationToken ct = default)
        {
            // None は無音扱いのため、再生しない
            if (se == SEType.None) return;

            await base.PlayAsync(se, ct);
        }
    }
}
