using System;
using Common.UI.Animation;
using UnityEngine;

namespace InGame.Skill.UI.Animation
{
    [CreateAssetMenu(fileName = "SkillCellAnimationSO", menuName = "SO/UI/Animation/MoveCell")]
    public class SkillCellAnimationSO: UIAnimationComponentParamSO
    {
        [Tooltip("表示位置X座標")]
        public float ShowPosX = 0f;
        [Tooltip("非表示位置X座標")]
        public float HidePosX = 400f;
        
        [Tooltip("移動Y座標配列")]
        public float[] MovePosYs;
        
        [Tooltip("移動の間隔(秒)")]
        public float DelaySec;
    }
}