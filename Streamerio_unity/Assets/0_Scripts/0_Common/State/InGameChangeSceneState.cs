using System.Threading;
using Common.Scene;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;

namespace Common.State
{
    public class InGameChangeSceneState: ChangeSceneState
    {
        private Transform _player;
        
        public InGameChangeSceneState(SceneType changeSceneType) : base(changeSceneType)
        {
        }
        
        [Inject]
        public void Construct([Key("Player")] Transform player)
        {
            _player = player;
        }
        
        public override async UniTask EnterAsync(CancellationToken ct)
        {
            await LoadingScreen.ShowAsync(_player.position, ct);
            await SceneManager.LoadSceneAsync(ChangeSceneType);
        }
    }
}