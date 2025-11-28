using Common;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace InGame.Skill.UI.Panel
{
    public class SkillPanelTest: MonoBehaviour
    {
        private ISkillPanel _skillPanel;

        [Inject]
        public void Construct(ISkillPanel skillPanel)
        {
            _skillPanel = skillPanel;
        }

        public void ShowBeam()
        {
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(MasterUltType.Beam, "Player A"));
        }
        
        public void ShowBullet()
        {
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(MasterUltType.Bullet, "Player B"));
        }
        
        public void ShowChargeBeam()
        {
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(MasterUltType.ChargeBeam, "Player C"));
        }
        
        public void ShowThunder()
        {
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(MasterUltType.Thunder, "Player D"));
        }
    }
}