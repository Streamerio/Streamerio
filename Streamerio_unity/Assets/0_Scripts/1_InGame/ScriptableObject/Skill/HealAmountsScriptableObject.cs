using UnityEngine;

[CreateAssetMenu(fileName = "HealAmountsScriptableObject", menuName = "SO/InGame/Skill/HealAmountsScriptableObject")]
public class HealAmountsScriptableObject : ScriptableObject
{
    [Header("Strong Skill Setting")]
    public int StrongHealAmount = 15;
    public int MiddleHealAmount = 10;
    public int WeakHealAmount = 5;
}