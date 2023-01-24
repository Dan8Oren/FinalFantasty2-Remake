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

    [Header("No more than 6 attacks!")] public MeleeAttackData[] attacks;

    [Header("No more than 6 magics!")] public MagicAttackData[] magics;

    [SerializeField] private int beforeStatsMaxHp;
    [SerializeField] private int beforeStatsMaxMp;
    [SerializeField] private int beforeStatsAttack;
    [SerializeField] private int beforeStatsMagic;

    [Tooltip("Increases attack modifier")] public int strength;

    [Tooltip("Increases magic attack modifier")] public int intelligence;

    [Tooltip("Increases HP")] public int stamina;

    [Tooltip("Increases MP")] public int wisdom;

    [Tooltip("chance to take action first increases")] public int speed;
    public int MaxHp { get; private set; }
    public int MaxMp { get; private set; }
    public int currentHp;
    public int currentMp;

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
}