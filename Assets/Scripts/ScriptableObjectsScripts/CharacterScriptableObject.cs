using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "CharacterScriptableObject",menuName = "ScriptableObjects/CharacterData")]
public class CharacterScriptableObject : ScriptableObject
{
    public Sprite characterImage;
    public AttackScriptableObject[] attacks;
    public MagicScriptableObject[] magics;
    public int maxHP;
    public int maxMP;
    public int currentHP;
    public int currentMP;
    public int strength;  //Increases attack modifier
    public int intelligence;  //Increases magic modifier
    public int agility; //chance to dodge 
    public int stamina; //Increases HP
    public int wisdom; //Increases MP
    public int speed; //chance to take action increases (first and more actions in total)
    public int defence;
    public int resistance;
    public int attack;
    public int magic;
    public EquipmentScriptableObject[] items;

    private void OnEnable()
    {
        foreach (var equipment in items)
        {
            attack += equipment.attack;
            defence += equipment.defence;
            magic += equipment.magic;
            resistance += equipment.resistance;
        }
    }
}
