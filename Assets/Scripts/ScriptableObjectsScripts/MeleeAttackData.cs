using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "MeleeAttackData",menuName = "ScriptableObjects/MeleeAttackData")]
public class MeleeAttackData : ScriptableObject
{
    public String displayName;
    public int sequence;
    public String info;
    public int damage;
    [Tooltip("if the attack effects all of the enemy group")]
    public bool effectAllEnemyGroup = false;
    public int agilityRequirement; 
    public int strengthRequirement;
    public Animator attackEffect;
}
