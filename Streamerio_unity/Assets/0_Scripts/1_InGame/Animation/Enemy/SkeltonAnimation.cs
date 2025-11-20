using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class SkeltonAnimation : MonoBehaviour
{
    [SerializeField] SkeltonScriptableObject _skeltonScriptableObject;
    private int _bornDuration;
    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _bornDuration = (int)(_skeltonScriptableObject.StartMoveDelay * 1000);
    }

    void Start()
    {
        PlayBorn().Forget();
    }
    
    public async UniTask PlayBorn()
    {
        await UniTask.Delay(_bornDuration);
        _animator.SetBool("hasBorn", true);
    }
}