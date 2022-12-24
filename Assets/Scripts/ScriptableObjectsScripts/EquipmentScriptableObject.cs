using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
[CreateAssetMenu(fileName = "EquipmentScriptableObject",menuName = "ScriptableObjects/EquipmentData")]
public class EquipmentScriptableObject : ScriptableObject
{
    public int defence;
    public int resistance;
    public int attack;
    public int magic;
    public int strengthRequirement; 
    public int intelligenceRequirement;
    public int agilityRequirement;
    public Sprite equipmentImage;
}
