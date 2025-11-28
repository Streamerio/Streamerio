using Common.UI.Display.Window.Book.Page;
using Common.UI.Part.Button;
using VContainer;

namespace OutGame.Book.Menu
{
    public interface IMenuPagePanelView : IPagePanelView
    {
        ICommonButton StartButton { get; }
        ICommonButton HowToPlayButton { get; }
        ICommonButton OptionButton { get; }
        ICommonButton CreditButton { get; }
    }
    /// <summary>
    /// メニューパネルの View（見た目担当）。
    /// - スタート / 遊び方 / オプション / クレジット 各ボタンを保持
    /// - 各ボタンの Initialize を呼び出して、共通の初期化処理を適用する
    /// - 実際のボタン押下時の処理は Presenter 側で制御される
    /// </summary>
    public class MenuPagePanelView : CommonPagePanelView, IMenuPagePanelView
    {
        private ICommonButton _startButton;
        public ICommonButton StartButton => _startButton;
        
        private ICommonButton _howToPlayButton;
        public ICommonButton HowToPlayButton => _howToPlayButton;

        private ICommonButton _optionButton;
        public ICommonButton OptionButton => _optionButton;

        private ICommonButton _creditButton;
        public ICommonButton CreditButton => _creditButton;

        [Inject]
        public void Construct([Key(ButtonType.GameStart)]ICommonButton starButton, 
            [Key(ButtonType.HowToPlay)]ICommonButton howToPlayButton,
            [Key(ButtonType.Option)]ICommonButton optionButton,
            [Key(ButtonType.Credit)]ICommonButton creditButton)
        {
            _startButton = starButton;
            _howToPlayButton = howToPlayButton;
            _optionButton = optionButton;
            _creditButton = creditButton;
        }
    }
}