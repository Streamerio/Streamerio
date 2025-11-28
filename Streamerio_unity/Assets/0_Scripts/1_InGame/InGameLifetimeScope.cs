using Common.Booster;
using Common.Camera;
using Common.Scene;
using Common.State;
using Common.UI.Animation;
using Common.UI.Part.Button;
using InGame.Goal;
using InGame.Setting;
using InGame.UI.Heart;
using InGame.UI.Timer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace InGame
{
    public class InGameLifetimeScope: LifetimeScope
    {
        [SerializeField]
        private InGameSettingSO _inGameSetting;
        
        [SerializeField]
        private Transform _playerTransform;
        
        [SerializeField]
        private Camera _mainCamera;
        [SerializeField]
        private Camera[] _overlayCameras;
        [SerializeField]
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

            builder.RegisterInstance(_playerTransform)
                .Keyed("Player");
            
            var ingameCamera = new CameraManager(_mainCamera, _overlayCameras);
            builder.RegisterInstance<ICameraManager>(ingameCamera);

            builder.RegisterInstance<IInGameSetting>(_inGameSetting);
            
            builder.Register<ITimer, TimerPresenter>(Lifetime.Singleton)
                .As<IStartable>();
            
            builder.Register<IState, InGameStartState>(Lifetime.Singleton)
                .Keyed(StateType.InGameStart);
            builder.Register<IState, FirstPlayState>(Lifetime.Singleton)
                .Keyed(StateType.FirstPlay);
            builder.Register<IState, PlayFromTitleState>(Lifetime.Singleton)
                .Keyed(StateType.PlayFromTitle);
            builder.Register<IState, InGameState>(Lifetime.Singleton)
                .Keyed(StateType.InGame);
            builder.Register<IState, ChangeSceneState>(Lifetime.Singleton)
                .WithParameter(_ => SceneType.GameOverScene)
                .Keyed(StateType.ToGameOver);
            builder.Register<IState, ToResultState>(Lifetime.Singleton)
                .Keyed(StateType.ToResult);

            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Jump);
            builder
                .Register<ICommonButton, CommonButtonPresenter>(Lifetime.Singleton)
                .Keyed(ButtonType.Attack);

            builder.RegisterComponentInHierarchy<StickInput>()
                .As<IStartable>()
                .As<ITickable>();
            builder.RegisterComponentInHierarchy<Result>();
            builder.RegisterComponentInHierarchy<SkillRandomActivator>();
            builder.RegisterComponentInHierarchy<EnemyRandomActivator>();
            builder.RegisterComponentInHierarchy<IHeartGroupView>();
            builder.RegisterComponentInHierarchy<HpPresenter>()
                .AsSelf()
                .As<IInitializable>()
                .As<IStartable>();

            builder.RegisterInstance<IUIAnimation>(new ZoomAnimation(ingameCamera, _zoomAnimationParam))
                .Keyed(AnimationType.InGameBackground);
            
            SceneBoosterBinder.Bind(builder, StateType.InGameStart);
        }
    }
}