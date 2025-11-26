using Alchemy.Inspector;
using Common;
using Common.Audio;
using Common.State;
using Common.UI.Animation;
using Common.UI.Click;
using Common.UI.Display;
using Common.UI.Part.Text;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;
using VContainer.Unity;

namespace OutGame.Title.UI
{
    [RequireComponent(typeof(ObservableEventTrigger))]
    public class TitleScreenLifetimeScope: DisplayLifetimeScopeBase<ITitleScreen, TitleScreenPresenter, ITitleScreenView, TitleScreenContext>
    {
        [SerializeField, ReadOnly]
        [Tooltip("PointerEventData を流す ObservableEventTrigger。OnValidate で自動補完される。")]
        private ObservableEventTrigger _clickTrigger;
        
        [SerializeField]
        private SEType _clickSE = SEType.NESRPG0112;

        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private FadeAnimationParamSO _showAnimationParam;
        [SerializeField]
        private FadeAnimationParamSO _hideAnimationParam;
        
        [SerializeField]
        private FlashTextBinder _clickTextBinder;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _canvasGroup ??= GetComponent<CanvasGroup>();
            _clickTrigger ??= GetComponent<ObservableEventTrigger>();
        }
#endif
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance<IUIAnimation>(new FadeAnimation(_canvasGroup, _showAnimationParam))
                .Keyed(AnimationType.Show);
            builder.RegisterInstance<IUIAnimation>(new FadeAnimation(_canvasGroup, _hideAnimationParam))
                .Keyed(AnimationType.Hide);
            
            _clickTextBinder.Bind(builder);
            
            base.Configure(builder);
        }

        protected override void BindPresenter(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<Wiring<ITitleScreen, TitleScreenContext>>()
                .WithParameter(CreateContext);
        }

        protected override TitleScreenContext CreateContext(IObjectResolver resolver)
        {
            var audioFacade = resolver.Resolve<IAudioFacade>();
            var binder = new ClickEventBinder<PointerEventData>(
                _clickTrigger.OnPointerClickAsObservable(),
                audioFacade,
                _clickSE
            );
            
            return new TitleScreenContext()
            {
                View = resolver.Resolve<ITitleScreenView>(),
                ClickEventBinder = binder,
                StateManager = resolver.Resolve<IStateManager>(),
                NextState = resolver.Resolve<IState>(StateType.Menu)
            };
        }
    }
}