using System.Threading;
using Common.UI.Animation;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using InGame.Skill.UI.Cell;
using UnityEngine;

namespace InGame.Skill.UI.Animation
{
    public class SkillCellAnimation: SequenceAnimationBase
    {
        private readonly ISkillCell[] _cells;
        private readonly SkillCellAnimationSO _param;
        
        public SkillCellAnimation(ISkillCell[] cells, SkillCellAnimationSO param)
        {
            _cells = cells;
            _param = param;

            SetSequence();
        }
        
        public override async UniTask PlayAsync(CancellationToken ct, bool useInitial = true)
        {
            if (useInitial)
            {
                int cellCount = _cells.Length;

                int index;
                for(index=0; index<cellCount-1; index++)
                {
                    _cells[index].RectTransform.anchoredPosition = new Vector2(_param.ShowPosX, _param.MovePosYs[index]);
                }
                _cells[index].RectTransform.anchoredPosition = new Vector2(_param.HidePosX, _param.MovePosYs[index<_param.MovePosYs.Length ? index : ^1]);
            }
            
            await base.PlayAsync(ct, useInitial);
        }

        public override void PlayImmediate()
        {
            int count = _cells.Length;
            _cells[0].RectTransform.anchoredPosition = new Vector2(_param.HidePosX, _param.MovePosYs[0]);
            for(int i=0; i<count-1; i++)
            {
                _cells[i].RectTransform.anchoredPosition = new Vector2(_param.ShowPosX, _param.MovePosYs[i]);
            }
        }

        private void SetSequence()
        {
            int cellCount = _cells.Length;
            if (cellCount <= _param.MovePosYs.Length)
            {
                for(int i=0; i<cellCount; i++)
                {
                    AppendAnim(_cells[i], _param.ShowPosX, _param.MovePosYs[i]);
                }
                
                return;
            }
            
            AppendAnim(_cells[0], _param.HidePosX, _param.MovePosYs[0]);
            for(int i=0; i<cellCount-1; i++)
            {
                AppendAnim(_cells[i+1], _param.ShowPosX, _param.MovePosYs[i]);
            }
        }
        
        private void AppendAnim(ISkillCell cell, float posX, float posY)
        {
            Sequence.Join(cell.RectTransform
                .DOAnchorPos(new Vector2(posX, posY), _param.DurationSec)
                .SetEase(_param.Ease));
            
            Sequence.AppendInterval(_param.DelaySec);
        }
    }
}