using UnityEngine;

[CreateAssetMenu(fileName = "MagicAttackData", menuName = "ScriptableObjects/MagicData")]
public class MagicAttackData : AttackData
{
    public int manaPointsToConsume;

    [Tooltip("if the magic only effect the character who casts it")]
    public bool isOnSelf;

    [Tooltip("if the magic effects all of the group, of the character who casts it")]
    public bool effectAllSameGroup;
}