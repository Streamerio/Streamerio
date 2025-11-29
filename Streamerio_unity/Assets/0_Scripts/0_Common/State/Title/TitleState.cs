using System.Threading;
using Cysharp.Threading.Tasks;
using OutGame.Title.UI;
using VContainer;

namespace Common.State
{
    public class TitleState: IState
    {
        private readonly ITitleScreen _titleScreen;

        [Inject]
        public TitleState(ITitleScreen titleScreen)
        {
            _titleScreen = titleScreen;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _titleScreen.ShowAsync(ct);
        }

        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _titleScreen.HideAsync(ct);
        }
    }
}