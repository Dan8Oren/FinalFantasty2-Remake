using UnityEngine;

[CreateAssetMenu(fileName = "MagicAttackData", menuName = "ScriptableObjects/MagicData")]
public class MagicAttackData : ScriptableObject
{
    public string displayName;
    public string info;
    public int sequence;

    [Tooltip("if the magic only effect the character who casts it")]
    public bool isOnSelf;

    [Tooltip("if the magic effects all of the group, of the character who casts it")]
    public bool effectAllSameGroup;

    [Tooltip("if the magic effects all of the enemy group")]
    public bool effectAllEnemyGroup;
    
    public int pointsOfEffect;
    public int manaPointsToConsume;
}