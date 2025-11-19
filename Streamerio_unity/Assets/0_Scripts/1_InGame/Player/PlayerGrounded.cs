using Common.Audio;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class PlayerGrounded : MonoBehaviour
{
    private ReactiveProperty<bool> _isGrounded = new ReactiveProperty<bool>(true);
    /// <summary>
    /// 地面についているかどうか
    /// </summary>
    public ReadOnlyReactiveProperty<bool> IsGrounded => _isGrounded;
    
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ground")) return;
        _isGrounded.Value = true;
    }
    
    public void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ground")) return;
        _isGrounded.Value = true;
    }
    
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Ground")) return;
        AudioManager.Instance.PlayAsync(SEType.PlayerJump, destroyCancellationToken).Forget();
        _isGrounded.Value = false;
    }
}
