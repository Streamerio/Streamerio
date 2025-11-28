using System.Threading;
using Common.UI.Display.Window;
using Common.UI.Display.Window.Book;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.State
{
    public class FirstPlayState: IState
    {
        private readonly IWindowService _windowService;
        
        [Inject]
        public FirstPlayState(IWindowService windowService)
        {
            _windowService = windowService;
        }
        
        public async UniTask EnterAsync(CancellationToken ct)
        {
            await _windowService.OpenDisplayAsync<IBookWindow>(ct);
        }
        
        public async UniTask ExitAsync(CancellationToken ct)
        {
            await _windowService.CloseTopAsync(ct);
        }
    }
}