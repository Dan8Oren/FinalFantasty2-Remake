using TMPro;
using UnityEngine;

public class InventoryItemDisplay : MonoBehaviour
{
    public SpriteRenderer icon;
    public TextMeshProUGUI amountText;
    [SerializeField] private int amount;
    public InventoryItemData Data { get; private set; }

    /**
     * Increase the amount and updates the display
     */
    public void IncreaseAmount()
    {
        amount++;
        amountText.SetText(amount.ToString());
    }

    /**
     * Decrease the amount and updates the display
     */
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

    /**
     * Sets the display to show a new item at the inventory by a given InventoryItemData scriptable object.
     */
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