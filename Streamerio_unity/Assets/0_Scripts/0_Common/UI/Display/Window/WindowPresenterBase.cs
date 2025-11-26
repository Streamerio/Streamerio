using R3;
using UnityEngine;

namespace Common.UI.Display.Window
{
    public interface IWindow : IDisplay
    {
        
    }
    
    /// <summary>
    /// Window 系 UI の Presenter 基底クラス。
    /// - 共通の View 型として CommonWindowView を利用
    /// - 個別の Window Presenter はこのクラスを継承して実装する
    /// - 共通処理は DisplayPresenterBase に集約されているため、
    ///   ここでは特に追加実装はせず「マーカー的役割」を担う
    /// </summary>
    public abstract class WindowPresenterBase<TView, TContext> : DisplayPresenterBase<TView, TContext>, IWindow
        where TView : IWindowView
        where TContext : WindowContext<TView>
    {
        protected override void AttachContext(TContext context)
        {
            View = context.View;
        }

        protected abstract void CloseEvent();
    }

    public class WindowContext<TView>
        where TView : IWindowView
    {
        public TView View;
    }
}