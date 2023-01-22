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
    public bool disableX;
    public bool IsOpen { get; private set; }
    public GameObject[] DisplayObjects;
    public GameObject pointerPrefab;
    public InventoryItemData[] startingItems;
    [SerializeField] private int capacity;
    
    public Dictionary<InventoryItemDisplay,GameObject> Inventory { get; private set; }
    public GameObject inventorySlotPrefab;
    
    private Dictionary<InventoryItemData, InventoryItemDisplay> _items;
    private Transform _transform;
    private IEnumerator _wait;
    private GameObject _selectedItem;
    private Canvas _canvas;

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

        disableX = IsOpen = false;
        _transform = transform;
        Inventory = new Dictionary<InventoryItemDisplay,GameObject>();
        Inventory.EnsureCapacity(capacity);
        _items = new Dictionary<InventoryItemData, InventoryItemDisplay>();
        _items.EnsureCapacity(capacity);
        _canvas = GetComponent<Canvas>();
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
            MySceneManager.Instance.messageBoxScript.ShowDialogs(new String[]{"INVENTORY IS FULL"},false);
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
            if (value.GetAmount() == 0)
            {
                value.DisableDisplay();
                Inventory.Remove(value);
                _items.Remove(itemData);
            }
        }
    }

    public void OpenInventory()
    {
        IsOpen = true;
        if (MySceneManager.Instance.IsInFight)
        {
            _canvas.worldCamera = MySceneManager.Instance.fightCamera.GetComponent<Camera>();
        }
        else
        {
            _canvas.worldCamera = Camera.main;
        }
        
        Vector3 pos = _transform.position;
        pos.z = 0;
        _transform.position = pos;
        foreach (GameObject obj  in DisplayObjects)
        {
            obj.SetActive(true);
        }

        if (PointerBehavior.Instance == null)
        {
            GameObject temp = Instantiate(pointerPrefab,transform);
            PointerBehavior.Instance = temp.GetComponent<PointerBehavior>();
            PointerBehavior.Instance.enabled = true;
        }
        PointerBehavior.Instance.transform.SetParent(transform);
        PointerBehavior.Instance.gameObject.SetActive(true);
        PointerBehavior.Instance.SetNewObjects(Inventory.Values.ToArray(),NUM_ITEMS_IN_A_ROW,true);
        if (!MySceneManager.Instance.IsInFight)
        {
            MySceneManager.Instance.HeroRigid.constraints = RigidbodyConstraints2D.FreezeAll;
            _wait = WaitForPlayerToChoose();
            StartCoroutine(_wait);
        }
    }
    
    private IEnumerator WaitForPlayerToChoose()
    {
        yield return new WaitUntil(() => (PointerBehavior.Instance.SelectedObj != null));
        _selectedItem = PointerBehavior.Instance.SelectedObj;
        PointerBehavior.Instance.SelectedObj = null;
        GameManager.Instance.OpenHeroesMenu();
        disableX = true;
    }
    
    public void CloseInventory()
    {
        if (_wait!= null)
        {
            StopCoroutine(_wait);
        }
        Vector3 pos = _transform.position;
        pos.z = POS_Z_NOT_VISIBLE;
        _transform.position = pos;
        foreach (GameObject obj  in DisplayObjects)
        {
            obj.SetActive(false);
        }

        disableX = false;
        IsOpen = false;
        PointerBehavior.Instance.gameObject.SetActive(false);
        if (!MySceneManager.Instance.IsInFight)
        {
            MySceneManager.Instance.HeroRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public void ApplyItem(CharacterData heroData)
    {
        InventoryItemDisplay res = GetItem(_selectedItem);
        switch (res.Data.id)
        {
            case InventoryItemData.k_HEALTH_POTION_ID:
                heroData.currentHp += res.Data.pointsOfEffect;
                break;
            case InventoryItemData.k_MANA_POTION_ID:
                heroData.currentMp += res.Data.pointsOfEffect;
                break;
        }
        Remove(res.Data);
    }
}
