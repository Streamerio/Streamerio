using Common.Booster;
using Common.Scene;
using Common.State;
using Common.UI.Animation;
using Common.UI.Part.Button;
using InGame.Goal;
using InGame.Setting;
using InGame.UI.Timer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame
{
    public class InGameLifetimeScope: LifetimeScope
    {
        [SerializeField]
        private HpPresenter _hpPresenter;
        [SerializeField]
        private InGameSettingSO _inGameSetting;
        
        [SerializeField]
        private Camera _mainCamera;
        [SerializeField]
        private ZoomAnimationParamSO _zoomAnimationParam;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterInstance<IInGameSetting>(_inGameSetting);
            
            builder.Register<ITimer, TimerPresenter>(Lifetime.Singleton)
                .As<IStartable>();
            
            builder.Register<IState, InGameStartState>(Lifetime.Singleton)
                .Keyed(StateType.InGameStart);
            builder.Register<IState, FirstPlayState>(Lifetime.Singleton)
                .Keyed(StateType.FirstPlay);
            builder.Register<IState, PlayFromTitleState>(Lifetime.Singleton)
                .Keyed(StateType.PlayFromTitle);
            builder.Register<IState, InGameState>(Lifetime.Singleton)
                .Keyed(StateType.InGame);
            builder.Register<IState, ChangeSceneState>(Lifetime.Singleton)
                .WithParameter(_ => SceneType.GameOverScene)
                .Keyed(StateType.ToGameOver);
            builder.Register<IState, ToResultState>(Lifetime.Singleton)
                .Keyed(StateType.ToResult);

            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Jump);
            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Attack);

            builder.Register<StickInput>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<Result>();
            builder.RegisterComponentInHierarchy<SkillRandomActivator>();
            builder.RegisterComponentInHierarchy<EnemyRandomActivator>();
            builder.RegisterInstance<HpPresenter>(_hpPresenter);

            builder.RegisterInstance<IUIAnimation>(new ZoomAnimation(_mainCamera, _zoomAnimationParam))
                .Keyed(AnimationType.InGameBackground);
            
            SceneBoosterBinder.Bind(builder, StateType.InGameStart);
        }
    }
}