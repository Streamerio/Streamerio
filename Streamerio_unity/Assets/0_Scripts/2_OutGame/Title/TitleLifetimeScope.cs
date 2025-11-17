using Common.Booster;
using Common.State;
using Common.UI.Animation;
using OutGame.Title.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace OutGame.Title
{
    public class TitleLifetimeScope: LifetimeScope
    {
        [SerializeField]
        private RectTransform _background;
        [SerializeField]
        private CloseAnimationParamSO _closeAnimationParam;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<IState, TitleStartState>(Lifetime.Singleton)
                .Keyed(StateType.TitleStart);
            builder.Register<IState, TitleState>(Lifetime.Singleton)
                .Keyed(StateType.Title);
            builder.Register<IState, MenuState>(Lifetime.Singleton)
                .Keyed(StateType.Menu);
            builder.Register<IState, TitleEndState>(Lifetime.Singleton)
                .Keyed(StateType.TitleEnd);
            builder.Register<IState, InGameLoadingState>(Lifetime.Singleton)
                .Keyed(StateType.InGameLoading);
            builder.Register<IState, ToInGameState>(Lifetime.Singleton)
                .Keyed(StateType.ToInGame);

            builder.Register<ITitleScreen, TitleScreenPresenter>(Lifetime.Singleton)
                .As<IStartable>();
            
            builder.RegisterInstance<IUIAnimation>(new CloseAnimation(_background, _closeAnimationParam))
                .Keyed(AnimationType.TitleBackground);

            SceneBoosterBinder.Bind(builder, StateType.TitleStart);
        }
    }
}