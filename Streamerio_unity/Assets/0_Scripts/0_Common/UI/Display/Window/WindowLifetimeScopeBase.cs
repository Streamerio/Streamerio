using Common.UI.Animation;
using Common.UI.Display.Background;
using Common.UI.Part;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Display.Window
{
    public abstract class WindowLifetimeScopeBase<TWindow, TPresenter, TView, TContext>: DisplayLifetimeScopeBase<TWindow, TPresenter, TView, TContext>
        where TWindow : IWindow, IAttachable<TContext>
        where TPresenter : TWindow, IStartable
        where TView : IWindowView
        where TContext: WindowContext<TView>
    {
        [SerializeField]
        private RectTransform _displayRectTransform;
        
        [SerializeField]
        private MoveAnimationParamSO _showAnimParam;
        [SerializeField]
        private MoveAnimationParamSO _hideAnimParam;

        [SerializeField] private AnimationPartGroup _animationPartGroup;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IDisplayBackground, DisplayBackgroundPresenter>(Lifetime.Singleton);
            
            var showAnim = new MoveAnimation(_displayRectTransform, _showAnimParam);
            var hideAnim = new MoveAnimation(_displayRectTransform, _hideAnimParam);

            builder.RegisterInstance<IUIAnimation>(showAnim)
                .Keyed(AnimationType.Show);
            builder.RegisterInstance<IUIAnimation>(hideAnim)
                .Keyed(AnimationType.Hide);
            
            _animationPartGroup.BindAnimation(builder);
            
            base.Configure(builder);
        }
    }
}