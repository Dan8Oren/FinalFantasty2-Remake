using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "MagicAttackData",menuName = "ScriptableObjects/MagicData")]
public class MagicAttackData : ScriptableObject
{
    public String displayName;
    public String info;
    public int sequence;
    [Tooltip("if the magic only effect the character who casts it")]
    public bool isOnSelf = false;
    [Tooltip("if the magic effects all of the group, of the character who casts it")]
    public bool effectAllSameGroup = false;
    [Tooltip("if the magic effects all of the enemy group")]
    public bool effectAllEnemyGroup = false;
    public int turnsOfEffect = 1;
    public int pointsOfEffect;
    public int manaPointsToConsume; 
    public int intelligenceRequirement;
    public Animator magicEffect;
}
