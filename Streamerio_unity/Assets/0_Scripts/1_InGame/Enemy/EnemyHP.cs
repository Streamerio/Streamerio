using System.Threading;
using Common.Audio;
using Cysharp.Threading.Tasks;
using R3;
using VContainer;

namespace InGame.Enemy
{
    public interface IEnemyHP : IDamageable
    {
        void Initialize(float initialHealth);
        ReadOnlyReactiveProperty<bool> IsDeadProp { get; }
    }

    public class EnemyHP: IEnemyHP
    {
        private float _health;
        public float CurrentHealth => _health;
        private ReactiveProperty<bool> _isDeadProp = new (false);
        public ReadOnlyReactiveProperty<bool> IsDeadProp => _isDeadProp;

        private IAudioFacade _audioFacade;
    
        private CancellationTokenSource _cts = new ();
    
        [Inject]
        public void Construct(IAudioFacade audioFacade)
        {
            _audioFacade = audioFacade;
        }
    
        public void Initialize(float initialHealth)
        {
            _health = initialHealth;
            _isDeadProp.Value = false;
        }

        public void TakeDamage(float amount)
        {
            if (IsDeadProp.CurrentValue) return;
        
            _health -= amount;
        
            if (_health <= 0)
            {
                _health = 0;
                Die();
            }
            else
            {
                _audioFacade.PlayAsync(SEType.どん_効果音,_cts.Token).Forget();
            }
        }

        protected virtual void Die()
        {
            _audioFacade.PlayAsync(SEType.敵のダウン,_cts.Token).Forget();
            _isDeadProp.Value = true;
        }
    }   
}