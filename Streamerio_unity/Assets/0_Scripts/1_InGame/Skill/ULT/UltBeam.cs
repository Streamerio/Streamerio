using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Common.Audio;
using VContainer;

public class UltBeam : MonoBehaviour
{
    [SerializeField] private float _speed = 15f;
    [SerializeField] private float _damage = 80f;
    [SerializeField] private float _lifetime = 6f;
    [SerializeField] private float _beamWidth = 2f;
    [SerializeField] private float _continuousDamageInterval = 0.5f; // 持続ダメージ間隔(秒)
    [SerializeField] private float _continuousDamage = 20f; // 持続ダメージ量
    
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
            transform.position = new Vector2(_player.transform.position.x + 6f, _player.transform.position.y + 1f);
        }
        // フレームベースでインターバルを計算
        _damageIntervalFrames = Mathf.RoundToInt(_continuousDamageInterval / Time.fixedDeltaTime);
        
        AudioManager.Instance.AudioFacade.PlayAsync(SEType.魔法1, this.GetCancellationTokenOnDestroy()).Forget();
    }

    void Update()
    {
        Move();
        if (_lifetime <= 0)
        {
            DestroySkill();
        }
        _lifetime -= Time.deltaTime;
    }

    private void Move()
    {
        // ビームは直進し、幅を持つ貫通攻撃
        transform.Translate(Vector2.right * _speed * Time.deltaTime);
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
                Debug.Log($"UltBeam entered: {collision.gameObject.name}");
                // 初期ダメージ
                enemy.TakeDamage((int)_damage);
                
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
                    Debug.Log($"UltBeam continuous damage: {collision.gameObject.name}");
                    enemy.TakeDamage((int)_continuousDamage);
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
            //Debug.Log($"UltBeam exited: {collision.gameObject.name}");
        }
    }
}