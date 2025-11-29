using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Common.UI.Display.Window.Book
{
    public class BookTurnAnimation: SequenceAnimationBase
    {
        private readonly Image _bookImage;
        private readonly RectTransform _pageRectTransform;
        private readonly BookTurnAnimationParamSO _param;
        
        public BookTurnAnimation(Image bookImage, BookTurnAnimationParamSO param)
        {
            _bookImage = bookImage;
            _pageRectTransform = bookImage.GetComponent<RectTransform>();
            _param = param;

            SetSequence();
        }

        public override void PlayImmediate()
        {
            ApplyParam(_param.InitialBookParam);
        }

        public override async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                ApplyParam(_param.InitialBookParam);    
            }
            
            await base.PlayAsync(ct, useInitial);
        }

        private void SetSequence()
        {
            var length = _param.BookParams.Length;
            if(length == 0) return;
            
            var interval = _param.DurationSec/length;

            for (int i = 0; i < length-1; i++)
            {
                int index = i;
                Sequence.AppendCallback(() => ApplyParam(_param.BookParams[index]))
                    .AppendInterval(interval);
            }
            
            Sequence.AppendCallback(() => ApplyParam(_param.BookParams[length - 1]));
        }
        
        private void ApplyParam(BookParam param)
        {
            _bookImage.sprite = param.page;
            _pageRectTransform.anchoredPosition = new Vector2(_pageRectTransform.anchoredPosition.x, param.posY);
            _pageRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, param.height);
        }
    }
}