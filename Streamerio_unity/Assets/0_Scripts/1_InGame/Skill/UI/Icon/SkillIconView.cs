using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InGame.Skill.UI.Icon
{
    public class SkillIconView: UIBehaviour, ISkillIcon
    {
        [SerializeField, Tooltip("アイコン画像")]
        private Image _iconImage;
        [SerializeField, Tooltip("アイコンRectTransform")]
        private RectTransform _iconRectTransform;
        [SerializeField, Tooltip("アイコンサイズ")]
        private Vector2 _iconSize;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if(_iconImage != null)
            {
                _iconRectTransform ??= _iconImage.GetComponent<RectTransform>();
            }

            if (_iconRectTransform != null)
            {
                 _iconSize = _iconRectTransform.sizeDelta;
            }
        }
#endif
        
        public void SetIcon(SkillIconData iconData)
        {
            _iconImage.sprite = iconData.Icon;
            _iconImage.color = iconData.Color;
            
            var verticalInversion = iconData.IsInversionVertical ? -180f : 0f;
            var horizontalInversion = iconData.IsInversionHorizontal ? -180f : 0;
            _iconRectTransform.localEulerAngles = new Vector3(verticalInversion, horizontalInversion, iconData.Rotation);

            bool isVerticalMultiply = iconData.AspectRatio.x > iconData.AspectRatio.y;
            float multiplier = isVerticalMultiply
                ? iconData.AspectRatio.y / iconData.AspectRatio.x
                : iconData.AspectRatio.x / iconData.AspectRatio.y;
            var scale = isVerticalMultiply
                ? new Vector2(_iconSize.x, _iconSize.y*multiplier)
                : new Vector2(_iconSize.x*multiplier, _iconSize.y);
            _iconRectTransform.sizeDelta = scale;
        }
    }
    
    public interface ISkillIcon
    {
        void SetIcon(SkillIconData iconData);
    }
}