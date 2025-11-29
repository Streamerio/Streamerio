using Common.Booster;
using Common.Camera;
using Common.Scene;
using Common.State;
using Common.UI.Animation;
using Common.UI.Part.Button;
using InGame.Goal;
using InGame.Setting;
using InGame.Skill.Object;
using InGame.Skill.Spawner;
using InGame.Skill.UI.Panel;
using InGame.UI.Heart;
using InGame.UI.Timer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame
{
    public class InGameLifetimeScope: LifetimeScope
    {
        [SerializeField, Tooltip("インゲーム設定")]
        private InGameSettingSO _inGameSetting;
        
        [SerializeField, Tooltip("プレイヤーのTransform")]
        private Transform _playerTransform;
        
        [SerializeField, Tooltip("スキルリポジトリ")]
        private SkillRepositorySO _skillRepository;
        [SerializeField, Tooltip("スキルの親")]
        private Transform _skillParent;
        
        [SerializeField, Tooltip("メインカメラ")]
        private Camera _mainCamera;
        [SerializeField, Tooltip("オーバーレイカメラ")]
        private Camera[] _overlayCameras;
        [SerializeField, Tooltip("ズームアニメーション設定")]
        private ZoomAnimationParamSO _zoomAnimationParam;

#if UNITY_EDITOR
        private void OnValidate()
        {
            _mainCamera ??= Camera.main;
            
            if((_overlayCameras == null || _overlayCameras.Length == 0) && _mainCamera != null)
            {
                _overlayCameras = _mainCamera.transform.GetComponentsInChildren<Camera>();
            }
        }
        
#endif
        
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            // プレイヤー
            builder.RegisterInstance(_playerTransform)
                .Keyed("Player");
            
            // カメラ
            var ingameCamera = new CameraManager(_mainCamera, _overlayCameras);
            builder.RegisterInstance<ICameraManager>(ingameCamera);

            // 設定
            builder.RegisterInstance<IInGameSetting>(_inGameSetting);
            
            // タイマー
            builder.Register<ITimer, TimerPresenter>(Lifetime.Singleton)
                .As<IStartable>();
            
            // ステート
            builder.Register<IState, InGameStartState>(Lifetime.Singleton)
                .Keyed(StateType.InGameStart);
            builder.Register<IState, FirstPlayState>(Lifetime.Singleton)
                .Keyed(StateType.FirstPlay);
            builder.Register<IState, PlayFromTitleState>(Lifetime.Singleton)
                .Keyed(StateType.PlayFromTitle);
            builder.Register<IState, InGameState>(Lifetime.Singleton)
                .Keyed(StateType.InGame);
            builder.Register<IState, InGameChangeSceneState>(Lifetime.Singleton)
                .WithParameter(_ => SceneType.GameOverScene)
                .Keyed(StateType.ToGameOver);
            builder.Register<IState, ToResultState>(Lifetime.Singleton)
                .Keyed(StateType.ToResult);

            // プレイヤー入力
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Jump);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Attack);

            builder.RegisterComponentInHierarchy<StickInput>()
                .As<IStartable>()
                .As<ITickable>();
            
            // リザルト
            builder.RegisterComponentInHierarchy<Result>();
            
            // スキル
            builder.RegisterInstance<ISkillRepository>(_skillRepository);
            builder.Register<ISkillSpawner, SkillSpawner>(Lifetime.Singleton)
                .WithParameter("parent", _skillParent);
            builder.Register<ISkillPanel, SkillPanelPresenter>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<SkillRandomActivator>();
            
            // 敵
            builder.RegisterComponentInHierarchy<EnemyRandomActivator>();
            
            // HP
            builder.RegisterComponentInHierarchy<IHeartGroupView>();
            builder.RegisterComponentInHierarchy<HpPresenter>()
                .AsSelf()
                .As<IInitializable>()
                .As<IStartable>();

            // アニメーション
            builder.RegisterInstance<IUIAnimation>(new ZoomAnimation(ingameCamera, _zoomAnimationParam))
                .Keyed(AnimationType.InGameBackground);
            
            // ブースター
            SceneBoosterBinder.Bind(builder, StateType.InGameStart);
        }
    }
}