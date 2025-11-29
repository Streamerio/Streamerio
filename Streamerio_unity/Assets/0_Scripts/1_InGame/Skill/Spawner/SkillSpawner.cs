using System;
using System.Collections.Generic;
using Common;
using Common.Audio;
using InGame.Skill.Object;
using UnityEngine;
using VContainer;

namespace InGame.Skill.Spawner
{
    public interface ISkillSpawner
    {
        /// <summary>
        /// スキルのスポーン
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        IUltSkill Spawn(MasterUltType type);
    }
    
    public class SkillSpawner: IDisposable, ISkillSpawner
    {
        private readonly Transform _parent;
        
        private ISkillRepository _skillObjectRepository;
        private IMasterData _masterData;
        private Transform _player;
        private IAudioFacade _audioFacade;
        
        private Dictionary<MasterUltType, SkillPool> _skillPoolDict = new();
        
        public SkillSpawner(Transform parent)
        {
            _parent = parent;
        }
        
        [Inject]
        public void Construct(ISkillRepository skillObjectRepository, IMasterData masterData, [Key("Player")]Transform player, IAudioFacade audioFacade)
        {
            _skillObjectRepository = skillObjectRepository;
            _masterData = masterData;
            _player = player;
            _audioFacade = audioFacade;
        }
        
        /// <inheritdoc/>
        public IUltSkill Spawn(MasterUltType type)
        {
            // プールが存在しない場合は作成
            if (!_skillPoolDict.ContainsKey(type))
            {
                _skillPoolDict[type] = new SkillPool(
                    _masterData.UltStatusDictionary[type],
                    _skillObjectRepository.GetUltPrefab(type),
                    _parent,
                    _player,
                    _audioFacade
                );
            }

            return _skillPoolDict[type].Get();
        }
        
        public void Dispose()
        {
            foreach (var pool in _skillPoolDict.Values)
            {
                pool.Dispose();
            }
            _skillPoolDict.Clear();
        }
    }
}