using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 既存スタートステージ(シーン配置済み)を起点に LeftPoint/RightPoint で連結生成
/// </summary>
public class StageCreater : MonoBehaviour
{
    [Header("ステージPrefab群")]
    [SerializeField] private List<GameObject> stagePrefabs = new();
    [SerializeField] private GameObject stageParent;
    [SerializeField] private GameObject goalPrefab;
    [SerializeField] private int stageCountToGoal = 10;

    [Header("スタートステージ (シーン上の既存)"), Tooltip("シーンに既にある最初のステージの StageConnectionPoint")] 
    [SerializeField] private StageConnectionPoint startStage;

    [Header("生成制御")]
    [SerializeField] private Transform player;
    [SerializeField] private float generateDistance = 25f;   // プレイヤー前方この距離で次生成
    [SerializeField] private bool spawnFirstImmediately = true; // 起動直後に1つ目生成

    [Header("デバッグ")]
    [SerializeField] private bool showLog = false;
    [SerializeField] private bool drawGizmos = true;

    private StageConnectionPoint _lastConn;          // 直近ステージ
    private float _nextGenerateX;                    // 次生成判定用X
    private int _generatedCount;                     // 追加生成数(スタート除外)
    private bool _goalSpawned;

    void Start()
    {
        if (!Validate()) { enabled = false; return; }
        InitializeFromStartStage();
        if (spawnFirstImmediately) SpawnNext();
    }

    void Update()
    {
        if (_goalSpawned) return;
        if (player && player.position.x + generateDistance > _nextGenerateX)
        {
            SpawnNext();
        }
    }

    private bool Validate()
    {
        if (startStage == null)
        {
            Debug.LogError("[StageCreater] startStage 未設定 (スタートステージに StageConnectionPoint を付けて割当)");
            return false;
        }
        if (startStage.LeftPoint == null || startStage.RightPoint == null)
        {
            Debug.LogError("[StageCreater] startStage の LeftPoint / RightPoint が未設定");
            return false;
        }
        if (stagePrefabs.Count == 0)
        {
            Debug.LogError("[StageCreater] stagePrefabs が空");
            return false;
        }
        return true;
    }

    private void InitializeFromStartStage()
    {
        _lastConn = startStage; // 既存ステージをそのまま起点
        _nextGenerateX = _lastConn.RightPosition.x;
        _generatedCount = 0; // スタートはカウントしない
        if (showLog) Debug.Log($"[StageCreater] Base={startStage.name} RightX={_nextGenerateX}");
    }

    private void SpawnNext()
    {
        // ゴール生成条件
        bool spawnGoal = !_goalSpawned && goalPrefab && _generatedCount >= stageCountToGoal;
        GameObject prefab = spawnGoal ? goalPrefab : stagePrefabs[Random.Range(0, stagePrefabs.Count)];

        GameObject inst = Instantiate(prefab, stageParent.transform);
        var conn = inst.GetComponent<StageConnectionPoint>();

        if (conn)
        {
            // 新ステージ Left を直前の Right に一致
            conn.AlignLeftTo(_lastConn.RightPosition);
            _lastConn = conn;
            _nextGenerateX = conn.RightPosition.x;
        }
        else
        {
            // フォールバック: 接続点なし
            float baseX = _lastConn.RightPosition.x;
            inst.transform.position = new Vector3(baseX, startStage.transform.position.y, startStage.transform.position.z);
            _nextGenerateX = inst.transform.position.x + 10f;
            if (showLog) Debug.LogWarning($"[StageCreater] 接続点不足 {inst.name}");
        }

        if (spawnGoal)
        {
            _goalSpawned = true;
            if (showLog) Debug.Log($"[StageCreater] Goal Spawned={inst.name}");
        }
        else
        {
            _generatedCount++;
        }

        if (showLog) Debug.Log($"[StageCreater] Spawn={inst.name} count={_generatedCount} nextX={_nextGenerateX}");
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (player)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(player.position.x + generateDistance, player.position.y, 0f), 1f);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(_nextGenerateX, -2f, 0f), new Vector3(_nextGenerateX, 2f, 0f));
        if (startStage)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(startStage.LeftPosition, 0.35f);
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(startStage.RightPosition, 0.35f);
        }
    }
#endif
}