using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "InventoryItemData",menuName = "ScriptableObjects/InventoryItemData")]
public class InventoryItemData : ScriptableObject
{
    public string id;
    public String displayName;
    public Sprite image;
    public GameObject prefab;
    public int pointsOfEffect;
    
}
