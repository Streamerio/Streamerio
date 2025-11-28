using Common.Audio;
using Common.UI.Display.Window.Book.Page;
using VContainer;

namespace OutGame.Book.Option
{
    public class SoundPageLifeTimeScope: PageLifetimeScopeBase<ISoundPagePanel, SoundPagePanelPresenter, ISoundPagePanelView, SoundPagePanelContext>
    {
        protected override SoundPagePanelContext CreateContext(IObjectResolver resolver)
        {
            return new SoundPagePanelContext()
            {
                View = resolver.Resolve<ISoundPagePanelView>(),
                AudioFacade = resolver.Resolve<IAudioFacade>()
            };
        }
    }
}