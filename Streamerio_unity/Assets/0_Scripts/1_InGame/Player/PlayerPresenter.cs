using UnityEngine;
using R3;

public class PlayerPresenter : MonoBehaviour
{
    private PlayerModel _model;
    private PlayerView _view;
    [SerializeField] private PlayerScriptableObject _playerData;
    [SerializeField] private HpPresenter _hp;
    [SerializeField] private PowerPresenter _power;
    [SerializeField] private SpeedPresenter _speed;
    [SerializeField] private JumpPowerPresenter _jumpPower;

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
        _view.Jump(_jumpPower.Amount);
    }

    public void Attack(int num)
    {
        
    }
}