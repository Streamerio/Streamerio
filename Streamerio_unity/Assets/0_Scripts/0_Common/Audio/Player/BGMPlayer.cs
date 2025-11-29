// モジュール概要:
// BGM 再生の具体実装と公開インターフェースを提供する。AudioPlayerBase を継承して BGMType を扱う。
// 依存関係: MusicData 辞書、AudioSourcePoolFactory デリゲート、Cysharp.Threading.Tasks。
// 使用例: AudioFacade が IBGMPlayer を介して BGM 再生/停止を指示する。

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.Audio
{
    /// <summary>
    /// BGM 再生の公開インターフェース。  
    /// <para>
    /// BGM の再生・停止機能を統一的に提供します。
    /// </para>
    /// </summary>
    public interface IBGMPlayer
    {
        /// <summary>
        /// 指定した BGM を再生します。
        /// </summary>
        /// <param name="bgm">再生する BGM の種類。</param>
        /// <param name="ct">キャンセル用トークン。</param>
        UniTask PlayAsync(BGMType bgm, CancellationToken ct);

        /// <summary>
        /// 再生中の BGM を停止します。
        /// </summary>
        void Stop();
        
        UniTask StopAsync(CancellationToken ct);
    }

    /// <summary>
    /// BGM 再生を担当するクラス。  
    /// <see cref="AudioPlayerBase{T}"/> を継承し、BGM 再生に特化した処理を提供します。
    /// </summary>
    public class BGMPlayer : AudioPlayerBase<BGMType>, IBGMPlayer
    {
        /// <summary>
        /// DI 用コンストラクタ。
        /// </summary>
        /// <param name="musicDict">BGMType ごとの音源データ。</param>
        /// <param name="poolFactory">SoundType と容量から AudioSourcePool を生成するファクトリ。</param>
        public BGMPlayer(
            IReadOnlyDictionary<BGMType, MusicData> musicDict,
            Func<SoundType, int, AudioSourcePool> poolFactory
        ) : base(musicDict, poolFactory)
        {
        }

        /// <summary>
        /// このプレイヤーが扱うサウンド種別（BGM）を返します。
        /// </summary>
        protected override SoundType GetSoundType() => SoundType.BGM;

        /// <inheritdoc />
        public override async UniTask PlayAsync(BGMType bgm, CancellationToken ct)
        {
            // None は再生しない（無音扱い）
            if (bgm == BGMType.None) return;
            
            await base.PlayAsync(bgm, ct);
        }
    }
}
