using R3;
using System;

public class HpModel
{
    public ReactiveProperty<float> CurrentHp { get; set; }
    public float _maxHp { get; set; }

    public HpModel(float initialHp, float maxHp)
    {
        CurrentHp = new ReactiveProperty<float>(initialHp);
        _maxHp = maxHp;
    }
    public void Increase(float amount)
    {
        CurrentHp.Value = Math.Min(CurrentHp.Value + amount, _maxHp);
        UnityEngine.Debug.Log("Hp increased to: " + CurrentHp.Value);
    }
    public void Decrease(float amount)
    {
        CurrentHp.Value = Math.Max(CurrentHp.Value - amount, 0);
    }
}