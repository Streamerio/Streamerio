using Cysharp.Threading.Tasks;
using UnityEngine;

public class BulletShooter : MonoBehaviour
{
    [SerializeField] private BulletPool _bulletPool;
    [SerializeField] private Transform _firePoint;
    private PlayerAnimation _animator;

    void Awake()
    {
        _animator = GetComponent<PlayerAnimation>();
    }

    public async void Shoot()
    {
        _animator.PlayAttack(1);
        await UniTask.WaitForSeconds(0.5f);
        NormalBullet bullet = _bulletPool.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = _firePoint.position;
            bullet.transform.rotation = _firePoint.rotation;
            bullet.gameObject.SetActive(true);
            bullet.OnSpawn();
        }
        else
        {
            Debug.LogError("Failed to retrieve bullet from pool.");
        }
    }
}