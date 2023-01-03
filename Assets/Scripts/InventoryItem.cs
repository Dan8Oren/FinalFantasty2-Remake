using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class InventoryItem
{
    public InventoryItemData Data { get; private set; }
    public int Amount { get; private set; }

    public InventoryItem(InventoryItemData data)
    {
        Data = data;
        IncreaseAmount();
    }

    public void IncreaseAmount()
    {
        Amount++;
    }
    
    public void DecreaseAmount()
    {
        Amount--;
    }
}