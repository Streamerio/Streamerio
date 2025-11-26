using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Common.UI.Dialog.Common
{
    [RequireComponent(typeof(CommonDialogView))]
    public class CommonDialogLifetimeScopeBase<TDialog, TPresenter>: DialogLifetimeScopeBase<TDialog, TPresenter, ICommonDialogView, CommonDialogContext>
        where TDialog : IDialog, IAttachable<CommonDialogContext>
        where TPresenter : TDialog, IStartable
    {
        protected override CommonDialogContext CreateContext(IObjectResolver resolver)
        {
            return new CommonDialogContext()
            {
                View = resolver.Resolve<ICommonDialogView>(),
                Service = resolver.Resolve<IDialogService>(),
            };
        }
    }
}