using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CharacterScriptableObject",menuName = "ScriptableObjects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public String displayName;
    public bool isHero;
    public Sprite idleImage;
    public Sprite characterIcon;
    [Header("No more than 6 attacks!")]
    public MeleeAttackData[] attacks;
    [Header("No more than 6 magics!")]
    public MagicAttackData[] magics;
    public int MaxHp { get; private set; }
    public int MaxMp { get; private set; }
    
    [SerializeField] private int beforeStatsMaxHp;
    [SerializeField] private int beforeStatsMaxMp;
    public int currentHp;
    public int currentMp;
    [Tooltip("Increases attack modifier")]
    public int strength;
    [Tooltip("Increases magic attack modifier")]
    public int intelligence;
    [Tooltip("Increases chance to dodge")]
    public int agility;
    [Tooltip("Increases HP, and chance to block")]
    public int stamina;
    [Tooltip("Increases MP, and chance to deflect")]
    public int wisdom;
    [Tooltip("chance to take action first increases")]
    public int speed;
    [Tooltip("decreases damage taken")]
    public int defence;
    
    [SerializeField] private int beforeStatsAttack;
    public int Attack { get; private set; }
    [SerializeField] private int beforeStatsMagicMp;
    public int Magic { get; private set; }
    
    public EquipmentData[] items;
    
    public void ResetStats()
    {
        MaxHp = (int)Mathf.Ceil(beforeStatsMaxHp * (1 + (stamina / 100)));
        MaxMp = (int)Mathf.Ceil(beforeStatsMaxMp * (1 + (wisdom / 100)));
        Attack = (int)Mathf.Ceil(beforeStatsAttack * (1 + (strength / 100)));
        Magic = (int)Mathf.Ceil(beforeStatsAttack * (1 + (Magic / 100)));
        currentHp = MaxHp;
        currentMp = MaxMp;
    }
    
    
}
