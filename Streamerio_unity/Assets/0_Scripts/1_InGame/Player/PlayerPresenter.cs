using Common.Audio;
using Cysharp.Threading.Tasks;
using UnityEngine;
using R3;
using VContainer;

public class PlayerPresenter : MonoBehaviour
{
    private PlayerModel _model;
    private PlayerView _view;
    [SerializeField] private PlayerScriptableObject _playerData;
    [SerializeField] private HpPresenter _hp;
    [SerializeField] private PowerPresenter _power;
    [SerializeField] private SpeedPresenter _speed;
    [SerializeField] private JumpPowerPresenter _jumpPower;
    
    private IAudioFacade _audioFacade;

    [Inject]
    public void Construct(IAudioFacade audioFacade)
    {
        _audioFacade = audioFacade;
    }

    void Awake()
    {
        _view = GetComponent<PlayerView>();
        _model = new PlayerModel(_view.gameObject.transform.position.x, _view.gameObject.transform.position.y);
    }

    void Start()
    {
        Bind();
        _view.Bind();
    }

    private void Bind()
    {
        Observable.EveryUpdate()
            .Select(_ => _view.gameObject.transform.position)
            .DistinctUntilChanged()
            .Subscribe(pos =>
            {
                _model.PosX = pos.x;
                _model.PosY = pos.y;
            })
            .AddTo(this);
    }

    public void Move(Vector2 delta)
    {
        _view.Move(delta * _speed.Amount * Time.deltaTime);
    }

    public void Jump()
    {
        AudioManager.Instance.AudioFacade.PlayAsync(SEType.PlayerJump, destroyCancellationToken).Forget();
        _view.Jump(_jumpPower.Amount);
    }

    public void Attack(int num)
    {
        
    }
}