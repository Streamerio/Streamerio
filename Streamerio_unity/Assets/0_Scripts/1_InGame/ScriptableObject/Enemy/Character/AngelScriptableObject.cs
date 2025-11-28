using UnityEngine;

[CreateAssetMenu(fileName = "AngelScriptableObject", menuName = "SO/InGame/Enemy/Character/Angel")]
public class AngelScriptableObject : ScriptableObject
{
    [Header("Base Parameter")]
    public int Health = 30;

    [Header("Relative Spawn Position With Player")]
    public float MinRelativeSpawnPosX = 15;
    public float MaxRelativeSpawnPosX = 20;
    public float MinRelativeSpawnPosY = 5;
    public float MaxRelativeSpawnPosY = 10;

    [Header("移動設定")]
    public float verticalSpeed = 2f;
    public float horizontalSpeed = 1f;
    public float verticalRange = 3f;
    public float horizontalRange = 4f;
    public float baseLeftSpeed = 0.8f;

    [Header("攻撃設定")]
    public GameObject energyCirclePrefab;
    public float attackInterval = 4f;
    public float circleLifetime = 2f;
    public float circleRadius = 12f;
    public int circleCount = 8;
    public float circleExpandDuration = 0.6f;
    public float circleOrbitSpeedDeg = 50f;
    public bool randomizeStartAngle = true;
}