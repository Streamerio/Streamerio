using Common.Audio;
using Common.State;
using Cysharp.Threading.Tasks;
using InGame;
using InGame.UI.Heart;
using UnityEngine;
using R3;
using VContainer;

public class HpPresenter : MonoBehaviour, IAbility
{
    private HpModel _hpModel;
    public float Amount => _hpModel.CurrentHp.Value;
    [SerializeField] private HeartGroupView _hpView;
    [SerializeField] private PlayerScriptableObject _scriptableObject;

    private float _currentHp;
    private float _maxHp;
    
    private IState _gameOverState;
    private IStateManager _stateManager;
    private IAudioFacade _audioFacade;
    
    [Inject]
    public void Construct([Key(StateType.ToGameOver)] IState gameOverState, IStateManager stateManager, IAudioFacade audioFacade)
    {
        _gameOverState = gameOverState;
        _stateManager = stateManager;
        _audioFacade = audioFacade;
    }

    void Awake()
    {
        _currentHp = _scriptableObject.InitialHp;
        _maxHp = _scriptableObject.MaxHp;
        _hpModel = new HpModel(_currentHp, _maxHp);
        _hpView.Initialize(0, _currentHp);
    }

    void Start()
    {
        Bind();
    }

    void Bind()
    {
        _hpModel.CurrentHp.Subscribe(hp =>
        {
            _hpView.UpdateHP(hp);
            if (hp <= 0)
            {
                Debug.Log("ゲームオーバー");
                //_stateManager.ChangeState(_gameOverState);
            }
        }).AddTo(this);
    }

    public void Increase(float amount)
    {
        _hpModel.Increase(amount);
    }

    public void Decrease(float amount)
    {
        _audioFacade.PlayAsync(SEType.PlayerDamage, destroyCancellationToken).Forget();
        _hpModel.Decrease(amount);
    }
}