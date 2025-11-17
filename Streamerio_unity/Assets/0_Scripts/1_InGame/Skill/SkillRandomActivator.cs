using UnityEngine;
using R3;

public class SkillRandomActivator : MonoBehaviour
{
    [SerializeField] private StrongSkillScriptableObject _strongSkillScriptableObject;
    [SerializeField] private MiddleSkillScriptableObject _middleSkillScriptableObject;
    [SerializeField] private WeakSkillScriptableObject _weakSkillScriptableObject;
    [SerializeField] private GameObject _parentObject;

    void Start()
    {
        Bind();
    }

    private void Bind()
    {
        WebsocketManager.Instance.FrontEventDict[FrontKey.skill3].Subscribe(_ => ActivateStrongSkill());
        WebsocketManager.Instance.FrontEventDict[FrontKey.skill2].Subscribe(_ => ActivateMiddleSkill());
        WebsocketManager.Instance.FrontEventDict[FrontKey.skill1].Subscribe(_ => ActivateWeakSkill());
    }
    public void ActivateStrongSkill()
    {
        int randomIndex = Random.Range(0, _strongSkillScriptableObject.Skills.Length);
        Instantiate(_strongSkillScriptableObject.Skills[randomIndex], _parentObject.transform);
        Debug.Log("Strong Skill Spawned");
    }
    public void ActivateMiddleSkill()
    {
        int randomIndex = Random.Range(0, _middleSkillScriptableObject.Skills.Length);
        Instantiate(_middleSkillScriptableObject.Skills[randomIndex], _parentObject.transform);
        Debug.Log("Middle Skill Spawned");
    }
    public void ActivateWeakSkill()
    {
        int randomIndex = Random.Range(0, _weakSkillScriptableObject.Skills.Length);
        Instantiate(_weakSkillScriptableObject.Skills[randomIndex], _parentObject.transform);
        Debug.Log("Weak Skill Spawned");
    }
}