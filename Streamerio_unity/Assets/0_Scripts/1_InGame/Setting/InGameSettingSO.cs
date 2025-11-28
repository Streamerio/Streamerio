using Common.Audio;
using UnityEngine;

namespace InGame.Setting
{
    [CreateAssetMenu(fileName = "InGameSettingSO", menuName = "SO/InGame/Setting")]
    public class InGameSettingSO: ScriptableObject, IInGameSetting
    {
        [SerializeField]
        private BGMType _bgm;
        public BGMType BGM => _bgm;
        
    }
    
    public interface IInGameSetting
    {
        BGMType BGM { get; }
    }
}