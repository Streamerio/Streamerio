using System;
using System.Threading;
using Common;
using R3;
using VContainer;
using VContainer.Unity;
using Random = UnityEngine.Random;

namespace InGame.Enemy
{
    public class EnemyMediator: IStartable, IDisposable
    {
        private IWebSocketManager _webSocketManager;
        private IMasterData _masterData;
        
        private IEnemySpawner _enemySpawner;
        
        private readonly CancellationTokenSource _cts = new ();
        
        [Inject]
        public void Construct(IWebSocketManager webSocketManager, IMasterData masterData, IEnemySpawner enemySpawner)
        {
            _webSocketManager = webSocketManager;
            _masterData = masterData;
            _enemySpawner = enemySpawner;
        }

        public void Start()
        {
            Bind();
        }

        /// <summary>
        /// バインド処理
        /// </summary>
        private void Bind()
        {
            BindSpawnEnemy(FrontKey.enemy1,  MasterEnemyRarityType.Weak); 
            BindSpawnEnemy(FrontKey.enemy2, MasterEnemyRarityType.Normal);
            BindSpawnEnemy(FrontKey.enemy3, MasterEnemyRarityType.Strong);
        }
        
        /// <summary>
        /// WebSocketのイベントと敵出現処理を紐付ける
        /// </summary>
        /// <param name="frontKey"></param>
        /// <param name="rarityType"></param>
        private void BindSpawnEnemy(FrontKey frontKey, MasterEnemyRarityType rarityType)
        {
            _webSocketManager.FrontEventDict[frontKey]
                .Subscribe(_ =>
                {
                    var enemys = _masterData.EnemyRarityDictionary[rarityType];
                    var randomIndex = Random.Range(0, enemys.Count);
                    _enemySpawner.Spawn(enemys[randomIndex]);
                })
                .RegisterTo(_cts.Token);
        }
        
        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}