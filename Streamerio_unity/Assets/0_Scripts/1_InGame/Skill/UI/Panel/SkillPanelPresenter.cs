using System.Threading;
using Common;
using R3;

namespace InGame.Skill.UI.Panel
{
    public interface ISkillPanel : IAttachable<SkillPanelContext>
    {
        ISubject<SkillCellData> OnActiveSkillSubject { get; }
    }
    
    public class SkillPanelPresenter: ISkillPanel
    {
        private ISkillPanelView _view;
        private CancellationTokenSource _cts = new ();
        
        private Subject<SkillCellData> _onActiveSkill = new ();
        public ISubject<SkillCellData> OnActiveSkillSubject => _onActiveSkill;
        
        public void Attach(SkillPanelContext context)
        {
            _view = context.View;
            
            Bind();
        }

        private void Bind()
        {
            _onActiveSkill
                .SubscribeAwait(async (data, ct) =>
                {
                    Debug.Log($"SkillPanelPresenter: Show Skill {data.SkillType} by {data.UserName}");
                    await _view.ShowSkillAsync(data.SkillType, data.UserName, ct);
                })
                .RegisterTo(_cts.Token);
        }

        public void Detach()
        {
            _cts.Cancel();
            _cts.Dispose();
        }
    }

    public class SkillPanelContext
    {
        public ISkillPanelView View;
    }
}