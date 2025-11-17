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
        private bool _isDataFetched = false;
        public bool IsDataFetched => _isDataFetched;
        
        [SerializeField, Min(0f)]
        private int _timeOutTime = 8;
        
        [SerializeField]
        private MasterGameSetting _gameSetting;
        public MasterGameSetting GameSetting => _gameSetting;

        [SerializeField] private MasterPlayerStatus _playerStatus; 
        public MasterPlayerStatus PlayerStatus => _playerStatus;
        [SerializeField]
        private SerializeDictionary<MasterUltType, MasterUltStatus> _ultStatusDictionary;
        public IReadOnlyDictionary<MasterUltType, MasterUltStatus> UltStatusDictionary => _ultStatusDictionary.ToDictionary();
        [SerializeField]
        private SerializeDictionary<MasterEnemyType, MasterEnemyStatus> _enemyStatusDictionary;
        public IReadOnlyDictionary<MasterEnemyType, MasterEnemyStatus> EnemyStatusDictionary => _enemyStatusDictionary.ToDictionary();
        
        public async UniTask FetchDataAsync(CancellationToken ct)
        {
            var gameTask    = SpreadSheetClient.GetRequestAsync(SheetType.GameSettings, ct, _timeOutTime);
            var playerTask  = SpreadSheetClient.GetRequestAsync(SheetType.PlayerStatus, ct, _timeOutTime);
            var ultTask     = SpreadSheetClient.GetRequestAsync(SheetType.UltStatus, ct, _timeOutTime);
            var enemyTask   = SpreadSheetClient.GetRequestAsync(SheetType.EnemyStatus, ct, _timeOutTime);

            var (gameRows, playerRows, ultRows, enemyRows) =
                await UniTask.WhenAll(gameTask, playerTask, ultTask, enemyTask);

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
                        HP = ToFloat(enemyRows[MasterEnemyStatus.HPKey][i]),
                        AttackPower = ToFloat(enemyRows[MasterEnemyStatus.AttackPowerKey][i]),
                        Speed = ToFloat(enemyRows[MasterEnemyStatus.SpeedKey][i]),
                    });   
            }
            
            _isDataFetched = IsValidDataRow(gameRows)
                             && IsValidDataRow(playerRows)
                             && IsValidDataRow(ultRows)
                             && IsValidDataRow(enemyRows);
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
        public const string HPKey = "HP";
        public const string AttackPowerKey = "AttackPower";
        public const string SpeedKey = "Speed";
        
        public float HP;
        public float AttackPower;
        public float Speed;
    }
    
    public enum MasterEnemyType
    {
        Skelton,
        FireMan,
        Cat,
    }
}