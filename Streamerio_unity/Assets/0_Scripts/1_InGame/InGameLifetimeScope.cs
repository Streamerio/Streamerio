using Common.Booster;
using Common.Scene;
using Common.State;
using Common.UI.Part.Button;
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
        private InGameSettingSO _inGameSetting;
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
            builder.Register<IState, ChangeSceneState>(Lifetime.Singleton)
                .WithParameter(_ => SceneType.ResultScene)
                .Keyed(StateType.ToResult);

            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Jump);
            builder.Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Attack);

            SceneBoosterBinder.Bind(builder, StateType.InGameStart);
        }
    }
}