using System.Threading;
using Common;
using Common.State;
using R3;
using VContainer.Unity;

namespace InGame.UI.Timer
{
    public interface ITimer: IAttachable<TimerContext>
    {
        void StartCountdownTimer();
        void StopCountdownTimer();
    }
    
    public class TimerPresenter: ITimer, IStartable
    {
        private ITimerModel _model;
        private ITimerView _view;
        
        private IStateManager _stateManager;
        private IState _gameOverState;

        private CancellationTokenSource _cts;
        
        public void Attach(TimerContext context)
        {
            _model = context.Model;
            _view = context.View;
            _stateManager = context.StateManager;
            _gameOverState = context.GameOverState;
            
            _cts = new CancellationTokenSource();
            
            _view.ZeroSetting(_model.ValueProp.CurrentValue);
        }
        
        public void Start()
        {
            Bind();
        }
        
        public void Detach()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
        
        private void Bind()
        {
            _model.ValueProp
                .DistinctUntilChanged()
                .Subscribe(value =>
                {
                    _view.UpdateTimerView(value);
                }).RegisterTo(_cts.Token);
            
            _model.ValueProp
                .Where(value => value <= 0)
                .Subscribe(_ =>
                {
                    _stateManager.ChangeState(_gameOverState);
                }).RegisterTo(_cts.Token);
        }

        /// <summary>
        /// カウントダウンタイマー開始
        /// </summary>
        public void StartCountdownTimer()
        {
            _model.StartCountdownTimer();
        }
        
        public void StopCountdownTimer()
        {
            _model.StopCountdownTimer();
        }
    }

    public class TimerContext
    {
        public ITimerModel Model;
        public ITimerView View;
        public IStateManager StateManager;
        public IState GameOverState;
    }
}