using Common.State;
using VContainer;
using VContainer.Unity;

namespace Common.Booster
{
    public static class SceneBoosterBinder
    {
        public static void Bind(IContainerBuilder builder, StateType initialStateType)
        {
            builder.RegisterEntryPoint<SceneBooster>()
                .WithParameter(resolver => resolver.Resolve<IStateManager>())
                .WithParameter(resolver => resolver.Resolve<IState>(initialStateType));
        }
    }
}