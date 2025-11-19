using UnityEngine;

public class EnemyAttackManager : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    public int CurrentDamage => damage;
}