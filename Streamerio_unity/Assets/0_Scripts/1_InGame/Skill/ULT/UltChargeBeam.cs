using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Common.Audio;
using VContainer;

public class UltChargeBeam : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _damage = 60f;
    [SerializeField] private float _lifetime = 8f;
    [SerializeField] private float _chargeTime = 4f;
    [SerializeField] private float _accelerationRate = 1.5f;
    [SerializeField] private float _damageIncrease = 20f;
    [SerializeField] private float _continuousDamageInterval = 0.3f; // 持続ダメージ間隔(秒)
    [SerializeField] private float _continuousDamage = 15f; // 持続ダメージ量
    
    private float _currentSpeed;
    private float _currentDamage;
    private bool _isCharging = true;
    private float _chargeTimer = 0f;
    private Dictionary<GameObject, int> _enemyDamageCounters = new Dictionary<GameObject, int>();
    private int _damageIntervalFrames;
    private GameObject _player;
    
    private IAudioFacade _audioFacade;

    [Inject]
    public void Construct(IAudioFacade audioFacade)
    {
        _audioFacade = audioFacade;
    }

    void Awake()
    {
        _player = GameObject.FindWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player object not found in the scene.");
        }
    }

    void Start()
    {
        if (_player != null)
        {
            //playerのy座標から8マス右側に生成
            transform.position = new Vector2(_player.transform.position.x + 2, _player.transform.position.y);
        }
        _currentSpeed = _speed * 0.3f; // 最初は遅い
        _currentDamage = _damage;
        
        // フレームベースでインターバルを計算
        _damageIntervalFrames = Mathf.RoundToInt(_continuousDamageInterval / Time.fixedDeltaTime);
        
        // チャージエフェクト（色変化など）
        StartChargingEffect();
        
        AudioManager.Instance.AudioFacade.PlayAsync(SEType.魔法1, destroyCancellationToken).Forget();
    }

    void Update()
    {
        HandleCharging();
        Move();
        
        if (_lifetime <= 0)
        {
            DestroySkill();
        }
        _lifetime -= Time.deltaTime;
    }

    private void HandleCharging()
    {
        if (_isCharging)
        {
            _chargeTimer += Time.deltaTime;
            
            // チャージ中は徐々に加速・威力増加
            float chargeProgress = _chargeTimer / _chargeTime;
            _currentSpeed = Mathf.Lerp(_speed * 0.3f, _speed, chargeProgress);
            
            if (_chargeTimer >= _chargeTime)
            {
                _isCharging = false;
                _currentSpeed = _speed * _accelerationRate;
                _currentDamage += _damageIncrease;
                OnChargeComplete();
            }
        }
        else
        {
            // チャージ完了後は更に加速
            _currentSpeed += _accelerationRate * Time.deltaTime;
        }
    }

    private void StartChargingEffect()
    {
        // スプライトの色を変化させてチャージを表現
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.yellow;
        }
    }

    private void OnChargeComplete()
    {
        // チャージ完了エフェクト
        var renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.color = Color.red;
        }
        //Debug.Log("ChargeBeam fully charged!");
    }

    private void Move()
    {
        transform.Translate(Vector2.right * _currentSpeed * Time.deltaTime);
    }

    public void DestroySkill()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyHpManager>();
            if (enemy != null)
            {
                //Debug.Log($"UltChargeBeam entered: {collision.gameObject.name} for {_currentDamage} damage");
                // 初期ダメージ
                enemy.TakeDamage((int)_currentDamage);
                
                // 持続ダメージ用のカウンターを初期化
                _enemyDamageCounters[collision.gameObject] = 0;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && _enemyDamageCounters.ContainsKey(collision.gameObject))
        {
            // フレームカウンターを更新
            _enemyDamageCounters[collision.gameObject]++;
            
            // インターバルに達したら持続ダメージを与える
            if (_enemyDamageCounters[collision.gameObject] >= _damageIntervalFrames)
            {
                var enemy = collision.gameObject.GetComponent<EnemyHpManager>();
                if (enemy != null)
                {
                    float continuousDmg = _isCharging ? _continuousDamage : _continuousDamage * 1.5f; // チャージ完了後は持続ダメージも増加
                    //Debug.Log($"UltChargeBeam continuous damage: {collision.gameObject.name}");
                    enemy.TakeDamage((int)continuousDmg);
                }
                
                // カウンターリセット
                _enemyDamageCounters[collision.gameObject] = 0;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") && _enemyDamageCounters.ContainsKey(collision.gameObject))
        {
            // 敵が範囲から出たらカウンターを削除
            _enemyDamageCounters.Remove(collision.gameObject);
            Debug.Log($"UltChargeBeam exited: {collision.gameObject.name}");
        }
    }
}