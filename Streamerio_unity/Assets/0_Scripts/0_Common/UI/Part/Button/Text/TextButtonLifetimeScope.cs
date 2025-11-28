using Alchemy.Inspector;
using Common.UI.Animation;
using UnityEngine;
using VContainer;

namespace Common.UI.Part.Button
{
    public class TextButtonLifetimeScope: ButtonLifetimeScopeBase
    {
        [SerializeField, ReadOnly]
        private RectTransform _rectTransform;
        
        [SerializeField]
        private ScaleAnimationParamSO _pushDownAnimParam;
        [SerializeField]
        private ScaleAnimationParamSO _pushUpAnimParam;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            _rectTransform ??= GetComponent<RectTransform>();
        }
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            UIAnimationBinder.Bind(builder, new ScaleAnimation(_rectTransform, _pushDownAnimParam), AnimationType.PushDown);
            UIAnimationBinder.Bind(builder, new ScaleAnimation(_rectTransform, _pushUpAnimParam), AnimationType.PushUp);
            
            base.Configure(builder);
        }
    }
}