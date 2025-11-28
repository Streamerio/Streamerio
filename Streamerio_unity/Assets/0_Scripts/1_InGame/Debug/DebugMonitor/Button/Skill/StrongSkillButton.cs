using UnityEngine;
using UnityEngine.UI;

public class StrongSkillButton : MonoBehaviour
{
    [SerializeField] private SkillRandomActivator _skillRandomActivator; // SkillRandomActivatorの参照

    public void OnClick()
    {
        _skillRandomActivator.ActivateStrongSkill(new WebSocketManager.ViewerDetails("Debug Player"));
    }
}