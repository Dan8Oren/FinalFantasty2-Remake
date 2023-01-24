using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackData", menuName = "ScriptableObjects/MeleeAttackData")]
public class MeleeAttackData : ScriptableObject
{
    public string displayName;
    public int sequence;
    public string info;
    public int damage;

    [Tooltip("if the attack effects all of the enemy group")]
    public bool effectAllEnemyGroup;
}