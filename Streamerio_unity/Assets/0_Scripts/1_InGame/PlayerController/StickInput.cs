using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Common.UI.Part.Button;
using R3;
using VContainer;
using VContainer.Unity;
using System;

public class StickInput : MonoBehaviour, IController, IStartable, ITickable
{
    [SerializeField] private PlayerPresenter _player;
    [SerializeField] private BulletShooter _bulletShooter;
    [SerializeField] private Joystick _joystick;
    private ICommonButton _jumpButton;
    private ICommonButton _attackButton;

    private IAudioFacade _audioFacade;
    private int _lastJumpFrame = -1;
    
    [Inject]
    public void Construct([Key(ButtonType.Jump)] ICommonButton jumpButton,
                          [Key(ButtonType.Attack)] ICommonButton attackButton,
                          IAudioFacade audioFacade)
    {
        _jumpButton = jumpButton;
        _attackButton = attackButton;
        
        _audioFacade = audioFacade;
    }
    
    public void Start()
    {
        _jumpButton.OnClickAsObservable
            .Subscribe(_ =>
            {
                Jump();
            }).RegisterTo(destroyCancellationToken);
        
        _attackButton.OnClickAsObservable
            .Subscribe(_ =>
            {
                Attack();
            }).RegisterTo(destroyCancellationToken);
    }

    public void Tick()
    {
        float moveX = _joystick.Horizontal;
        Move(new Vector2(moveX, 0));
    }

    public void Move(Vector2 direction)
    {
        _player.Move(direction);
    }

    public void Jump()
    {
        // 同一フレーム内の重複呼び出しを抑止して、二重ジャンプを防ぐ
        if (_lastJumpFrame == Time.frameCount)
        {
            return;
        }
        _lastJumpFrame = Time.frameCount;

        _player.Jump();
    }

    public void Attack()
    {
        _audioFacade.PlayAsync(SEType.PlayerAttack, destroyCancellationToken).Forget();
        _bulletShooter.Shoot();
    }
}
