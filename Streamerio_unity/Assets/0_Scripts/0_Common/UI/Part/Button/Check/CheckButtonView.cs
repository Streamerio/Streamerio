using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.Part.Button
{
    public class CheckButtonView: CommonButtonView
    {
        [SerializeField]
        private Image _image;

        [SerializeField]
        private Sprite _defaultSprite;
        [SerializeField]
        private Sprite _clickedSprite;

        public override async UniTask PlayPointerDownAsync(CancellationToken ct)
        {
            _image.sprite = _defaultSprite;
            await base.PlayPointerDownAsync(ct);
        }

        public override async UniTask PlayPointerClickAsync(CancellationToken ct)
        {
            _image.sprite = _clickedSprite;
            await UniTask.CompletedTask;
        }

        public override void ResetButtonState()
        {
            base.ResetButtonState();
            _image.sprite = _defaultSprite;
        }
    }
}