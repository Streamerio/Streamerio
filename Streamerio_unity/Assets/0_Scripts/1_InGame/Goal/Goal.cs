using Common.State;
using UnityEngine;
using InGame;
using VContainer;

public class Goal : MonoBehaviour
{
    private IState _gameClearState;
    private IStateManager _stateManager;
    
    [Inject]
    public void Construct([Key(StateType.ToResult)] IState resultState, IStateManager stateManager)
    {
        _gameClearState = resultState;
        _stateManager = stateManager;
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("ゴール");
            InGame.Goal.Result.Instance.OnGoal();
            //_stateManager.ChangeState(_gameClearState);
        }
    }
}