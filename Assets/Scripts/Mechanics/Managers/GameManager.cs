using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[DefaultExecutionOrder(-999)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int CurFightLevel { get; private set; }

    private void Awake()
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
        CompletedFightLevels = new HashSet<int>();
        CurFightLevel = 0;
        _isOpen = false;
        _curHeroesContainer = new Stack<CharacterData>();
        _heroInfoContainer = new List<GameObject>();
        foreach (var hero in allHeroes) hero.ResetStats();
        _canvas = GetComponent<Canvas>();
        AddHeroToGame(allHeroes.Length - 1); //add warrior
    }
    

    private void Update()
    {
        isStartOfGame = false;
        if (Input.GetKeyDown(KeyCode.X) && _isOpen) CloseHeroesMenu();
    }

    /**
     * adds a new hero to the game fight scene by a given index of the hero at the hero's database.
     */
    private void AddHeroToGame(int heroIndex)
    {
        Assert.IsFalse(heroIndex >= allHeroes.Length);
        _curHeroesContainer.Push(allHeroes[heroIndex]);
        var heroInfo = Instantiate(heroInfoPrefab, heroesParent.transform);
        var display = heroInfo.GetComponent<HeroInfoDisplay>();
        display.SetHeroData(allHeroes[heroIndex]);
        _heroInfoContainer.Add(heroInfo);
    }

    /**
     * Updates the game in accordance to the current fight won.
     */
    public void FightWon()
    {
        CompletedFightLevels.Add(CurFightLevel);
        switch (CurFightLevel)
        {
            case 0:
                AddHeroToGame(KNIGHT_INDEX);
                break;
            case 1:
                AddHeroToGame(MAGICIAN_INDEX);
                break;
        }
    }

    /**
     * Gets the current active hero's in game.
     */
    public CharacterData[] GetHeroes()
    {
        return _curHeroesContainer.ToArray();
    }

    public void OpenHeroesMenu()
    {
        _isOpen = true;
        _canvas.worldCamera = Camera.main;
        foreach (var obj in displayObjects) obj.SetActive(true);
        PointerBehavior.Instance.gameObject.SetActive(true);
        PointerBehavior.Instance.enabled = true;
        PointerBehavior.Instance.transform.SetParent(transform);
        PointerBehavior.Instance.SetNewObjects(_heroInfoContainer.ToArray(), 4, true);
        if (!MySceneManager.Instance.IsInFight)
        {
            _wait = WaitForPlayerToChoose();
            StartCoroutine(_wait);
        }
    }

    /**
     * Sets a delay for the player to decide his actions and handles it accordingly.
     */
    private IEnumerator WaitForPlayerToChoose()
    {
        yield return new WaitUntil(() => PointerBehavior.Instance.SelectedObj != null);
        _heroInfo = PointerBehavior.Instance.SelectedObj;
        PointerBehavior.Instance.SelectedObj = null;
        CloseHeroesMenu();
        InventoryManager.Instance.enabled = true;
        var infoDisplay = _heroInfo.GetComponent<HeroInfoDisplay>();
        Assert.IsFalse(infoDisplay == null);
        InventoryManager.Instance.ApplyItem(infoDisplay);
        PointerBehavior.Instance.gameObject.SetActive(false);
    }

    public void CloseHeroesMenu()
    {
        _isOpen = false;
        StopCoroutine(_wait);
        foreach (var obj in displayObjects) obj.SetActive(false);
        InventoryManager.Instance.disableX = false;
        InventoryManager.Instance.CloseInventory();
    }

    public void SetFightLevel(int fightLevel)
    {
        if (fightLevel == HEALER_INDEX &&
            ((_heroInfoContainer.Count == 3 && CompletedFightLevels.Contains(KNIGHT_INDEX)) ||
             _heroInfoContainer.Count == 2)) AddHeroToGame(HEALER_INDEX);
        CurFightLevel = fightLevel;
    }

    #region Constants

    private const int KNIGHT_INDEX = 0;
    private const int MAGICIAN_INDEX = 1;
    private const int HEALER_INDEX = 2;

    #endregion

    #region Inspector

    public GameObject[] displayObjects;
    public GameObject heroesParent;
    public GameObject heroInfoPrefab;
    public GameObject[] items;
    public HashSet<int> CompletedFightLevels;


    [SerializeField] public bool isStartOfGame = true;
    [SerializeField] private CharacterData[] allHeroes;
    [SerializeField] private float coins;

    #endregion

    #region Private Fields

    private Stack<CharacterData> _curHeroesContainer;
    private List<GameObject> _heroInfoContainer;
    private Transform _transform;
    private IEnumerator _wait;
    private GameObject _heroInfo;
    private Canvas _canvas;
    private bool _isOpen;

    #endregion
}