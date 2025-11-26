// ============================================================================
// モジュール概要: オーバーレイ UI の LifetimeScope 基底クラスを提供し、共通部品グループの依存登録を統一する。
// 外部依存: VContainer、Common.UI.Part.Group。
// 使用例: 特定オーバーレイ用に派生させ、共通パーツグループを DI で提供して演出制御を共通化する。
// ============================================================================

using Alchemy.Inspector;
using Common.UI.Animation;
using Common.UI.Display.Background;
using Common.UI.Part;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Overlay
{
    /// <summary>
    /// 【目的】オーバーレイ UI の LifetimeScope を構築する際の共通処理をまとめる基底クラス。
    /// 【理由】各オーバーレイで部品グループの登録を繰り返さず、DI 設定の重複を防ぐ。
    /// </summary>
    public abstract class OverlayLifetimeScopeBase<TOverlay, TPresenter, TView, TContext>: DisplayLifetimeScopeBase<TOverlay, TPresenter, TView, TContext>
        where TOverlay : IOverlay, IAttachable<TContext>
        where TPresenter : TOverlay, IStartable
        where TView : IOverlayView
        where TContext: OverlayContext<TView>
    {
        [SerializeField, ReadOnly]
        private CanvasGroup _canvasGroup;
        
        [SerializeField]
        private FadeAnimationParamSO _showAnimationParam;
        [SerializeField]
        private FadeAnimationParamSO _hideAnimationParam;
        
        [SerializeField]
        private AnimationPartGroup _animationParts;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();
        }
#endif
        
        /// <summary>
        /// 【目的】パーツグループを DI コンテナへ登録し、基底 Configure の登録と組み合わせる。
        /// 【理由】Overlay 毎に共通パーツ制御を利用できるよう、基底で登録しておく。
        /// </summary>
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IDisplayBackground, DisplayBackgroundPresenter>(Lifetime.Singleton);
            
            builder.RegisterInstance<IUIAnimation>(new FadeAnimation(_canvasGroup, _showAnimationParam))
                .Keyed(AnimationType.Show);
            builder.RegisterInstance<IUIAnimation>(new FadeAnimation(_canvasGroup, _hideAnimationParam))
                .Keyed(AnimationType.Hide);
            
            _animationParts.BindAnimation(builder);
            
            base.Configure(builder);
        }
    }
}
