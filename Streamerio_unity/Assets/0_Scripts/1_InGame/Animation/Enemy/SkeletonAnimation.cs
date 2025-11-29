using UnityEngine;

namespace InGame.Enemy.Skelton
{
    [RequireComponent(typeof(Animator))]
    public class SkeletonAnimation : MonoBehaviour
    {
        [SerializeField, Tooltip("アニメーター")]
        private Animator _animator;
        [SerializeField, Tooltip("生成時のアニメーションステート名")]
        private string _bornAnimStateName = "Born";

        /// <summary>
        /// 生成時のアニメーションが終わったか
        /// </summary>
        public bool IsEndBornAnim => !_animator.GetCurrentAnimatorStateInfo(0).IsName(_bornAnimStateName);
    
#if UNITY_EDITOR
        private void OnValidate()
        {
            _animator ??= GetComponent<Animator>();
        }
#endif
    }
}