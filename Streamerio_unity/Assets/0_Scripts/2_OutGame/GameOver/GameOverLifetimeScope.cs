using Common.Booster;
using Common.Scene;
using Common.State;
using VContainer;
using VContainer.Unity;

namespace OutGame.GameOver
{
    public class GameOverLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IState, GameOverState>(Lifetime.Singleton)
                .Keyed(StateType.GameOver);
            builder.Register<IState, RestartState>(Lifetime.Singleton)
                .WithParameter(_ => SceneType.GameScene)
                .Keyed(StateType.Restart);
            builder.Register<IState, ChangeSceneState>(Lifetime.Singleton)
                .WithParameter(_ => SceneType.Title)
                .Keyed(StateType.ToTitle);
            
            SceneBoosterBinder.Bind(builder, StateType.GameOver);
        }
    }
}