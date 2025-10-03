using UnityEngine;

[CreateAssetMenu(fileName = "BurningGhoulScriptableObject", menuName = "SO/InGame/Enemy/Character/BurningGhoul")]
public class BurningGhoulScriptableObject : ScriptableObject
{
    [Header("Base Parameter")]
    public float Speed = 1.5f;
    public float Power = 10;

    [Header("Relative Spawn Position With Player")]
    public float MinRelativeSpawnPosX = 10;
    public float MaxRelativeSpawnPosX = 15;
    public float MinRelativeSpawnPosY = 0;
    public float MaxRelativeSpawnPosY = 0;

    [Header("Chase Range")]
    public float DetectionRange = 8f;
    public float StopRange = 0.5f;
}