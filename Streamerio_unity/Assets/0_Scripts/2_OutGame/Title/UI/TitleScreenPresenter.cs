using Common;
using Common.State;
using Common.UI.Click;
using Common.UI.Display;
using Common.UI.Display.Window;
using Common.UI.Display.Window.Book;
using R3;

namespace OutGame.Title.UI
{
	public interface ITitleScreen: IDisplay, IAttachable<TitleScreenContext>
	{
	}
	
	public class TitleScreenPresenter : DisplayPresenterBase<ITitleScreenView, TitleScreenContext>, ITitleScreen
	{
		private IClickEventBinder _clickEventBinder;
		
		private IStateManager _stateManager;
		private IState _nextState;

		protected override void AttachContext(TitleScreenContext context)
		{
			View = context.View;
			_clickEventBinder = context.ClickEventBinder;
			_stateManager = context.StateManager;
			_nextState = context.NextState;
		}

		protected override void Bind()
		{
			base.Bind();

			_clickEventBinder.BindClickEvent();
			_clickEventBinder.ClickEvent
				.Subscribe(_ => 
				{
					_stateManager.ChangeState(_nextState);
				})
				.RegisterTo(GetCt());
		}
	}


	public class TitleScreenContext
	{
		public ITitleScreenView View;
		public IClickEventBinder ClickEventBinder;
		public IStateManager StateManager;
		public IState NextState;
	}

}