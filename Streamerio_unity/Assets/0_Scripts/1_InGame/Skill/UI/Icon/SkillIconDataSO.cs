using System;
using System.Collections.Generic;
using Common;
using UnityEngine;

namespace InGame.Skill.UI.Icon
{
    [CreateAssetMenu(fileName = "SkillIconDataSO", menuName = "SO/Skill/Icon")]
    public class SkillIconDataSO: ScriptableObject
    {
        [SerializeField]
        private SerializeDictionary<MasterUltType, SkillIconData> _skillIconDataDict;
        public IReadOnlyDictionary<MasterUltType, SkillIconData> SkillIconDataDict => _skillIconDataDict.ToDictionary();
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