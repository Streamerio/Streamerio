using UnityEngine;
using UnityEngine.UI;

public class WeakSkillButton : MonoBehaviour
{
    [SerializeField] private SkillRandomActivator _skillRandomActivator; // SkillRandomActivatorの参照

    public void OnClick()
    {
        _skillRandomActivator.ActivateWeakSkill(new WebSocketManager.ViewerDetails("Debug Player"));
    }
}