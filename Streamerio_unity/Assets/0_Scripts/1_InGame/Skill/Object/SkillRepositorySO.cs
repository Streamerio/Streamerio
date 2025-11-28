using Common;
using UnityEngine;

namespace InGame.Skill.Object
{
    [CreateAssetMenu(fileName = "SkillRepositorySO", menuName = "SO/Skill/Repository")]
    public class SkillRepositorySO: ScriptableObject, ISkillRepository
    {
        [SerializeField]
        private SerializeDictionary<MasterUltType, GameObject> _ultPrefabDict = new SerializeDictionary<MasterUltType, GameObject>();
        
        public IUltSkill GetUltPrefab(MasterUltType ultType)
        {
            if (_ultPrefabDict.ContainsKey(ultType))
            {
                return _ultPrefabDict[ultType].GetComponent<IUltSkill>();
            }
            
            Debug.LogError($"Ult Prefab not found for type: {ultType}");
            return null;
        }
    }
    
    public interface ISkillRepository
    {
        /// <summary>
        /// ウルトのプレファブを取得する
        /// </summary>
        /// <param name="ultType"></param>
        /// <returns></returns>
        IUltSkill GetUltPrefab(MasterUltType ultType);
    }
}