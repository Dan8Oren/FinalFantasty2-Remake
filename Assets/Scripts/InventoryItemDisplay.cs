using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemDisplay : MonoBehaviour
{
    public Image Icon;
    public TextMeshProUGUI amountText;
    public bool IsActive { get; private set; }
    public InventoryItemData Data { get; private set; }
    public int Amount { get; private set; }

    private void Awake()
    {
        Amount = 0;
    }

    public void IncreaseAmount()
    {
        Amount++;
        amountText.SetText(Amount.ToString());
    }
    
    public void DecreaseAmount()
    {
        Amount--;
        amountText.SetText(Amount.ToString());
    }
    public void DisableDisplay()
    {
        Icon.enabled = false;
        amountText.enabled = false;
        IsActive = false;
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
        Icon.enabled = true;
        amountText.enabled = true;
        Icon.sprite = itemData.image;
        IsActive = true;
    }
    
    
}
