using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "MagicAttackData",menuName = "ScriptableObjects/MagicData")]
public class MagicAttackData : ScriptableObject
{
    public String name;
    public bool isOnSelf = false; //if the magic only effect the character who casts it
    public bool isOnSameGroup = false;//if the magic effects the group of character who casts it
    public bool effectAllGroup = false;
    public int turnsOfEffect = 1;
    public int pointsOfEffect;
    public int manaPointsToConsume; 
    public int intelligenceRequirement;
    public Animator magicEffect;
}
