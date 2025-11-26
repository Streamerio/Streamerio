using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Common.State
{
    public interface IState
    {
        UniTask EnterAsync(CancellationToken ct);
        UniTask ExitAsync(CancellationToken ct);
    }
    
    public interface IStateManager: IDisposable
    {
        UniTaskVoid ChangeState(IState newState);
    }
    
    public class StateManager: IStateManager
    {
        private IState _currentState;

        private CancellationTokenSource _cts;
        
        public StateManager()
        {
            _cts = new CancellationTokenSource();
        }
        
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }

        public async UniTaskVoid ChangeState(IState newState)
        {
            if (_currentState != null)
            {
                await _currentState.ExitAsync(_cts.Token);   
            }
            
            _currentState = newState;
            await _currentState.EnterAsync(_cts.Token);
        }
    }
}