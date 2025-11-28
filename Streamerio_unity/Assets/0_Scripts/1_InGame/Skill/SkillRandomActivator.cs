using Common;
using InGame.Skill.UI.Panel;
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
    private ISkillPanel _skillPanel;

    [Inject]
    public void Construct(IWebSocketManager webSocketManager, ISkillPanel skillPanel)
    {
        _webSocketManager = webSocketManager;
        _skillPanel = skillPanel;
    }

    void Start()
    {
        Bind();
    }

    private void Bind()
    {
        _webSocketManager.UltEventViewerNameDict[FrontKey.skill3].Subscribe(value => ActivateStrongSkill(value));
        _webSocketManager.UltEventViewerNameDict[FrontKey.skill2].Subscribe(value => ActivateMiddleSkill(value));
        _webSocketManager.UltEventViewerNameDict[FrontKey.skill1].Subscribe(value => ActivateWeakSkill(value));
    }
    public void ActivateStrongSkill(WebSocketManager.ViewerDetails viewerDetails)
    {
        int randomIndex = Random.Range(0, _strongSkillScriptableObject.Skills.Length);
        Instantiate(_strongSkillScriptableObject.Skills[randomIndex], _parentObject.transform);
        _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(MasterUltType.Thunder, viewerDetails.ViewerName));
        Debug.Log("Strong Skill Spawned");
    }
    public void ActivateMiddleSkill(WebSocketManager.ViewerDetails viewerDetails)
    {
        int randomIndex = Random.Range(0, _middleSkillScriptableObject.Skills.Length);
        Instantiate(_middleSkillScriptableObject.Skills[randomIndex], _parentObject.transform);
        _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(MasterUltType.Bullet, viewerDetails.ViewerName));
        Debug.Log("Middle Skill Spawned");
    }
    public void ActivateWeakSkill(WebSocketManager.ViewerDetails viewerDetails)
    {
        int randomIndex = Random.Range(0, _weakSkillScriptableObject.Skills.Length);
        Instantiate(_weakSkillScriptableObject.Skills[randomIndex], _parentObject.transform);
        _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(MasterUltType.Beam, viewerDetails.ViewerName));
        Debug.Log("Weak Skill Spawned");
    }
}