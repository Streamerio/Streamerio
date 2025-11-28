using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace InGame.Skill.UI.Icon
{
    [CreateAssetMenu(fileName = "SkillIconDataSO", menuName = "SO/Skill/Icon")]
    public class SkillIconDataSO: ScriptableObject, ISkillIconRepository
    {
        [SerializeField]
        private SerializeDictionary<MasterUltType, SkillIconData> _skillIconDataDict;
        
        public SkillIconData GetSkillIconData(MasterUltType skillType)
        {
            if (_skillIconDataDict.ContainsKey(skillType))
            {
                return _skillIconDataDict[skillType];
            }
            
            Debug.LogError("SkillIconDataSO: Skill icon data not found for type " + skillType);
            return default;
        }
    }
    
    public interface ISkillIconRepository
    {
        SkillIconData GetSkillIconData(MasterUltType skillType);
    }

    [Serializable]
    public struct SkillIconData
    {
        [Tooltip("アイコン画像")]
        public Sprite Icon;
        [Tooltip("アイコンカラー")]
        public Color Color;
        [Tooltip("垂直反転フラグ")]
        public bool IsInversionVertical;
        [Tooltip("水平反転フラグ")]
        public bool IsInversionHorizontal;
        [Tooltip("回転角度")]
        public float Rotation;
        [Tooltip("アスペクト比")]
        public Vector2 AspectRatio;
    }
}