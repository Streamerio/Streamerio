using System.Linq;
using Common.UI;
using TMPro;
using UnityEngine;
using VContainer.Unity;

namespace InGame.UI.Timer
{
    public class TimerView: UIBehaviourBase, ITimerView
    {
        [SerializeField]
        private TMP_Text _timerText;

        /// <summary>
        /// ToStringの0埋め
        /// </summary>
        private string _zero;

        public void ZeroSetting(float value)
        {
            int zeroCount = Digit(value);
            char[] zeros = Enumerable.Repeat('0',zeroCount).ToArray();
            _zero = new string(zeros);
        }
        
        /// <summary>
        /// タイマーの見た目更新
        /// </summary>
        /// <param name="value"></param>
        public void UpdateTimerView(float value)
        {
            _timerText.SetText(value.ToString(_zero));
        }
        
        /// <summary>
        /// 桁数取得
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private int Digit(float num)
        {
            return (num == 0) ? 1 : ((int)Mathf.Log10(num) + 1);
        }
    }
    
    public interface ITimerView
    {
        void ZeroSetting(float value);
        void UpdateTimerView(float value);
    }
}