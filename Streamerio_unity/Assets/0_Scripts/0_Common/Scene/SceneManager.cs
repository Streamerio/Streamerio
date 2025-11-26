using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace Common.Scene
{
    public interface ISceneManager
    {
        /// <summary>
        /// リロードしたか
        /// </summary>
        bool IsRestart { get; }
        
        /// <summary>
        /// シーンをロードする(前のシーンがアンロード)
        /// </summary>
        /// <param name="sceneType"></param>
        UniTask LoadSceneAsync(SceneType sceneType);
        
        /// <summary>
        /// リロード
        /// </summary>
        UniTask ReloadSceneAsync();
        
        void UpdateRestartFlag(bool isRestart);
    }
    
    public class SceneManager: ISceneManager, IInitializable, IDisposable
    {
        private SceneType _currentScene = SceneType.None;
        
        private bool _isRestart = false;
        /// <summary>
        /// リロードしたか
        /// </summary>
        public bool IsRestart => _isRestart;
        
        private CancellationTokenSource _cts;
        
        public void Initialize()
        {
            _cts = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        
        /// <summary>
        /// シーンをロードする(前のシーンがアンロード)
        /// </summary>
        /// <param name="sceneType"></param>
        public async UniTask LoadSceneAsync(SceneType sceneType)
        {
            if (_currentScene == sceneType)
            {
                // 既に同じシーンがロードされている場合は何もしない
                return;
            }
            
            if(_currentScene != SceneType.None)
            {
                // 現在のシーンをアンロード
                await UnityEngine.SceneManagement.SceneManager
                    .UnloadSceneAsync(_currentScene.ToString())
                    .ToUniTask(cancellationToken: _cts.Token);
            }

            _currentScene = sceneType;
            await UnityEngine.SceneManagement.SceneManager
                .LoadSceneAsync(sceneType.ToString(), LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: _cts.Token);
        }

        /// <summary>
        /// リロード
        /// </summary>
        public async UniTask ReloadSceneAsync()
        {
            await UnityEngine.SceneManagement.SceneManager
                .UnloadSceneAsync(_currentScene.ToString())
                .ToUniTask(cancellationToken: _cts.Token);

            await UnityEngine.SceneManagement.SceneManager
                .LoadSceneAsync(_currentScene.ToString(), LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: _cts.Token);
        }

        public void UpdateRestartFlag(bool isRestart)
        {
            _isRestart = isRestart;
        }
    }
}
