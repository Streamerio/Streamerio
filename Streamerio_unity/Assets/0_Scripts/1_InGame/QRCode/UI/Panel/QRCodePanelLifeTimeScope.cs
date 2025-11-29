using VContainer;
using VContainer.Unity;

namespace InGame.QRCode.UI.Panel
{
    public class QRCodePanelLifeTimeScope: LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var view = GetComponent<IQRCodePanelView>();
            builder.RegisterComponent(view);
            
            builder.RegisterEntryPoint<QRCodePanelPresenter>();
        }
    }
}