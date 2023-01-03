using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "CharacterScriptableObject",menuName = "ScriptableObjects/CharacterData")]
public class CharacterScriptableObject : ScriptableObject
{
    private const int DEFAULT_AMOUNT = 5;
    public bool isHero;
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

    CharacterScriptableObject(int hp,int mp,bool isHero)
    {
        this.isHero = isHero;
        maxHP = hp;
        maxMP = mp;
        currentHP = hp;
        currentMP = mp;
        strength = DEFAULT_AMOUNT; 
        intelligence = DEFAULT_AMOUNT;
        agility = DEFAULT_AMOUNT;
        stamina = DEFAULT_AMOUNT;
        wisdom = DEFAULT_AMOUNT;
        speed = DEFAULT_AMOUNT;
        defence = DEFAULT_AMOUNT;
        attack = DEFAULT_AMOUNT;
        magic = DEFAULT_AMOUNT;
        resistance = 0;
    }
    
}
