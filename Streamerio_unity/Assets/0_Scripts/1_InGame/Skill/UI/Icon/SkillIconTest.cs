using Common;
using UnityEngine;

namespace InGame.Skill.UI.Icon
{
    public class SkillIconTest: MonoBehaviour
    {
        [SerializeField]
        private MasterUltType _masterUltType;
        [SerializeField]
        private SkillIconDataSO _skillIconDataSO;
        [SerializeField]
        private SkillIconView _skillIconView;


        public void OnClick()
        {
            _skillIconView.SetIcon(_skillIconDataSO.GetSkillIconData(_masterUltType));
        }
    }
}