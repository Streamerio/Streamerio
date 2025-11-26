using Common.State;
using VContainer.Unity;

namespace Common.Booster
{
    public class SceneBooster: IStartable
    {
        private readonly IStateManager _stateManager;
        private readonly IState _initialState;
        
        public SceneBooster(IStateManager stateManager, IState initialState)
        {
            _stateManager = stateManager;
            _initialState = initialState;
        }
        
        public void Start()
        {
            _stateManager.ChangeState(_initialState);
        }
    }
}