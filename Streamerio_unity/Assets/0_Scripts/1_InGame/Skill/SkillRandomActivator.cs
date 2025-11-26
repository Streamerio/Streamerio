using UnityEngine;
using R3;
using VContainer;

public class SkillRandomActivator : MonoBehaviour
{
    [SerializeField] private StrongSkillScriptableObject _strongSkillScriptableObject;
    [SerializeField] private MiddleSkillScriptableObject _middleSkillScriptableObject;
    [SerializeField] private WeakSkillScriptableObject _weakSkillScriptableObject;
    [SerializeField] private GameObject _parentObject;

    private IWebSocketManager _webSocketManager;

    [Inject]
    public void Construct(IWebSocketManager webSocketManager)
    {
        _webSocketManager = webSocketManager;
    }

    void Start()
    {
        Bind();
    }

    private void Bind()
    {
        _webSocketManager.FrontEventDict[FrontKey.skill3].Subscribe(_ => ActivateStrongSkill());
        _webSocketManager.FrontEventDict[FrontKey.skill2].Subscribe(_ => ActivateMiddleSkill());
        _webSocketManager.FrontEventDict[FrontKey.skill1].Subscribe(_ => ActivateWeakSkill());
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