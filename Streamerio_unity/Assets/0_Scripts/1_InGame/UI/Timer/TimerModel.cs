using System.Threading;
using R3;
using UnityEngine;

namespace InGame.UI.Timer
{
    /// <summary>
    /// タイマー
    /// </summary>
    public class TimerModel: ITimerModel
    {
        private const float _countInterval = 1f;
        
        private ReactiveProperty<float> _valueProp;
        public ReadOnlyReactiveProperty<float> ValueProp => _valueProp;
        
        private CancellationTokenSource _cts;
        
        public TimerModel(float initialTime)
        {
            _valueProp = new (initialTime);
        }

        /// <summary>
        /// カウントダウンスタート
        /// </summary>
        public void StartCountdownTimer()
        {
            float time = 0f;
            StopCountdownTimer();
            _cts = new CancellationTokenSource();
            
            Observable.EveryUpdate()
                .Where(_ => _valueProp.Value > 0)
                .Subscribe(_ =>
                {
                    time += Time.deltaTime;

                    if (time > _countInterval)
                    {
                        _valueProp.Value -= _countInterval;
                        time -= _countInterval;
                    }
                }).RegisterTo(_cts.Token);
        }
        
        public void StopCountdownTimer()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
    
    public interface ITimerModel
    {
        ReadOnlyReactiveProperty<float> ValueProp { get; }
        void StartCountdownTimer();
        void StopCountdownTimer();
    }
}