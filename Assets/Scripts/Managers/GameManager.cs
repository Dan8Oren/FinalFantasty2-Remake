using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    private const int KNIGHT_INDEX = 0;
    private const int MAGICIAN_INDEX = 1;
    private const int HEALER_INDEX = 2;

    public static GameManager Instance = null;
    public int CurFightLevel { get; private set;}
    public GameObject[] displayObjects;
    public GameObject heroesParent;
    public GameObject heroInfoPrefab;
    public GameObject[] items;
    public HashSet<int> CompletedFightLevels;
    
    
    [SerializeField] public bool isStartOfGame = true;
    [SerializeField] private CharacterData[] allHeroes;
    [SerializeField] private float coins;
    private Stack<CharacterData> _curHeroesContainer;
    private List<GameObject> _heroInfoContainer;
    private Transform _transform;
    private IEnumerator _wait;
    private GameObject _heroInfo;
    private Canvas _canvas;
    private bool _isOpen;

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
    }

    private void Start()
    {
        CompletedFightLevels = new HashSet<int>();
        CurFightLevel = 0;
        _isOpen = false;
        _curHeroesContainer = new Stack<CharacterData>();
        _heroInfoContainer = new List<GameObject>();
        foreach (var hero in allHeroes)
        {
            hero.ResetStats();
        }
        _canvas = GetComponent<Canvas>();
        AddHeroToGame(allHeroes.Length-1); //add warrior
    }

    private void AddHeroToGame(int heroIndex)
    {
        Assert.IsFalse(heroIndex >= allHeroes.Length);
        _curHeroesContainer.Push(allHeroes[heroIndex]);
        GameObject heroInfo = Instantiate(heroInfoPrefab, heroesParent.transform);
        HeroInfoScript script = heroInfo.GetComponent<HeroInfoScript>();
        script.SetHeroData(allHeroes[heroIndex]);
        _heroInfoContainer.Add(heroInfo);
    }

    private void Update()
    {
        isStartOfGame = false;
        if (Input.GetKeyDown(KeyCode.X) && _isOpen)
        {
            CloseHeroesMenu();
        }
    }

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
    
    public CharacterData[] GetHeroes()
    {
        if (_curHeroesContainer == null)
        {
            Start();
        }
        return _curHeroesContainer.ToArray(); 
    }

    public void OpenHeroesMenu()
    {
        _isOpen = true;
        _canvas.worldCamera = Camera.main;
        foreach (GameObject obj  in displayObjects)
        {
            obj.SetActive(true);
        }
        PointerBehavior.Instance.gameObject.SetActive(true);
        PointerBehavior.Instance.enabled = true;
        PointerBehavior.Instance.transform.SetParent(transform);
        PointerBehavior.Instance.SetNewObjects(_heroInfoContainer.ToArray(),4,true);
        if (!MySceneManager.Instance.IsInFight)
        {
            _wait = WaitForPlayerToChoose();
            StartCoroutine(_wait);
        }
    }
    
    private IEnumerator WaitForPlayerToChoose()
    {
        yield return new WaitUntil(() => (PointerBehavior.Instance.SelectedObj != null));
        _heroInfo = PointerBehavior.Instance.SelectedObj;
        PointerBehavior.Instance.SelectedObj = null;
        CloseHeroesMenu();
        InventoryManager.Instance.enabled = true;
        HeroInfoScript infoScript = _heroInfo.GetComponent<HeroInfoScript>();
        Assert.IsFalse(infoScript == null);
        CharacterData data = infoScript.GetData();
        Assert.IsFalse(data == null);
        InventoryManager.Instance.ApplyItem(data);
        PointerBehavior.Instance.gameObject.SetActive(false);
    }

    public void CloseHeroesMenu()
    {
        
        _isOpen = false;
        StopCoroutine(_wait);
        foreach (GameObject obj  in displayObjects)
        {
            obj.SetActive(false);
        }
        InventoryManager.Instance.disableX = false;
        InventoryManager.Instance.CloseInventory();

    }

    public void SetFightLevel(int fightLevel)
    {
        if (fightLevel == HEALER_INDEX)
        {
            AddHeroToGame(HEALER_INDEX);
        }
        CurFightLevel = fightLevel;
    }
}
