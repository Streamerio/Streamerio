using VContainer;

namespace Common.Audio
{
    public class AudioManager: SingletonBase<AudioManager>
    {
        public IAudioFacade AudioFacade { get; private set; }
        [Inject]
        public void Construct(IAudioFacade audioFacade)
        {
            AudioFacade = audioFacade;
        }
    }
}