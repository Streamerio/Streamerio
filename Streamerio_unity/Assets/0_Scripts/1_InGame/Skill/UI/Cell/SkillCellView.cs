using Common.UI;
using InGame.Skill.UI.Icon;
using TMPro;
using UnityEngine;

namespace InGame.Skill.UI.Cell
{
    public class SkillCellView: ISkillCell
    {
        private readonly RectTransform _rectTransform;
        public RectTransform RectTransform => _rectTransform;
        
        private readonly ISkillIcon _skillIcon;
        private readonly TMP_Text _userText;

        public SkillCellView(RectTransform rectTransform, ISkillIcon skillIcon, TMP_Text userText)
        {
            _rectTransform = rectTransform;
            
            _skillIcon = skillIcon;
            _userText = userText;
        }
        
        public void Initialize(SkillIconData iconData, string userName)
        {
            _skillIcon.SetIcon(iconData);
            _userText.SetText(userName);
        }
    }
    
    public interface ISkillCell
    {
        RectTransform RectTransform { get; }
        void Initialize(SkillIconData iconData, string userName);
    }
}