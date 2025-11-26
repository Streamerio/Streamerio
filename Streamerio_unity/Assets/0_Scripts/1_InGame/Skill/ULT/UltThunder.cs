using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Common.Audio;
using System.Linq;
using VContainer;

[RequireComponent(typeof(BoxCollider2D))]
public class UltThunder : MonoBehaviour
{
    [SerializeField] private float _speed = 25f;
    [SerializeField] private float _damage = 90f;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private int _strikeCount = 3;
    [SerializeField] private float _strikeInterval = 0.5f;
    [SerializeField] private float _continuousDamageInterval = 0.4f;
    [SerializeField] private float _continuousDamage = 30f;

    private int _currentStrikes = 0;
    private float _strikeTimer = 0f;

    // 侵入中の敵
    private readonly HashSet<GameObject> _enemiesInRange = new HashSet<GameObject>();
    private readonly HashSet<GameObject> _hitEnemies = new HashSet<GameObject>();
    private readonly Dictionary<GameObject, float> _enemyTimers = new Dictionary<GameObject, float>();

    private GameObject _player;
    private BoxCollider2D _box;
    private float _damageIntervalFrames;
    
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

        _box = GetComponent<BoxCollider2D>();
        _box.isTrigger = true;
    }

    void OnValidate()
    {
        if (_box == null) _box = GetComponent<BoxCollider2D>();
        if (_box != null)
        {
            _box.isTrigger = true;
        }
    }

    void Start()
    {
        if (_player != null)
        {
            transform.position = new Vector2(_player.transform.position.x + 6f,
                                             _player.transform.position.y + 3f);
        }
        _damageIntervalFrames = Mathf.RoundToInt(_continuousDamageInterval / Time.fixedDeltaTime);
        
        StartThunderStrike();

        AudioManager.Instance.AudioFacade.PlayAsync(SEType.UltThunder, destroyCancellationToken).Forget();
    }

    void Update()
    {
        BlinkEffect();
        HandleContinuousDamage();
        _lifetime -= Time.deltaTime;
        if (_lifetime <= 0f) DestroySkill();
    }



    private async UniTaskVoid StartThunderStrike()
    {
        for (int i = 0; i < _strikeCount; i++)
        {
            PerformThunderStrike();
            await UniTask.Delay((int)(_strikeInterval * 1000));
            if (this == null) break;
        }
    }

    private void PerformThunderStrike()
    {
        // 安全のためスナップショットを作成して列挙します。
        // さらに foreach 内で例外を捕捉して処理の中断を防止します。
        foreach (var enemyObj in _enemiesInRange.ToList())
        {
            try
            {
                if (_hitEnemies.Contains(enemyObj)) continue;
                var hp = enemyObj.GetComponent<EnemyHpManager>();
                if (hp != null)
                {
                    hp.TakeDamage((int)_damage);
                    _hitEnemies.Add(enemyObj);
                }
            }
            catch (System.InvalidOperationException invEx)
            {
                // コレクションの変更による例外が念のため発生した場合のログ
                Debug.LogWarning($"PerformThunderStrike: collection modified during enumeration for {enemyObj?.name}. Exception: {invEx.Message}");
            }
            catch (System.Exception ex)
            {
                // その他の例外も握りつぶさずログ化
                Debug.LogError($"PerformThunderStrike: unexpected exception for {enemyObj?.name}: {ex}");
            }
        }
        _currentStrikes++;
    }

    private void HandleContinuousDamage()
    {
        if (_enemiesInRange.Count == 0) return;

        float dt = Time.deltaTime;
        foreach (var enemyObj in _enemiesInRange.ToList())
        {
            if (!_enemyTimers.ContainsKey(enemyObj))
            {
                _enemyTimers[enemyObj] = 0f;
                continue;
            }

            _enemyTimers[enemyObj] += dt;
            if (_enemyTimers[enemyObj] >= _continuousDamageInterval)
            {
                var hp = enemyObj.GetComponent<EnemyHpManager>();
                if (hp != null)
                {
                    hp.TakeDamage((int)_continuousDamage);
                }
                _enemyTimers[enemyObj] = 0f;
            }
        }
    }

    private void BlinkEffect()
    {
        _strikeTimer += Time.deltaTime;
        if (_strikeTimer > 0.1f)
        {
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.enabled = !renderer.enabled;
            _strikeTimer = 0f;
        }
    }

    public void DestroySkill()
    {
        Destroy(gameObject);
    }

    // Trigger 侵入
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        var go = other.gameObject;
        _enemiesInRange.Add(go);
        if (!_enemyTimers.ContainsKey(go)) _enemyTimers[go] = 0f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        var go = other.gameObject;
        _enemiesInRange.Remove(go);
        _enemyTimers.Remove(go);
    }
}