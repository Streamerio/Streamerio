public interface IHealth
{
    public int Health { get; }
}

/// <summary>
/// 攻撃を受けることができるインターフェース
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// ダメージを受ける
    /// </summary>
    /// <param name="amount"></param>
    void TakeDamage(float amount);
}