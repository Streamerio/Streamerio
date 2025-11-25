namespace Common.UI.Dialog.Common
{
    public abstract class CommonDialogPresenterBase: DialogPresenterBase<ICommonDialogView, CommonDialogContext>
    {
        
    }
    
    public class CommonDialogContext : DialogContext<ICommonDialogView>
    {
    }
}