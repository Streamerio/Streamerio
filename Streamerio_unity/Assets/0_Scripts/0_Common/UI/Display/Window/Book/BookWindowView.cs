using Common.UI.Part.Button;
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

        [Inject]
        public void Construct(
            [Key(ButtonType.Close)] ICommonButton closeButton,
            [Key(ButtonType.NextPage)] ICommonButton nextButton,
            [Key(ButtonType.BackPage)] ICommonButton backButton)
        {
            _closeButton = closeButton;
            _nextButton = nextButton;
            _backButton = backButton;
        }
    }
    
    public interface IBookWindowView : IWindowView
    {
        ICommonButton CloseButton { get; }
        ICommonButton NextButton { get; }
        ICommonButton BackButton { get; }
    }
}