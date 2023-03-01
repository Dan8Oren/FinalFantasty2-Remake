using System;
using UnityEngine;
using UnityEngine.U2D.Animation;

[CreateAssetMenu(fileName = "CharacterScriptableObject", menuName = "ScriptableObjects/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string displayName;
    public bool isHero;
    public SpriteLibraryAsset spriteLibrary;
    public Sprite idleImage;
    public Sprite characterIcon;

    [Header("No more than 6 attacks!")] public RegularAttackData[] attacks;

    [Header("No more than 6 magics!")] public MagicAttackData[] magics;

    [SerializeField] private int beforeStatsMaxHp;
    [SerializeField] private int beforeStatsMaxMp;
    [SerializeField] private int beforeStatsAttack;
    [SerializeField] private int beforeStatsMagic;

    [Tooltip("Increases attack modifier")] public int strength;

    [Tooltip("Increases magic attack modifier")]
    public int intelligence;

    [Tooltip("Increases HP")] public int stamina;

    [Tooltip("Increases MP")] public int wisdom;

    [Tooltip("chance to take action first increases")]
    public int speed;

    public int currentHp;
    public int currentMp;
    public int MaxHp { get; private set; }
    public int MaxMp { get; private set; }

    /** Additional Melee attack of the character by his current stats.**/
    public int Attack { get; private set; }

    /** Additional Melee attack of the character by his current stats.**/
    public int Magic { get; private set; }

    /**
     * Resets and calculate the health,mana,attack and magic attack of the character by his current stats.
     */
    public void ResetStats()
    {
        MaxHp = (int)Mathf.Ceil(beforeStatsMaxHp * (1 + stamina / 100));
        MaxMp = (int)Mathf.Ceil(beforeStatsMaxMp * (1 + wisdom / 100));
        Attack = (int)Mathf.Ceil(beforeStatsAttack * (1 + strength / 100));
        Magic = (int)Mathf.Ceil(beforeStatsMagic * (1 + intelligence / 100));
        currentHp = MaxHp;
        currentMp = MaxMp;
    }
    
    /**
     * Calculates a Melee fight action points of effect by the player's stats.
     * *Assumes the attack is valid*
     */
    public int CalculateMeleeDamage(RegularAttackData chosenAttack)
    {
        if (chosenAttack.pointsOfEffect < 0) return (int)Mathf.Floor(chosenAttack.pointsOfEffect - Attack);
        return (int)Mathf.Ceil(chosenAttack.pointsOfEffect + Attack);
    }

    /**
     * Calculates a Magic fight action points of effect by the player's stats.
     * *Assumes the attack is valid*
     */
    public int CalculateMagicEffect(MagicAttackData chosenAttack)
    {
        if (chosenAttack.pointsOfEffect < 0) return (int)Mathf.Floor(chosenAttack.pointsOfEffect - Magic);
        return (int)Mathf.Ceil(chosenAttack.pointsOfEffect + Magic);
    }
    
    /**
     * gets the selected magic attack of the current fighter by the name given from the pointer.
     * below is where the name is being set.
     * <see cref="SetSelectedObject" />
     */
    public MagicAttackData GetMagic(String magicName)
    {
        foreach (var magic in magics)
            if (magic.name.Equals(magicName))
                return magic;
        return null;
    }

    /**
     * gets the selected melee attack of the current fighter by the name given from the pointer.
     * below is where the name is being set.
     * <see cref="SetSelectedObject" />
     */
    public RegularAttackData GetAttack(String attackName)
    {
        foreach (var attack in attacks)
            if (attack.name.Equals(attackName))
                return attack;
        return null;
    }
    
    
}