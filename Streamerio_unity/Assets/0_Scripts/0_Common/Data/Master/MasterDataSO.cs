using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Alchemy.Inspector;
using Common.GAS;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZLinq;

namespace Common
{
    
    [CreateAssetMenu(fileName = "MasterDataSO", menuName = "SO/Master")]
    public class MasterDataSO : ScriptableObject, IMasterData
    {
#if UNITY_EDITOR
        [Button]
        private async UniTaskVoid FetchData()
        {
            await FetchDataAsync(default);
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
#if UNITY_EDITOR
        [Button]
        private void Reset()
        {
            _gameSetting = new MasterGameSetting();
            _playerStatus = new MasterPlayerStatus();
            _ultStatusDictionary = new SerializeDictionary<MasterUltType, MasterUltStatus>();
            _enemyStatusDictionary = new SerializeDictionary<MasterEnemyType, MasterEnemyStatus>();
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
        }
#endif
#if UNITY_EDITOR
        [SerializeField, Tooltip("ゲーム開始時にマスターデータを取得するか")]
        private bool _isLoad = true;
#endif
        
        private bool _isDataFetched = false;
        public bool IsDataFetched => _isDataFetched;
        
        [SerializeField, Min(0f), Tooltip("マスターデータ取得のタイムアウト時間(秒)")]
        private int _timeOutTime = 8;
        
        [SerializeField, Tooltip("ゲーム設定")]
        private MasterGameSetting _gameSetting;
        public MasterGameSetting GameSetting => _gameSetting;

        [SerializeField, Tooltip("プレイヤーステータス")]
        private MasterPlayerStatus _playerStatus; 
        public MasterPlayerStatus PlayerStatus => _playerStatus;
        [SerializeField, Tooltip("スキルステータス辞書")]
        private SerializeDictionary<MasterUltType, MasterUltStatus> _ultStatusDictionary;
        public IReadOnlyDictionary<MasterUltType, MasterUltStatus> UltStatusDictionary => _ultStatusDictionary.ToDictionary();
        [SerializeField, Tooltip("敵ステータス辞書")]
        private SerializeDictionary<MasterEnemyType, MasterEnemyStatus> _enemyStatusDictionary;
        public IReadOnlyDictionary<MasterEnemyType, MasterEnemyStatus> EnemyStatusDictionary => _enemyStatusDictionary.ToDictionary();
        [SerializeField, ReadOnly, Tooltip("敵ステータスレアリティ辞書")]
        private SerializeDictionary<MasterEnemyRarityType, List<MasterEnemyType>> _enemyRarityDictionary;
        public IReadOnlyDictionary<MasterEnemyRarityType, List<MasterEnemyType>> EnemyRarityDictionary => _enemyRarityDictionary.ToDictionary();
        
        [SerializeField, Tooltip("バックエンド設定")]
        private MasterBackendSettings _backendSettings;
        public MasterBackendSettings BackendSettings => _backendSettings;
        
        public async UniTask FetchDataAsync(CancellationToken ct)
        {
#if UNITY_EDITOR
            if (!_isDataFetched && !_isLoad)
            {
                _isDataFetched = true;
                Debug.Log("MasterDataSO FetchDataAsync skipped due to already fetched and _isLoad is false.");
                return;
            }
#endif
            
            var gameTask    = SpreadSheetClient.GetRequestAsync(SheetType.GameSettings, ct, _timeOutTime);
            var playerTask  = SpreadSheetClient.GetRequestAsync(SheetType.PlayerStatus, ct, _timeOutTime);
            var ultTask     = SpreadSheetClient.GetRequestAsync(SheetType.UltStatus, ct, _timeOutTime);
            var enemyTask   = SpreadSheetClient.GetRequestAsync(SheetType.EnemyStatus, ct, _timeOutTime);
            var urlTask     = SpreadSheetClient.GetRequestAsync(SheetType.URL, ct, _timeOutTime);

            var (gameRows, playerRows, ultRows, enemyRows, urlRows) = 
                await UniTask.WhenAll(gameTask, playerTask, ultTask, enemyTask, urlTask);

            if (IsValidDataRow(gameRows))
            {
                _gameSetting = new MasterGameSetting()
                {
                    TimeLimit = ToFloat(gameRows[MasterGameSetting.TimeLimitKey][0]),
                };
            }

            if (IsValidDataRow(playerRows))
            {
                _playerStatus = new MasterPlayerStatus()
                {
                    HP = ToFloat(playerRows[MasterPlayerStatus.HPKey][0]),
                    AttackPower = ToFloat(playerRows[MasterPlayerStatus.AttackPowerKey][0]),
                    Speed = ToFloat(playerRows[MasterPlayerStatus.SpeedKey][0]),
                    JumpPower = ToFloat(playerRows[MasterPlayerStatus.JumpPowerKey][0]),
                };   
            }
            
            if (IsValidDataRow(ultRows))
            {
                _ultStatusDictionary = CreateDictionary<MasterUltType, MasterUltStatus>(
                    ultRows,
                    MasterUltStatus.UltTypeKey,
                    i => new MasterUltStatus()
                    {
                        AttackPower = ToFloat(ultRows[MasterUltStatus.AttackPowerKey][i]),
                    });   
            }

            if (IsValidDataRow(enemyRows))
            {
                _enemyStatusDictionary = CreateDictionary<MasterEnemyType, MasterEnemyStatus>(
                    enemyRows,
                    MasterEnemyStatus.EnemyTypeKey,
                    i => new MasterEnemyStatus()
                    {
                        EnemyType = (MasterEnemyType)Enum.Parse(typeof(MasterEnemyType), (string)enemyRows[MasterEnemyStatus.EnemyTypeKey][i]),
                        Rarity = (MasterEnemyRarityType)Enum.Parse(typeof(MasterEnemyRarityType), (string)enemyRows[MasterEnemyStatus.EnemyRarityTypeKey][i]),
                        HP = ToFloat(enemyRows[MasterEnemyStatus.HPKey][i]),
                        AttackPower = ToFloat(enemyRows[MasterEnemyStatus.AttackPowerKey][i]),
                        Speed = ToFloat(enemyRows[MasterEnemyStatus.SpeedKey][i]),
                        AblePlayerDistance = ToFloat(enemyRows[MasterEnemyStatus.AblePlayerDistanceKey][i]),
                        MinSpawnPositionX = ToFloat(enemyRows[MasterEnemyStatus.MinSpawnPositionXKey][i]),
                        MaxSpawnPositionX = ToFloat(enemyRows[MasterEnemyStatus.MaxSpawnPositionXKey][i]),
                        MinSpawnPositionY = ToFloat(enemyRows[MasterEnemyStatus.MinSpawnPositionYKey][i]),
                        MaxSpawnPositionY = ToFloat(enemyRows[MasterEnemyStatus.MaxSpawnPositionYKey][i]),
                        PoolSize = Convert.ToInt32(enemyRows[MasterEnemyStatus.PoolSizeKey][i], CultureInfo.InvariantCulture),
                    });
                
                // レアリティ辞書の作成
                _enemyRarityDictionary = new SerializeDictionary<MasterEnemyRarityType, List<MasterEnemyType>>();
                foreach (var enemyStatus in EnemyStatusDictionary.Keys)
                {
                    var rarity = _enemyStatusDictionary[enemyStatus].Rarity;
                    if (!_enemyRarityDictionary.ContainsKey(rarity))
                    {
                        _enemyRarityDictionary[rarity] = new List<MasterEnemyType>();
                    }
                    _enemyRarityDictionary[rarity].Add(enemyStatus);
                }
                
            }

            if (IsValidDataRow(urlRows))
            {
                _backendSettings = new MasterBackendSettings()
                {
                    FrontendURLFormat = (string)urlRows[MasterBackendSettings.FrontendURLFormatKey][0],
                    FrontendQueryParamFormat = (string)urlRows[MasterBackendSettings.FrontendQueryParamFormatKey][0],
                    BackendWebSocketURL = (string)urlRows[MasterBackendSettings.BackendWebSocketURLKey][0],
                    BackHttpURL = (string)urlRows[MasterBackendSettings.BackHttpURLKey][0],
                    GameStartResponse = (string)urlRows[MasterBackendSettings.GameStartResponseKey][0],
                    GameEndResponse = (string)urlRows[MasterBackendSettings.GameEndResponseKey][0],
                };
            }
            
            _isDataFetched = IsValidDataRow(gameRows)
                             && IsValidDataRow(playerRows)
                             && IsValidDataRow(ultRows)
                             && IsValidDataRow(enemyRows)
                             && IsValidDataRow(urlRows);
            Debug.Log($"MasterDataSO FetchDataAsync _isDataFetched:{_isDataFetched}");
        }
        
        private bool IsValidDataRow(Dictionary<string, List<object>> dataRows)
        {
            return dataRows is { Count: > 0 };
        }
        
        private SerializeDictionary<TEnum, TValue> CreateDictionary<TEnum, TValue>(Dictionary<string, List<object>> dataRows, string typeKey, Func<int, TValue> onCreate)
            where TEnum : Enum
            where TValue : new()
        {
            var dict = new SerializeDictionary<TEnum, TValue>();
            int count = dataRows.AsValueEnumerable().First().Value.Count;
            for (int i = 0; i < count; i++)
            {
                var type = (TEnum)Enum.Parse(typeof(TEnum), (string)dataRows[typeKey][i]);
                dict[type] = onCreate(i);
            }

            return dict;
        }
        
        private float ToFloat(object num)
        {
            return Convert.ToSingle(num, CultureInfo.InvariantCulture);
        }
    }
    
    public interface IMasterData
    {
        bool IsDataFetched { get; }
        
        MasterGameSetting GameSetting { get; }
        MasterPlayerStatus PlayerStatus { get; }
        IReadOnlyDictionary<MasterUltType, MasterUltStatus> UltStatusDictionary { get; }
        IReadOnlyDictionary<MasterEnemyType, MasterEnemyStatus> EnemyStatusDictionary { get; }
        IReadOnlyDictionary<MasterEnemyRarityType, List<MasterEnemyType>> EnemyRarityDictionary { get; }
        
        MasterBackendSettings BackendSettings { get; }
        
        UniTask FetchDataAsync(CancellationToken ct);
    }
    
    [Serializable]
    public class MasterGameSetting
    {
        public const string TimeLimitKey = "TimeLimit";
        
        public float TimeLimit;
    }
    
    [Serializable]
    public class MasterPlayerStatus
    {
        public const string HPKey = "HP";
        public const string AttackPowerKey = "AttackPower";
        public const string SpeedKey = "Speed";
        public const string JumpPowerKey = "JumpPower";
        
        public float HP;
        public float AttackPower;
        public float Speed;
        public float JumpPower;
    }

    [Serializable]
    public class MasterUltStatus
    {
        public const string UltTypeKey = "Type";
        public const string AttackPowerKey = "AttackPower";
        
        public float AttackPower;
    }
    
    public enum MasterUltType
    {
        Beam,
        Bullet,
        ChargeBeam,
        Thunder,
    }

    [Serializable]
    public class MasterEnemyStatus
    {
        public const string EnemyTypeKey = "Type";
        public const string EnemyRarityTypeKey = "Rarity";
        public const string HPKey = "HP";
        public const string AttackPowerKey = "AttackPower";
        public const string SpeedKey = "Speed";
        public const string AblePlayerDistanceKey = "AblePlayerDistance";
        public const string MinSpawnPositionXKey = "MinSpawnPositionX";
        public const string MaxSpawnPositionXKey = "MaxSpawnPositionX";
        public const string MinSpawnPositionYKey = "MinSpawnPositionY";
        public const string MaxSpawnPositionYKey = "MaxSpawnPositionY";
        public const string PoolSizeKey = "PoolSize";
        
        public MasterEnemyType EnemyType;
        public MasterEnemyRarityType Rarity;
        public float HP;
        public float AttackPower;
        public float Speed;
        public float AblePlayerDistance;
        public float MinSpawnPositionX;
        public float MaxSpawnPositionX;
        public float MinSpawnPositionY;
        public float MaxSpawnPositionY;
        public int PoolSize;
    }
    
    public enum MasterEnemyType
    {
        Skelton,
        FireMan,
        Cat,
        Angel,
    }
    
    public enum MasterEnemyRarityType
    {
        Weak,
        Normal,
        Strong,
    }

    [Serializable]
    public class MasterBackendSettings
    {
        public const string FrontendURLFormatKey = "FrontendURLFormat";
        public const string FrontendQueryParamFormatKey = "FrontendQueryParamFormat";
        public const string BackendWebSocketURLKey = "BackendWebSocketURL";
        public const string BackHttpURLKey = "BackHttpURL";
        public const string GameStartResponseKey = "GameStartResponse";
        public const string GameEndResponseKey = "GameEndResponse";
        
        public string FrontendURLFormat;
        public string FrontendQueryParamFormat;
        public string BackendWebSocketURL;
        public string BackHttpURL;
        public string GameStartResponse;
        public string GameEndResponse;
    }
}