using System;
using Common;
using Common.Audio;
using UnityEngine;
using UnityEngine.Pool;

namespace InGame.Skill.Spawner
{
    public class SkillPool: IDisposable
    {
        private readonly MasterUltStatus _skillStatus;
        private readonly IUltSkill _skillPrefab;
        
        private readonly Transform _parent;
        
        private readonly ObjectPool<IUltSkill> _pool;

        private readonly Transform _player;
        private readonly IAudioFacade _audioFacade;
        
        public SkillPool(MasterUltStatus skillStatus, IUltSkill skillPrefab, Transform parent, Transform player, IAudioFacade audioFacade)
        {
            _skillStatus = skillStatus;
            _skillPrefab = skillPrefab;
            _parent = parent;
            _player = player;
            _audioFacade = audioFacade;

            _pool = new ObjectPool<IUltSkill>(
                createFunc: OnCreateSkill,
                actionOnGet: OnGetSkill,
                defaultCapacity: _skillStatus.PoolSize
            );
        }
        
        /// <summary>
        /// 敵の取得
        /// </summary>
        /// <returns></returns>
        public IUltSkill Get()
        {
            return _pool.Get();
        }
        
        /// <summary>
        /// オブジェクトプールで生成するときの処理
        /// </summary>
        /// <returns></returns>
        private IUltSkill OnCreateSkill()
        {
            if (_skillPrefab is not MonoBehaviour prefabComponent)
            {
                throw new InvalidOperationException("Skill prefab must be a MonoBehaviour implementing IUltSkill.");
            }

            // プレハブをインスタンス化し、IUltSkill を取得
            var instanceComponent = UnityEngine.Object.Instantiate(prefabComponent, _parent);
            if (!instanceComponent.TryGetComponent<IUltSkill>(out var instance))
            {
                Debug.LogError("The instantiated skill does not implement IUltSkill.");
                return null;
            }

            instance.OnCreate(_skillStatus.AttackPower, () => _pool.Release(instance), _player, _audioFacade);

            return instance;
        }

        /// <summary>
        /// オブジェクトプールから取得したときの処理
        /// </summary>
        /// <param name="skill"></param>
        private void OnGetSkill(IUltSkill skill)
        {
            skill.Initialize();
        }
        
        public void Dispose()
        {
            _pool.Clear();
        }
    }
}
