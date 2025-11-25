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
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData()
            {
                SkillType = MasterUltType.Beam,
                UserName = "Player A"
            });
        }
        
        public void ShowBullet()
        {
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData()
            {
                SkillType = MasterUltType.Bullet,
                UserName = "Player B"
            });
        }
        
        public void ShowChargeBeam()
        {
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData()
            {
                SkillType = MasterUltType.ChargeBeam,
                UserName = "Player V"
            });
        }
        
        public void ShowThunder()
        {
            _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData()
            {
                SkillType = MasterUltType.Thunder,
                UserName = "Player D"
            });
        }
    }
}