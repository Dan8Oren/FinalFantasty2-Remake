using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "MeleeAttackData",menuName = "ScriptableObjects/FightData")]
public class MeleeAttackData : ScriptableObject
{
    public String displayName;
    public int sequence;
    public String info;
    public int turnsOfEffect = 1;
    public int damage;
    public int hits = 1; //how many hits of the above damage occurs 
    public int agilityRequirement; 
    public int strengthRequirement;
    public Animator attackEffect;
}
