using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDisplay : MonoBehaviour
{
    public SpriteRenderer icon;
    public TextMeshProUGUI amountText;
    public InventoryItemData Data { get; private set; }
    [SerializeField] private int amount = 0;

    public void IncreaseAmount()
    {
        amount++;
        amountText.SetText(amount.ToString());
    }
    
    public void DecreaseAmount()
    {
        amount--;
        amountText.SetText(amount.ToString());
    }
    public void DisableDisplay()
    {
        icon.enabled = false;
        amountText.enabled = false;
    }

    public void ActivateDisplay(InventoryItemData itemData)
    {
        if (itemData == null)
        {
            DisableDisplay();
            return;
        }

        Data = itemData;
        IncreaseAmount();
        icon.enabled = true;
        amountText.enabled = true;
        icon.sprite = itemData.image;
    }

    public int GetAmount()
    {
        return amount;
    }
    
}
