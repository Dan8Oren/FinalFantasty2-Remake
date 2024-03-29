using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItemData", menuName = "ScriptableObjects/InventoryItemData")]
public class InventoryItemData : ScriptableObject
{
    public const int k_HEALTH_POTION_ID = 0;
    public const int k_MANA_POTION_ID = 1;

    public int id;
    public string displayName;
    public Sprite image;
    public int pointsOfEffect;
}