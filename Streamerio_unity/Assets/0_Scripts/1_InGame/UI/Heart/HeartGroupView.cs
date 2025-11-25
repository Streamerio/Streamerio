using Alchemy.Inspector;
using Common.UI;
using UnityEngine;

namespace InGame.UI.Heart
{
    public class HeartGroupView: UIBehaviourBase, IHeartGroupView
    {
        [SerializeField, ReadOnly]
        private HeartIcon[] _heartIcons;
        
        private int _heartLength;
        private float _multiply;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if(_heartIcons.Length == 0)
               _heartIcons = GetComponentsInChildren<HeartIcon>();
        }
#endif
        
        /// <summary>
        /// 初期化
        /// </summary>
        /// <param name="minHp"></param>
        /// <param name="maxHp"></param>
        public void Initialize(float minHp, float maxHp)
        {
            _heartLength = _heartIcons.Length;
            _multiply = _heartLength / (maxHp - minHp);
        }
        
        
        /// <summary>
        /// HPの更新
        /// </summary>
        /// <param name="value"></param>
        public void UpdateHP(float value)
        {
            float heartValue = value * _multiply;

            for (int i = 0; i < _heartLength; i++)
            {
                _heartIcons[i].SetValue(heartValue);
                heartValue -= HeartIcon.MaxValue;
            }
        }
    }
    
    public interface IHeartGroupView
    {
        void Initialize(float minHp, float maxHp);
        void UpdateHP(float value);
    }
}