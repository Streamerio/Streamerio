using Common;
using InGame.Skill.Spawner;
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
    private IMasterData _masterData;
    private ISkillPanel _skillPanel;
    private ISkillSpawner _skillSpawner;

    [Inject]
    public void Construct(IWebSocketManager webSocketManager, IMasterData masterData, ISkillPanel skillPanel, ISkillSpawner skillSpawner)
    {
        _webSocketManager = webSocketManager;
        _masterData = masterData;
        _skillPanel = skillPanel;
        _skillSpawner = skillSpawner;
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
        var skills = _masterData.UltRarityDictionary[MasterUltRarityType.Strong];
        int randomIndex = Random.Range(0, skills.Count);
        _skillSpawner.Spawn(skills[randomIndex]);
        
        _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(skills[randomIndex], viewerDetails.ViewerName));
        Debug.Log("Strong Skill Spawned");
    }
    public void ActivateMiddleSkill(WebSocketManager.ViewerDetails viewerDetails)
    {
        var skills = _masterData.UltRarityDictionary[MasterUltRarityType.Normal];
        int randomIndex = Random.Range(0, skills.Count);
        _skillSpawner.Spawn(skills[randomIndex]);
        
        _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(skills[randomIndex], viewerDetails.ViewerName));
        Debug.Log("Middle Skill Spawned");
    }
    public void ActivateWeakSkill(WebSocketManager.ViewerDetails viewerDetails)
    {
        var skills = _masterData.UltRarityDictionary[MasterUltRarityType.Weak];
        int randomIndex = Random.Range(0, skills.Count);
        _skillSpawner.Spawn(skills[randomIndex]);
        
        _skillPanel.OnActiveSkillSubject.OnNext(new SkillCellData(skills[randomIndex], viewerDetails.ViewerName));
        Debug.Log("Weak Skill Spawned");
    }
}