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
        
        [SerializeField]
        private float _timeLimit = 300f;
        public float TimeLimit => _timeLimit;
        
    }
    
    public interface IInGameSetting
    {
        BGMType BGM { get; }
        float TimeLimit { get; }
    }
}