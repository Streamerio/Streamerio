using Common.Booster;
using Common.Scene;
using Common.State;
using VContainer;
using VContainer.Unity;

namespace OutGame.Result
{
    public class ResultLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            builder.Register<IState, ResultState>(Lifetime.Singleton)
                .Keyed(StateType.Result);
            builder.Register<IState, ChangeSceneState>(Lifetime.Singleton)
                .WithParameter(_ => SceneType.Title)
                .Keyed(StateType.ToTitle);
            
            SceneBoosterBinder.Bind(builder, StateType.Result);
        }
    }
}