using VContainer;

namespace Common.UI.Animation
{
    public static class UIAnimationBinder
    {
        public static void Bind(IContainerBuilder builder, IUIAnimation animation)
        {
            builder.RegisterInstance(animation);
        }
        
        public static void Bind(IContainerBuilder builder, IUIAnimation animation, AnimationType type)
        {
            builder.RegisterInstance(animation)
                .Keyed(type);
        }
    }
}