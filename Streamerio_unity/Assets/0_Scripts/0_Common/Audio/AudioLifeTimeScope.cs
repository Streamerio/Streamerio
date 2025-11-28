// モジュール概要:
// オーディオ関連依存をまとめて登録する VContainer LifetimeScope。AudioMixer、プレイヤー、ボリューム仲介を初期化する。
// 依存関係: AudioMixer、AudioMixerParameterSO、BGM/SE ScriptableObject、AudioSourcePoolFactory。
// 使用例: ゲームルートに配置し、他シーンから IAudioFacade を解決できるようにする。

using UnityEngine;
using UnityEngine.Audio;
using VContainer;
using VContainer.Unity;

namespace Common.Audio
{
    /// <summary>
    /// <para>Audio 関連（BGM/SE、AudioMixer、Volume/Mute Mediator など）をまとめて管理するObject。</para>
    /// <para>
    /// VContainer の DI により SaveManager を受け取り、  
    /// AudioMixer・各種 Mediator・Player の初期化を行い、  
    /// 外部に参照可能なプロパティとして公開する。
    /// </para>
    /// </summary>
    public class AudioLifeTimeScope : LifetimeScope
    {
        [Header("AudioMixer本体")]
        [Tooltip("音量調整の対象となる AudioMixer アセット。")]
        [SerializeField] 
        private AudioMixer _audioMixer;
        
        [Header("AudioMixer パラメータ定義")]
        [Tooltip("各 SoundType に対応する AudioMixer の Volume パラメータ名辞書を持つ ScriptableObject。")]
        [SerializeField] 
        private AudioMixerParameterSO _audioMixerParameterSO;

        [Header("SoundTypeごとのAudioSourceプレハブと親Transform")]
        [Tooltip("各 SoundType に対応する Source プレハブと Transform の辞書。")]
        [SerializeField] 
        private SerializeDictionary<SoundType, SourceData> _sourceDict;

        [Header("BGM/SE データ辞書")]
        [Tooltip("BGM 用の再生データ辞書 ScriptableObject。")]
        [SerializeField] 
        private BGMScriptableObject _bgmScriptableObject;

        [Tooltip("SE 用の再生データ辞書 ScriptableObject。")]
        [SerializeField] 
        private SEScriptableObject _seScriptableObject;

        /// <summary>
        /// 【目的】オーディオ関連の依存を DI コンテナへ登録する。
        /// 【処理概要】AudioMixer 制御 → ボリューム/ミュート仲介 → プレイヤー生成 → AudioFacade Wiring の順で構成する。
        /// 【理由】オーディオ機能を一括で初期化し、アプリ全体から共通ファサード経由で利用できるようにするため。
        /// </summary>
        /// <param name="builder">【用途】依存登録とエントリポイント設定を行う VContainer のビルダー。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IVolumeController, AudioMixerMediator>(Lifetime.Singleton)
                .WithParameter(_audioMixer)
                .WithParameter(_audioMixerParameterSO.VolumeParamDict);

            builder.Register<IVolumeMediator, IMuteMediator, VolumeMediator>(Lifetime.Singleton)
                .As<IStartable>();

            var audioSourcePoolFactory = new AudioSourcePoolFactory(_sourceDict.ToDictionary());
            var bgmPlayer = new BGMPlayer(_bgmScriptableObject.Dictionary, audioSourcePoolFactory.Create);
            var sePlayer = new SEPlayer(_seScriptableObject.Dictionary, audioSourcePoolFactory.Create);
            builder.RegisterInstance<IBGMPlayer, BGMPlayer>(bgmPlayer);
            builder.RegisterInstance<ISEPlayer, SEPlayer>(sePlayer);

            builder.RegisterEntryPoint<Wiring<IAudioFacade, AudioFacadeContext>>()
                .WithParameter(resolver =>
                {
                    return new AudioFacadeContext
                    {
                        BgmPlayer = resolver.Resolve<IBGMPlayer>(),
                        SePlayer = resolver.Resolve<ISEPlayer>(),
                        VolumeMediator = resolver.Resolve<IVolumeMediator>(),
                        MuteMediator = resolver.Resolve<IMuteMediator>(),
                    };
                });
        }
    }
}
