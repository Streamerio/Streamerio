using System.Threading;
using Common.UI.Animation;
using Common.UI.Display;
using Cysharp.Threading.Tasks;
using VContainer;

namespace OutGame.Title.UI
{
    public class TitleScreenView: DisplayViewBase, ITitleScreenView
    {
        private IUIAnimation _clickTextAnimation;

        private IUIAnimation _showAnimation;
        private IUIAnimation _hideAnimation;

        [Inject]
        public void Contruct([Key(AnimationType.FlashText)] IUIAnimation clickTextAnimation,
            [Key(AnimationType.Show)] IUIAnimation showAnimation,
            [Key(AnimationType.Hide)] IUIAnimation hideAnimation)
        {
            _clickTextAnimation = clickTextAnimation;
            
            _showAnimation = showAnimation;
            _hideAnimation = hideAnimation;
        }
        
        public override async UniTask ShowAsync(CancellationToken ct)
        {
            await _showAnimation.PlayAsync(ct);
            _clickTextAnimation.PlayAsync(destroyCancellationToken).Forget();
        }
        
        public override void Show()
        {
            _showAnimation.PlayImmediate();
            _clickTextAnimation.PlayAsync(destroyCancellationToken).Forget();
        }
        
        public override async UniTask HideAsync(CancellationToken ct)
        {
            await _hideAnimation.PlayAsync(ct);
            _clickTextAnimation.Skip();
        }
        
        public override void Hide()
        {
            _hideAnimation.PlayImmediate();
            _clickTextAnimation.Skip();
        }
    }
    
    public interface ITitleScreenView : IDisplayView
    {
    }
}