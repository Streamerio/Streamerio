using Common;
using Common.State;
using InGame.Setting;
using VContainer;
using VContainer.Unity;

namespace InGame.UI.Timer
{
    public class TimerLifetimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<ITimerModel, TimerModel>(Lifetime.Singleton)
                .WithParameter(resolver => resolver.Resolve<IMasterData>().GameSetting.TimeLimit);
            
            var view = GetComponent<ITimerView>();
            builder.RegisterComponent(view);

            builder.RegisterEntryPoint<Wiring<ITimer, TimerContext>>()
                .WithParameter(resolver =>
                {
                    return new TimerContext()
                    {
                        Model = resolver.Resolve<ITimerModel>(),
                        View = view,
                        StateManager = resolver.Resolve<IStateManager>(),
                        GameOverState = resolver.Resolve<IState>(StateType.ToGameOver),
                    };
                });
        }
    }
}