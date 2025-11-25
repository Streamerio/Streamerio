using Common.Audio;
using Common.UI.Click;
using R3;
using R3.Triggers;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using Alchemy.Inspector;

namespace Common.UI.Part.Button
{
    [RequireComponent(typeof(UnityEngine.UI.Button), typeof(ObservableEventTrigger))]
    public abstract class ButtonLifetimeScopeBase: LifetimeScope
    {
        /// <summary>
        /// 【目的】入力イベントの発生源となる Unity ボタンを Inspector で参照する。
        /// 【理由】Wiring 時に Button を Resolver へ渡し、Presenter が直接アクセスできるようにするため。
        /// </summary>
        [SerializeField, ReadOnly]
        [Tooltip("ユーザー入力を受け付ける UnityEngine.UI.Button。")]
        private UnityEngine.UI.Button _button;
        
        /// <summary>
        /// 【目的】Pointer イベントを Observable として提供するトリガーを保持する。
        /// 【理由】IClickEventBinder と Presenter が購読し、押下/ホバーを非同期で扱えるようにするため。
        /// </summary>
        [SerializeField, ReadOnly]
        [Tooltip("Pointer イベントを Observable へ変換する R3 Triggers。")]
        private ObservableEventTrigger _eventTrigger;
        /// <summary>
        /// 【目的】ボタン識別用のキーを指定し、DI で該当 Presenter を解決できるようにする。
        /// </summary>
        [SerializeField]
        [Tooltip("DI で解決するボタン Presenter を識別するキー。")]
        private ButtonType _buttonType = ButtonType.Default;
        /// <summary>
        /// 【目的】ボタン押下時に再生する効果音を指定する。
        /// 【理由】UI フィードバックを統一し、コンポーネント再利用時にも SE 設定が引き継がれるようにするため。
        /// </summary>
        [SerializeField, Tooltip("ボタン押下時に再生するSE")]
        private SEType _clickSE = SEType.NESRPG0112;

#if UNITY_EDITOR
        /// <summary>
        /// 【目的】エディタ上で必須参照を自動設定し、設定漏れを防ぐ。
        /// 【理由】プレハブ複製時にフィールドが外れても OnValidate で補完し、実行時例外を避けるため。
        /// </summary>
        protected virtual void OnValidate()
        {
            _button ??= GetComponent<UnityEngine.UI.Button>();
            _eventTrigger ??= GetComponent<ObservableEventTrigger>();
        }
#endif
        /// <summary>
        /// 【目的】共通ボタンに必要な依存登録と Wiring 設定を行う。
        /// 【処理概要】
        ///   1. View を Component 経由で取得し、IButtonView/IInitializable として登録。
        ///   2. クリックイベントを ClickEventBinder でラップし、効果音付きで Presenter に渡す。
        ///   3. CommonButtonPresenter を Wiring で起動し、ボタン構成をコンテキストとして供給する。
        /// 【理由】ボタンを単体プレハブとして生成しても、Presenter が必要な依存を取りこぼさないようにするため。
        /// </summary>
        /// <param name="builder">【用途】依存登録を組み立てる VContainer のビルダー。</param>
        protected override void Configure(IContainerBuilder builder)
        {
            // Viewの登録
            var view = GetComponent<IButtonView>();
            builder.RegisterComponent(view);
            
            builder
                .RegisterEntryPoint<Wiring<ICommonButton, CommonButtonContext>>()
                .WithParameter(resolver => resolver.Resolve<ICommonButton>(key: _buttonType))
                .WithParameter(resolver =>
                {
                    var audioFacade = resolver.Resolve<IAudioFacade>();
                    var binder = new ClickEventBinder<Unit>(_button.OnClickAsObservable(), audioFacade, _clickSE);
                    
                    return new CommonButtonContext
                    {
                        Button = _button,
                        EventTrigger = _eventTrigger,
                        ClickEventBinder = binder,
                        View = view
                    };
                });
        }
    }
}