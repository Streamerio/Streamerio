namespace Common.UI.Display.Window
{
    public interface IWindowService: IDisplayService { }
    public class WindowService: DisplayService, IWindowService { }
}