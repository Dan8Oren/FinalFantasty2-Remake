using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private const int POS_Z_NOT_VISIBLE = 100;
    private const int NUM_ITEMS_IN_A_ROW = 13;
    private const int STARTING_AMOUNT = 5;
    public static InventoryManager Instance = null;
    public GameObject[] DisplayObjects;
    public GameObject pointerPrefab;
    public InventoryItemData[] startingItems;
    [SerializeField] private int capacity;
    
    public Dictionary<InventoryItemDisplay,GameObject> Inventory { get; private set; }
    public GameObject inventorySlotPrefab;
    
    private Dictionary<InventoryItemData, InventoryItemDisplay> _items;
    private Camera _cameraComponent;
    private Transform _transform;
    private void Awake()
    {
        //singleton pattern the prevent two scene managers
        if ( Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject); 
        }
        
        foreach (GameObject obj  in DisplayObjects)
        {
            obj.SetActive(false);
        }

        _cameraComponent = GetComponent<Camera>();
        _transform = transform;
        Inventory = new Dictionary<InventoryItemDisplay,GameObject>();
        Inventory.EnsureCapacity(capacity);
        _items = new Dictionary<InventoryItemData, InventoryItemDisplay>();
        _items.EnsureCapacity(capacity);
        
        //Default items in the inventory;
        InitInventory();
    }

    private void InitInventory()
    {
        foreach (InventoryItemData itemData in startingItems)
        {
            for (int i = 0; i < STARTING_AMOUNT; i++)
            {
                Add(itemData);
            }
        }
    }

    public InventoryItemDisplay GetItem(GameObject displayObj)
    {
        InventoryItemDisplay itemData = displayObj.GetComponent<InventoryItemDisplay>();
        if (itemData != null && Inventory.ContainsKey(itemData))
        {
            return itemData;
        }
        return null;
    }
    
    public void Add(InventoryItemData itemData)
    {
        if (capacity <= Inventory.Count)
        {
            print("INVENTORY IS FULL");
            return;
        }
        if (_items.TryGetValue(itemData,out InventoryItemDisplay value))
        {
            value.IncreaseAmount();
        }
        else
        {
            GameObject newSlot = Instantiate(inventorySlotPrefab,DisplayObjects[0].transform,false);

            InventoryItemDisplay newItem = newSlot.GetComponent<InventoryItemDisplay>();
            newItem.ActivateDisplay(itemData);
            Inventory.Add(newItem,newSlot);
            _items.Add(itemData,newItem);
        }
    }
    
    public void Remove(InventoryItemData itemData)
    {
        if (_items.TryGetValue(itemData,out InventoryItemDisplay value))
        {
            value.DecreaseAmount();
            if (value.Amount == 0)
            {
                value.DisableDisplay();
                Inventory.Remove(value);
                _items.Remove(itemData);
            }
        }
    }

    public void OpenInventory()
    {
        _cameraComponent = Camera.main;
        Vector3 pos = _transform.position;
        pos.z = 0;
        _transform.position = pos;
        foreach (GameObject obj  in DisplayObjects)
        {
            obj.SetActive(true);
        }

        if (PointerBehavior.Instance == null)
        {
            GameObject temp = Instantiate(pointerPrefab);
            PointerBehavior.Instance = temp.GetComponent<PointerBehavior>();
        }
        PointerBehavior.Instance.SetNewObjects(Inventory.Values.ToArray(),NUM_ITEMS_IN_A_ROW,true);
    }
    
    public void CloseInventory()
    {
        Vector3 pos = _transform.position;
        pos.z = POS_Z_NOT_VISIBLE;
        _transform.position = pos;
        foreach (GameObject obj  in DisplayObjects)
        {
            obj.SetActive(false);
        }
    }
}
