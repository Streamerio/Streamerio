using Common;
using Common.State;
using VContainer;

namespace InGame.Goal
{
    public class Result: SingletonBase<Result>
    {
        private IState _gameClearState;
        private IStateManager _stateManager;
    
        [Inject]
        public void Construct([Key(StateType.ToResult)] IState resultState, IStateManager stateManager)
        {
            _gameClearState = resultState;
            _stateManager = stateManager;
        }
        
        public void OnGoal()
        {
            _stateManager.ChangeState(_gameClearState);
        }
    }
}