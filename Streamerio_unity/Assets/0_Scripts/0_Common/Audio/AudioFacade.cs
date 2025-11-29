// モジュール概要:
// アプリ全体のオーディオ制御を統括するファサードと、その契約・コンテキストを定義する。
// 依存関係: BGM/SE プレイヤー、音量・ミュート仲介 (VolumeMediator)、VContainer を通じた DI。
// 使用例: AudioLifeTimeScope が Wiring 経由で AudioFacade を起動し、ゲーム全体で音再生 API を統一提供する。

using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.Audio
{
    /// <summary>
    /// アプリ全体で利用するオーディオ制御の統一インターフェース。  
    /// <para>
    /// - BGM / SE の再生・停止  
    /// - 音量の取得・変更  
    /// - ミュートの取得・切り替え  
    /// など、音に関する操作を一括で提供します。
    /// </para>
    /// </summary>
    public interface IAudioFacade: IAttachable<AudioFacadeContext>
    {
        /// <summary>
        /// 現在の音量状態を取得します。
        /// </summary>
        IReadOnlyDictionary<SoundType, Volume> VolumeDict { get; }

        /// <summary>
        /// 現在のミュート状態を取得します。
        /// </summary>
        IReadOnlyDictionary<SoundType, bool> MuteDict { get; }

        /// <summary>
        /// 指定したサウンドタイプの音量を変更します。
        /// </summary>
        /// <param name="soundType">対象のサウンドタイプ。</param>
        /// <param name="volume">設定する音量。</param>
        void ChangeVolume(SoundType soundType, Volume volume);

        /// <summary>
        /// 指定したサウンドタイプのミュート状態を切り替えます。
        /// </summary>
        /// <param name="soundType">対象のサウンドタイプ。</param>
        void ToggleMute(SoundType soundType);

        /// <summary>
        /// 指定した BGM を再生します。
        /// </summary>
        /// <param name="bgm">再生する BGM の種類。</param>
        /// <param name="ct">キャンセルトークン（任意）。</param>
        UniTask PlayAsync(BGMType bgm, CancellationToken ct = default);

        /// <summary>
        /// 指定した SE を再生します。
        /// </summary>
        /// <param name="se">再生する SE の種類。</param>
        /// <param name="ct">キャンセルトークン（任意）。</param>
        UniTask PlayAsync(SEType se, CancellationToken ct = default);

        /// <summary>
        /// 再生中の BGM をすべて停止します。
        /// </summary>
        void StopBGM();
        
        UniTask StopBGMAsync(CancellationToken ct);

        /// <summary>
        /// 再生中の SE をすべて停止します。
        /// </summary>
        void StopSE();
        
        UniTask StopSEAsync(CancellationToken ct);
    }

    /// <summary>
    /// 【目的】オーディオ機能を一括管理するファサード実装。BGM/SE 再生、音量・ミュート制御などを統合する。
    /// 【理由】複数コンポーネントへ散らばりがちな音制御を一本化し、UI やゲームロジックが簡潔な API で扱えるようにするため。
    /// </summary>
    public class AudioFacade : IAudioFacade
    {
        /// <summary>
        /// 【目的】BGM 再生を担当するプレイヤーを保持する。
        /// 【理由】依存注入で受け取った後も Play/Stop を即座に呼び出せるようにするため。
        /// </summary>
        private IBGMPlayer _bgmPlayer;
        /// <summary>
        /// 【目的】SE 再生を担当するプレイヤーを保持する。
        /// 【理由】短い効果音の再生要求をファサードから委譲できるようにするため。
        /// </summary>
        private ISEPlayer _sePlayer;
        /// <summary>
        /// 【目的】音量制御を仲介するコンポーネントを保持する。
        /// 【理由】ChangeVolume 呼び出し時に AudioMixer へ即反映できるようにするため。
        /// </summary>
        private IVolumeMediator _volumeMediator;
        /// <summary>
        /// 【目的】ミュート状態を仲介するコンポーネントを保持する。
        /// 【理由】ToggleMute 呼び出し時に状態の反転と音量再適用を行うため。
        /// </summary>
        private IMuteMediator _muteMediator;

        /// <summary>
        /// 【目的】Wiring から渡される依存を接続し、ファサードの内部状態を初期化する。
        /// 【理由】プレイヤーやメディエーターをフィールドへキャッシュし、以降の API が利用できるようにするため。
        /// </summary>
        /// <param name="context">【用途】BGM/SE プレイヤーや音量・ミュート仲介をまとめたコンテキスト。</param>
        public void Attach(AudioFacadeContext context)
        {
            _bgmPlayer = context.BgmPlayer;
            _sePlayer = context.SePlayer;
            _volumeMediator = context.VolumeMediator;
            _muteMediator = context.MuteMediator;
        }
        
        /// <summary>
        /// 【目的】参照している依存を解放し、ガベージコレクションを促す。
        /// 【理由】スコープ破棄時に保持参照を残さず、ライフサイクル整合性を保つため。
        /// </summary>
        public void Detach()
        {
            _bgmPlayer = null;
            _sePlayer = null;
            _volumeMediator = null;
            _muteMediator = null;
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<SoundType, Volume> VolumeDict => _volumeMediator.VolumeDict;

        /// <inheritdoc />
        public IReadOnlyDictionary<SoundType, bool> MuteDict => _muteMediator.MuteDict;

        /// <inheritdoc />
        public void ChangeVolume(SoundType soundType, Volume volume)
            => _volumeMediator.ChangeVolume(soundType, volume);

        /// <inheritdoc />
        public void ToggleMute(SoundType soundType)
            => _muteMediator.ToggleMute(soundType);

        /// <inheritdoc />
        public async UniTask PlayAsync(BGMType bgm, CancellationToken ct)
            => await _bgmPlayer.PlayAsync(bgm, ct);

        /// <inheritdoc />
        public async UniTask PlayAsync(SEType se, CancellationToken ct)
            => await _sePlayer.PlayAsync(se, ct);   

        /// <inheritdoc />
        public void StopBGM() => _bgmPlayer.Stop();
        
        public async UniTask StopBGMAsync(CancellationToken ct)
            => await _bgmPlayer.StopAsync(ct);

        /// <inheritdoc />
        public void StopSE() => _sePlayer.Stop();
        
        public async UniTask StopSEAsync(CancellationToken ct)
            => await _sePlayer.StopAsync(ct);
    }

    /// <summary>
    /// 【目的】AudioFacade が必要とする依存（プレイヤーや仲介クラス）を束ねるコンテキスト。
    /// 【理由】Wiring で一括注入し、Attach 内で簡潔に展開できるようにするため。
    /// </summary>
    public class AudioFacadeContext
    {
        /// <summary>
        /// 【目的】BGM 再生を担当するプレイヤー。
        /// 【理由】AudioFacade から BGM 再生/停止を委譲するために保持する。
        /// </summary>
        public IBGMPlayer BgmPlayer;
        /// <summary>
        /// 【目的】SE 再生を担当するプレイヤー。
        /// 【理由】短時間の効果音再生を委譲し、ファサードが音の種類を意識しないようにするため。
        /// </summary>
        public ISEPlayer SePlayer;
        /// <summary>
        /// 【目的】音量設定を管理するメディエーター。
        /// 【理由】ChangeVolume 呼び出し時に AudioMixer へ値を適用するため。
        /// </summary>
        public IVolumeMediator VolumeMediator;
        /// <summary>
        /// 【目的】ミュート状態を管理するメディエーター。
        /// 【理由】ToggleMute 呼び出し時に状態を反転させ、音量を再適用するため。
        /// </summary>
        public IMuteMediator MuteMediator;
    }
}
