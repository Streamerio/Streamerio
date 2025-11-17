using Common;
using Common.Audio;
using Common.Booster;
using Common.Network;
using Common.QRCode;
using Common.Save;
using Common.Scene;
using Common.State;
using Common.UI.Dialog;
using Common.UI.Display.Overlay;
using Common.UI.Display.Window;
using Common.UI.Loading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class GlobalLifetimeScope : LifetimeScope
{
    [SerializeField]
    private MasterDataSO _masterDataSO;
    [SerializeField]
    private ApiConfigSO _apiConfigSO;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterEntryPoint<GlobalBooster>();

        builder.Register<INetworkConnection, NetworkConnection>(Lifetime.Singleton);
        
        builder.RegisterInstance<IMasterData>(_masterDataSO);
        
        builder.Register<IVolumeSaveFacade, IPlayDataSaveFacade, SaveManager>(Lifetime.Singleton);

        builder.Register<IAudioFacade, AudioFacade>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<AudioManager>();
        
        builder.Register<SceneManager>(Lifetime.Singleton)
            .AsImplementedInterfaces();
        
        builder.Register<ILoadingScreen, LoadingScreenPresenter>(Lifetime.Singleton);
        
        builder.Register<IStateManager, StateManager>(Lifetime.Singleton);
        
        builder.Register<IQRCodeService, QRCodeService>(Lifetime.Singleton)
            .WithParameter(new QRCodeSpriteFactory());

        builder.Register<IWindowService, WindowService>(Lifetime.Singleton);
        builder.Register<IOverlayService, OverlayService>(Lifetime.Singleton);
        builder.Register<IDialogService, DialogService>(Lifetime.Singleton);
        builder.Register<IWebSocketManager, WebSocketManager>(Lifetime.Singleton)
# if UNITY_EDITOR
    .As<ITickable>()
# endif
    .WithParameter(_ => _apiConfigSO);

        //builder.RegisterEntryPoint<TestWindow>();
        //builder.RegisterEntryPoint<TestOverlay>();
        //builder.RegisterEntryPoint<DialogTest>();
        //builder.RegisterEntryPoint<LoadingTest>();
    }
}
