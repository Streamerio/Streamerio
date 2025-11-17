using Common.Audio;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using VContainer;

public class PlayerGrounded : MonoBehaviour
{
    private ReactiveProperty<bool> _isGrounded = new ReactiveProperty<bool>(true);
    /// <summary>
    /// 地面についているかどうか
    /// </summary>
    public ReadOnlyReactiveProperty<bool> IsGrounded => _isGrounded;
    
    private IAudioFacade _audioFacade;
    
    [Inject]
    public void Construct(IAudioFacade audioFacade)
    {
        _audioFacade = audioFacade;
    }
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        _isGrounded.Value = true;
    }
    
    public void OnTriggerStay2D(Collider2D collision)
    {
        _isGrounded.Value = true;
    }
    
    public void OnTriggerExit2D(Collider2D collision)
    {
        AudioManager.Instance.AudioFacade.PlayAsync(SEType.PlayerJump, destroyCancellationToken).Forget();
        _isGrounded.Value = false;
    }
}
