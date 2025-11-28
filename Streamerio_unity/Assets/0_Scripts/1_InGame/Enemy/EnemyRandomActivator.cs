using Common.Audio;
using UnityEngine;
using R3;
using VContainer;

public class EnemyRandomActivator : MonoBehaviour
{
    [SerializeField] private StrongEnemyScriptableObject _strongEnemyScriptableObject;
    [SerializeField] private MiddleEnemyScriptableObject _middleEnemyScriptableObject;
    [SerializeField] private WeakEnemyScriptableObject _weakEnemyScriptableObject;
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
        _webSocketManager.FrontEventDict[FrontKey.enemy3].Subscribe(_ => ActivateStrongEnemy());
        _webSocketManager.FrontEventDict[FrontKey.enemy2].Subscribe(_ => ActivateMiddleEnemy());
        _webSocketManager.FrontEventDict[FrontKey.enemy1].Subscribe(_ => ActivateWeakEnemy());        
    }
    public void ActivateStrongEnemy()
    {
        int randomIndex = Random.Range(0, _strongEnemyScriptableObject.Enemys.Length);
        Instantiate(_strongEnemyScriptableObject.Enemys[randomIndex], _parentObject.transform);
        Debug.Log("Strong Enemy Spawned");
    }
    public void ActivateMiddleEnemy()
    {
        int randomIndex = Random.Range(0, _middleEnemyScriptableObject.Enemys.Length);
        Instantiate(_middleEnemyScriptableObject.Enemys[randomIndex], _parentObject.transform);
        Debug.Log("Middle Enemy Spawned");
    }
    public void ActivateWeakEnemy()
    {
        int randomIndex = Random.Range(0, _weakEnemyScriptableObject.Enemys.Length);
        Instantiate(_weakEnemyScriptableObject.Enemys[randomIndex], _parentObject.transform);
        Debug.Log("Weak Enemy Spawned");
    }
}