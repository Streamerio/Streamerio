using Common.State;
using Common.UI.Display.Window;
using Common.UI.Part.Text;
using UnityEngine;
using VContainer;

namespace OutGame.Result.UI
{
    public class ResultWindowLifetimeScope: WindowLifetimeScopeBase<IResultWindow, ResultWindowPresenter, IResultWindowView, ResultWindowContext>
    {
        [SerializeField]
        private FlashTextBinder _clickTextBinder;
        
        protected override void Configure(IContainerBuilder builder)
        {
            _clickTextBinder.Bind(builder);
            
            base.Configure(builder);
        }
        
        protected override ResultWindowContext CreateContext(IObjectResolver resolver)
        {
            return new ResultWindowContext
            {
                View = resolver.Resolve<IResultWindowView>(),
                StateManager = resolver.Resolve<IStateManager>(),
                NextState = resolver.Resolve<IState>(StateType.ToTitle),
            };
        }
    }
}