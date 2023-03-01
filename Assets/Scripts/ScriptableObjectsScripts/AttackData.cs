using UnityEngine;

public class AttackData : ScriptableObject
{
    public string displayName;
    public int sequence;
    public string info;
    public int pointsOfEffect;

    [Tooltip("if the attack effects all of the enemy group")]
    public bool effectAllEnemyGroup;
}
