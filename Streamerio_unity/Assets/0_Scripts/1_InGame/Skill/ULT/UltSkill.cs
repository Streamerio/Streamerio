using UnityEngine;
using Cysharp.Threading.Tasks;
using InGame.Enemy.Object;

public class UltSkill : MonoBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _damage = 100f;
    [SerializeField] private float _lifetime = 5f;
    void Update()
    {
        Move();
        if (_lifetime <= 0)
        {
            OnDestroy();
        }
        _lifetime -= Time.deltaTime;
    }

    private void Move()
    {
        transform.Translate(Vector2.right * _speed * Time.deltaTime);
    }

    public void OnDestroy()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<IEnemy>();
            if (enemy != null)
            {
                Debug.Log($"UltSkill hit: {collision.gameObject.name}");
                enemy.TakeDamage((int)_damage);
            }
        }
    }
}