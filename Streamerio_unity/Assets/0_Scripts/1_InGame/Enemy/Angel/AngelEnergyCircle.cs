using UnityEngine;

public class AngelEnergyCircle : MonoBehaviour
{
    private int _damage;
    private float _lifetime;
    private bool _hasHitPlayer;

    // 追尾/オービット用
    private Transform _followTarget;
    private Vector3 _dir;             // 正規化方向
    private float _maxRadius;
    private float _expandDuration;
    private float _orbitSpeedDeg;
    private float _age;
    private float _currentRadius;
    private float _angleDeg;          // オービット角度（回転用）

    private SpriteRenderer _renderer;
    private float _initialLife;

    public void Initialize(
        int damage,
        float lifetime,
        Transform followTarget,
        Vector3 direction,
        float maxRadius,
        float expandDuration,
        float orbitSpeedDeg
    )
    {
        // 既存の Invoke 等をクリア
        CancelInvoke();

        _damage = damage;
        _lifetime = lifetime;
        _initialLife = lifetime;
        _followTarget = followTarget;
        _dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.up;
        _maxRadius = Mathf.Max(0.1f, maxRadius);
        _expandDuration = Mathf.Max(0.01f, expandDuration);
        _orbitSpeedDeg = orbitSpeedDeg;
        _renderer = GetComponent<SpriteRenderer>();

        _currentRadius = 0f;
        _age = 0f;
        _hasHitPlayer = false;
        _angleDeg = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg;

        // 生成直後は本体位置に配置
        if (_followTarget != null)
        {
            transform.position = _followTarget.position;
        }

        // 非破壊化戦略: lifetime が尽きたら Deactivate を呼ぶ
        CancelInvoke(nameof(Deactivate));
        if (_lifetime > 0f)
        {
            Invoke(nameof(Deactivate), _lifetime + 0.2f);
        }

        // 表示系を初期化（フェード等の残存対策）
        if (_renderer != null)
        {
            var c = _renderer.color;
            c.a = 1f;
            _renderer.color = c;
        }
    }

    void Update()
    {
        // GameObject が非アクティブなら Update は呼ばれないため追加チェック不要だが安全策として
        if (!gameObject.activeInHierarchy) return;

        float dt = Time.deltaTime;
        _age += dt;
        _lifetime -= dt;

        UpdateMotion(dt);
        UpdateFade();

        // ライフが尽きたら非アクティブ化
        if (_lifetime <= 0f)
        {
            Deactivate();
        }
    }

    private void UpdateMotion(float dt)
    {
        if (_followTarget == null)
        {
            // フォロー対象消失時はその場で静止
            return;
        }

        // 半径拡張フェーズ
        if (_age <= _expandDuration)
        {
            float t = _age / _expandDuration;
            _currentRadius = Mathf.Lerp(0f, _maxRadius, t);
        }
        else
        {
            _currentRadius = _maxRadius;
            // オービット回転
            _angleDeg += _orbitSpeedDeg * dt;
            float rad = _angleDeg * Mathf.Deg2Rad;
            _dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        }

        transform.position = _followTarget.position + _dir * _currentRadius;
    }

    private void UpdateFade()
    {
        if (_renderer == null) return;
        // 残り 0.5 秒でフェード
        if (_lifetime < 0.5f)
        {
            float alpha = Mathf.Clamp01(_lifetime / 0.5f);
            var c = _renderer.color;
            c.a = alpha;
            _renderer.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_hasHitPlayer) return;
        if (!collision.CompareTag("Player")) return;

        _hasHitPlayer = true;
        var hpPresenter = collision.GetComponent<HpPresenter>();
        if (hpPresenter == null)
        {
            hpPresenter = collision.GetComponentInParent<HpPresenter>();
        }

        if (hpPresenter != null)
        {
            hpPresenter.Decrease(_damage);
            Debug.Log($"[AngelEnergyCircle] Hit Player damage={_damage}");
        }

        // ヒット時は短時間の猶予を置いて非アクティブ化（エフェクト残し）
        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), 0.1f);
    }

    private void Deactivate()
    {
        CancelInvoke(nameof(Deactivate));
        // 内部状態は Initialize 時にリセットされる想定で非アクティブ化のみ行う
        gameObject.SetActive(false);
    }
}