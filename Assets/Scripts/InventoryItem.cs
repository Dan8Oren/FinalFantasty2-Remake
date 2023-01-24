using System;

[Serializable]
public class InventoryItem
{
    public InventoryItem(InventoryItemData data)
    {
        Data = data;
        IncreaseAmount();
    }

    public InventoryItemData Data { get; private set; }
    public int Amount { get; private set; }

    public void IncreaseAmount()
    {
        Amount++;
    }

    public void DecreaseAmount()
    {
        Amount--;
    }
}