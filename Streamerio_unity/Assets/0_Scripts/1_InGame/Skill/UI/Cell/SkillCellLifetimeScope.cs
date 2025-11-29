using InGame.Skill.UI.Icon;
using TMPro;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Skill.UI.Cell
{
    public class SkillCellLifetimeScope: LifetimeScope
    {
        [SerializeField]
        private RectTransform _rectTransform;
        [SerializeField]
        private TMP_Text _userText;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            
            builder.Register<ISkillCell, SkillCellView>(Lifetime.Singleton)
                .WithParameter(_rectTransform)
                .WithParameter(GetComponentInChildren<ISkillIcon>())
                .WithParameter(_userText);
        }
    }
}