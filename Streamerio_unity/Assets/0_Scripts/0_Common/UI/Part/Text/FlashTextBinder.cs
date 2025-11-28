using System;
using Common.UI.Animation;
using UnityEngine;
using VContainer;

namespace Common.UI.Part.Text
{
    [Serializable]
    public class FlashTextBinder
    {
        [SerializeField]
        private CanvasGroup _flashText;
        [SerializeField]
        private FlashAnimationParamSO _flashAnimationParamSO;

        public void Bind(IContainerBuilder builder)
        {
            builder.RegisterInstance<IUIAnimation>(new FlashAnimation(_flashText, _flashAnimationParamSO))
                .Keyed(AnimationType.FlashText);
        }
    }
}