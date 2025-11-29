using System.Threading;
using Common.UI.Animation;
using Common.UI.Part.Button;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Common.UI.Display.Window.Book
{
    public class BookWindowView: WindowViewBase, IBookWindowView
    {
        private ICommonButton _closeButton;
        public ICommonButton CloseButton => _closeButton;
        
        private ICommonButton _nextButton;
        public ICommonButton NextButton => _nextButton;

        private ICommonButton _backButton;
        public ICommonButton BackButton => _backButton;
        
        private IUIAnimation _bookTurnLeftAnimation;
        private IUIAnimation _bookTurnRightAnimation;

        [Inject]
        public void Construct(
            [Key(ButtonType.Close)] ICommonButton closeButton,
            [Key(ButtonType.NextPage)] ICommonButton nextButton,
            [Key(ButtonType.BackPage)] ICommonButton backButton,
            [Key(AnimationType.TurnLeft)] IUIAnimation bookTurnLeftAnimation,
            [Key(AnimationType.TurnRight)] IUIAnimation bookTurnRightAnimation)
        {
            _closeButton = closeButton;
            _nextButton = nextButton;
            _backButton = backButton;
            
            _bookTurnLeftAnimation = bookTurnLeftAnimation;
            _bookTurnRightAnimation = bookTurnRightAnimation;
        }
        
        public async UniTask PlayTurnLeftAsync(CancellationToken ct)
        {
            await _bookTurnLeftAnimation.PlayAsync(ct);
        }
        
        public async UniTask PlayTurnRightAsync(CancellationToken ct)
        {
            await _bookTurnRightAnimation.PlayAsync(ct);
        }
    }
    
    public interface IBookWindowView : IWindowView
    {
        ICommonButton CloseButton { get; }
        ICommonButton NextButton { get; }
        ICommonButton BackButton { get; }
        
        UniTask PlayTurnLeftAsync(CancellationToken ct);
        UniTask PlayTurnRightAsync(CancellationToken ct);
    }
}