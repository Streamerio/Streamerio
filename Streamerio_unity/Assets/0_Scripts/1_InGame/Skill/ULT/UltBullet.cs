using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using Common.Audio;
using InGame.Skill.ULT;
using R3;

public class UltBullet : MonoBehaviour, IUltSkill
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _damage = 35f;
    [SerializeField] private float _initLifetime = 4f;
    [SerializeField] private int _bulletCount = 5;
    [SerializeField] private float _spreadAngle = 30f;
    [SerializeField] private UltSubBullet _bulletPrefab;
    [SerializeField] private float _continuousDamageInterval = 0.2f; // 持続ダメージ間隔(秒)
    [SerializeField] private float _continuousDamage = 10f; // 持続ダメージ量
    
    private Vector2 _direction;
    
    private List<UltSubBullet> _subBullets = new List<UltSubBullet>();
    
    private Dictionary<GameObject, int> _enemyDamageCounters = new Dictionary<GameObject, int>();
    private int _damageIntervalFrames;
    
    private float _lifetime;
    
    private bool _isCreated = false;
    
    private Action _onRelease;
    
    private Transform _player;
    private IAudioFacade _audioFacade;
    
    private CancellationTokenSource _cts;
    
    public void OnCreate(float damage, Action onRelease, Transform player, IAudioFacade audioFacade)
    {
        _damage = damage;
        _onRelease = onRelease;
        _player = player;
        _audioFacade = audioFacade;
    }

    public void Initialize()
    {
        //playerのy座標から8マス右側に生成
        transform.position = new Vector2(_player.transform.position.x + 2f, _player.transform.position.y);
        
        gameObject.SetActive(true);
        if (!_isCreated)
        {
            CreateBulletSpread();   
        }
        foreach (var subBullet in _subBullets)
        {
            subBullet.transform.position = transform.position;
            subBullet.Initialize();
        }
        
        _audioFacade.PlayAsync(SEType.ThunderBullet, this.GetCancellationTokenOnDestroy()).Forget();

        Bind();
    }

    private void Bind()
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
        _lifetime = _initLifetime;
        
        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                if (_lifetime <= 0)
                {
                    DestroySkill();
                }
                _lifetime -= Time.deltaTime;
            })
            .RegisterTo(_cts.Token);
    }

    private void CreateBulletSpread()
    {
        float angleStep = _spreadAngle / (_bulletCount - 1);
        float startAngle = -_spreadAngle / 2;

        for (int i = 0; i < _bulletCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            Vector2 direction = new Vector2(
                Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                Mathf.Sin(currentAngle * Mathf.Deg2Rad)
            );

            var subBullet = Instantiate(_bulletPrefab, transform.position, Quaternion.identity, transform.parent)
                .GetComponent<UltSubBullet>();
            _subBullets.Add(subBullet);
            subBullet.OnCreate(direction.normalized);
        }
        
        _isCreated = true;
    }

    private void DestroySkill()
    {
        _onRelease?.Invoke();
        _cts.Cancel();
        foreach (var subBullet in _subBullets)
        {
            subBullet.DestroySkill();
        }
        gameObject.SetActive(false);
    }
}