using System;
using Common.UI.Animation;
using UnityEngine;
using VContainer;

namespace Common.UI.Part
{
    [Serializable]
    public class AnimationPartGroup
    {
        [SerializeField]
        private CanvasGroup[] _animationParts;
        [SerializeField]
        private FadePartsAnimationParamSO _showPartsAnimationParam;
        [SerializeField]
        private FadePartsAnimationParamSO _hidePartsAnimationParam;

        public void BindAnimation(IContainerBuilder builder)
        {
            builder.RegisterInstance<IUIAnimation>(new FadePartsAnimation(_animationParts, _showPartsAnimationParam))
                .Keyed(AnimationType.ShowParts);
            builder.RegisterInstance<IUIAnimation>(new FadePartsAnimation(_animationParts, _hidePartsAnimationParam))
                .Keyed(AnimationType.HideParts);
        }
    }
}