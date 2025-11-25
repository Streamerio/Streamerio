using System.Collections.Generic;
using System.Threading;
using Common;
using Cysharp.Threading.Tasks;
using InGame.Skill.UI.Animation;
using InGame.Skill.UI.Cell;
using InGame.Skill.UI.Icon;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace InGame.Skill.UI.Panel
{
    public class SkillPanelView: UIBehaviour, ISkillPanelView
    {
        [SerializeField]
        private int _maxCellCount = 3;
        [SerializeField]
        private SkillCellAnimationSO _cellAnimationParam;
        
        private Queue<ISkillCell> _cellQueue = new Queue<ISkillCell>();

        private ISkillCellFactory _skillCellFactory;
        private ISkillIconRepository _skillIconRepository;
        
        private  bool _isMaxCell => _cellQueue.Count > _maxCellCount;

        [Inject]
        public void Construct(ISkillCellFactory skillCellFactory, ISkillIconRepository skillIconRepository)
        {
            _skillCellFactory = skillCellFactory;
            _skillIconRepository = skillIconRepository;
        }
        
        public async UniTask ShowSkillAsync(MasterUltType skillType, string userName, CancellationToken ct)
        {
            var skillCell = _isMaxCell ?
                _cellQueue.Dequeue() :
                _skillCellFactory.Create();
            skillCell.Initialize(_skillIconRepository.GetSkillIconData(skillType), userName);
            _cellQueue.Enqueue(skillCell);
            
            var anim = new SkillCellAnimation(
                _cellQueue.ToArray(),
                _cellAnimationParam);
            await anim.PlayAsync(ct);
        }
    }

    public interface ISkillPanelView
    {
        UniTask ShowSkillAsync(MasterUltType skillType, string userName, CancellationToken ct);
    }

    public class SkillCellData
    {
        public MasterUltType SkillType;
        public string UserName;
    }
}