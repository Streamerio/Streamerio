// ============================================================================
// モジュール概要: ローディング画面の依存解決スコープを構築し、View と Presenter を VContainer で接続する。
// 外部依存: VContainer（DI フレームワーク）。
// 使用例: ローディングシーンで LoadingLifeTimeScope を配置し、ILoadingScreenView を自動解決する。
// ============================================================================

using Common.UI.Animation;
using Common.UI.Display;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Loading
{
    /// <summary>
    /// ローディング UI 専用の LifetimeScope。
    /// <para>
    /// 【理由】View コンポーネントと Presenter を DI で結び、シーン遷移時にも同一設定を再利用できるようにする。
    /// </para>
    /// </summary>
    public class LoadingLifeTimeScope: DisplayLifetimeScopeBase<ILoadingScreen, LoadingScreenPresenter, ILoadingScreenView, LoadingScreenContext>
    {
        [SerializeField]
        private Material _irisAnimationMaterial;
        
        [SerializeField]
        private IrisAnimationParamSO _showAnimationParamSO;
        [SerializeField]
        private IrisAnimationParamSO _hideAnimationParamSO;
        
        [SerializeField]
        private Animator _playerAnimator;
        
        [SerializeField]
        private CanvasGroup _panel;
        [SerializeField]
        private FadeAnimationParamSO _panelShowAnimationParamSO;
        [SerializeField]
        private FadeAnimationParamSO _panelHideAnimationParamSO;
        
        /// <summary>
        /// 【目的】ローディング画面に必要な依存をコンテナへ登録する。
        /// <para>
        /// - View を ILoadingScreenView / IInitializable として登録し、外部から解決できるようにする。<br/>
        /// - Wiring を EntryPoint として登録し、Presenter 初期化のタイミングを制御する。
        /// </para>
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            var irisMaterialInstance = new Material(_irisAnimationMaterial);
            builder.RegisterInstance(irisMaterialInstance);
            
            builder.RegisterInstance<IIrisAnimation>(new IrisOutAnimation(irisMaterialInstance, _showAnimationParamSO))
                .Keyed(AnimationType.Show);
            builder.RegisterInstance<IIrisAnimation>(new IrisInAnimation(irisMaterialInstance, _hideAnimationParamSO))
                .Keyed(AnimationType.Hide);
            
            builder.RegisterInstance<IUIAnimation>(new FadeAnimation(_panel, _panelShowAnimationParamSO))
                .Keyed(AnimationType.Show);
            builder.RegisterInstance<IUIAnimation>(new FadeAnimation(_panel, _panelHideAnimationParamSO))
                .Keyed(AnimationType.Hide);
            
            base.Configure(builder);
        }

        protected override void BindPresenter(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Wiring<ILoadingScreen, LoadingScreenContext>>()
                .WithParameter(resolver => resolver.Resolve<ILoadingScreen>())
                .WithParameter(CreateContext);
        }

        protected override LoadingScreenContext CreateContext(IObjectResolver resolver)
        {
            return new LoadingScreenContext()
            {
                View = resolver.Resolve<ILoadingScreenView>(),
                PlayerAnimator = _playerAnimator,
            };
        }
    }
}
