using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame.Skill.UI.Cell
{
    public interface ISkillCellFactory
    {
        ISkillCell Create();
    }
    
    public class SkillCellFactory : ISkillCellFactory
    {
        private readonly IObjectResolver _resolver;
        private readonly SkillCellLifetimeScope _skillCellPrefab;
        private readonly Transform _parent;

        public SkillCellFactory(
            IObjectResolver resolver,
            SkillCellLifetimeScope skillCellPrefab,
            Transform parent)
        {
            _resolver = resolver;
            _skillCellPrefab = skillCellPrefab;
            _parent = parent;
        }

        public ISkillCell Create()
        {
            // 依存注入しつつPrefab生成
            var instance = _resolver.Instantiate(_skillCellPrefab, _parent);

            // ここではもうBuild済みなのでOK
            return instance.Container.Resolve<ISkillCell>();
        }
    }
}