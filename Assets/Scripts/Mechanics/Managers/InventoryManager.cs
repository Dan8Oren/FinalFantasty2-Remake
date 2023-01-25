using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    private Canvas _canvas;

    private Dictionary<InventoryItemData, InventoryItemDisplay> _items;
    private GameObject _selectedItem;
    private Transform _transform;
    private IEnumerator _wait;

    public bool IsOpen { get; private set; }
    public Dictionary<InventoryItemDisplay, GameObject> Inventory { get; private set; }

    private void Start()
    {
        //singleton pattern the prevent two scene managers
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        foreach (var obj in displayObjects) obj.SetActive(false);

        disableX = IsOpen = false;
        _transform = transform;
        Inventory = new Dictionary<InventoryItemDisplay, GameObject>();
        Inventory.EnsureCapacity(capacity);
        _items = new Dictionary<InventoryItemData, InventoryItemDisplay>();
        _items.EnsureCapacity(capacity);
        _canvas = GetComponent<Canvas>();
        //Default items in the inventory;
        InitInventory();
    }


    private void Update()
    {
        if (_canvas.worldCamera == null) SetToSceneCamera();
    }

    private void InitInventory()
    {
        foreach (var itemData in startingItems)
            for (var i = 0; i < STARTING_AMOUNT; i++)
                Add(itemData);
    }

    /**
     * Gets an item by a given game InventoryDisplay object's data.
     * @returns: The item current data or Null if not found.
     */
    public InventoryItemDisplay GetItem(GameObject displayObj)
    {
        var itemData = displayObj.GetComponent<InventoryItemDisplay>();
        if (itemData != null && Inventory.ContainsKey(itemData)) return itemData;
        return null;
    }

    public void Add(InventoryItemData itemData)
    {
        if (capacity <= Inventory.Count)
        {
            MySceneManager.Instance.messageBoxScript.ShowDialogs(new[] { "INVENTORY IS FULL" }, false);
            return;
        }

        if (_items.TryGetValue(itemData, out var value))
        {
            value.IncreaseAmount();
        }
        else
        {
            var newSlot = Instantiate(inventorySlotPrefab, displayObjects[0].transform, false);

            var newItem = newSlot.GetComponent<InventoryItemDisplay>();
            newItem.ActivateDisplay(itemData);
            Inventory.Add(newItem, newSlot);
            _items.Add(itemData, newItem);
        }
    }

    public void Remove(InventoryItemData itemData)
    {
        if (_items.TryGetValue(itemData, out var value))
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
        SetToSceneCamera();
        var pos = _transform.position;
        pos.z = 0;
        _transform.position = pos;
        foreach (var obj in displayObjects) obj.SetActive(true);

        if (PointerBehavior.Instance == null)
        {
            var temp = Instantiate(pointerPrefab, transform);
            PointerBehavior.Instance = temp.GetComponent<PointerBehavior>();
            PointerBehavior.Instance.enabled = true;
        }

        PointerBehavior.Instance.transform.SetParent(transform);
        PointerBehavior.Instance.gameObject.SetActive(true);
        PointerBehavior.Instance.disableSpace = false;
        PointerBehavior.Instance.SetNewObjects(Inventory.Values.ToArray(), NUM_ITEMS_IN_A_ROW, true);
        if (!MySceneManager.Instance.IsInFight)
        {
            PointerBehavior.Instance.gameObject.transform.localScale = Vector3.one * 60;
            MySceneManager.Instance.HeroRigid.constraints = RigidbodyConstraints2D.FreezeAll;
            _wait = WaitForPlayerToChoose();
            StartCoroutine(_wait);
        }
    }

    private void SetToSceneCamera()
    {
        if (MySceneManager.Instance.IsInFight)
            _canvas.worldCamera = MySceneManager.Instance.fightCamera.GetComponent<Camera>();
        else
            _canvas.worldCamera = Camera.main;
    }

    private IEnumerator WaitForPlayerToChoose()
    {
        yield return new WaitUntil(() => PointerBehavior.Instance.SelectedObj != null);
        _selectedItem = PointerBehavior.Instance.SelectedObj;
        PointerBehavior.Instance.SelectedObj = null;
        GameManager.Instance.OpenHeroesMenu();
        disableX = true;
    }

    public void CloseInventory()
    {
        if (_wait != null) StopCoroutine(_wait);
        var pos = _transform.position;
        pos.z = POS_Z_NOT_VISIBLE;
        _transform.position = pos;
        foreach (var obj in displayObjects) obj.SetActive(false);

        disableX = false;
        IsOpen = false;
        PointerBehavior.Instance.gameObject.SetActive(false);
        if (!MySceneManager.Instance.IsInFight)
            MySceneManager.Instance.HeroRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    public void ApplyItem(HeroInfoDisplay heroDisplay)
    {
        var res = GetItem(_selectedItem);
        var heroData = heroDisplay.GetData();
        switch (res.Data.id)
        {
            case InventoryItemData.k_HEALTH_POTION_ID:
                heroData.currentHp += res.Data.pointsOfEffect;
                if (heroData.currentHp > heroData.MaxHp) heroData.currentHp = heroData.MaxHp;
                break;
            case InventoryItemData.k_MANA_POTION_ID:
                heroData.currentMp += res.Data.pointsOfEffect;
                if (heroData.currentMp > heroData.MaxMp) heroData.currentMp = heroData.MaxMp;
                break;
        }

        heroDisplay.UpdateInfo();
        Remove(res.Data);
    }

    #region Constants

    private const int POS_Z_NOT_VISIBLE = 100;
    private const int NUM_ITEMS_IN_A_ROW = 13;
    private const int STARTING_AMOUNT = 5;

    #endregion

    #region Inspector

    public bool disableX;

    [Tooltip("Inventory prefab needs to be at index 0!")]
    public GameObject[] displayObjects;

    public GameObject pointerPrefab;
    public InventoryItemData[] startingItems;
    public GameObject inventorySlotPrefab;

    [SerializeField] private int capacity;

    #endregion
}