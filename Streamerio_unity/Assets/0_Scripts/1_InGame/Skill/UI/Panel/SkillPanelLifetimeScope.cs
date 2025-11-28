using Common;
using InGame.Skill.UI.Cell;
using InGame.Skill.UI.Icon;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Skill.UI.Panel
{
    public class SkillPanelLifetimeScope: LifetimeScope
    {
        [SerializeField]
        private SkillIconDataSO _skillIconData;
        [SerializeField]
        private SkillCellLifetimeScope _skillCellViewPrefab;
        [SerializeField]
        private Transform _skillCellParent;
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterInstance<ISkillIconRepository>(_skillIconData);
            
            builder.RegisterComponent(GetComponent<ISkillPanelView>());
            
            builder.Register<ISkillCellFactory, SkillCellFactory>(Lifetime.Singleton)
                .WithParameter(resolver => resolver)
                .WithParameter(_skillCellViewPrefab)
                .WithParameter(_skillCellParent);

            builder.RegisterEntryPoint<Wiring<ISkillPanel, SkillPanelContext>>()
                .WithParameter(resolver => resolver.Resolve<ISkillPanel>())
                .WithParameter(resolver => new SkillPanelContext()
                {
                    View = resolver.Resolve<ISkillPanelView>(),
                });
            
            //builder.RegisterComponent(GetComponent<SkillPanelTest>());
        }
    }
}