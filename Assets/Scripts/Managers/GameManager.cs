using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    
    public GameObject[] displayObjects;
    public GameObject heroesParent;
    public GameObject heroInfoPrefab;
    public GameObject[] items;
    
    [SerializeField] public bool isStartOfGame = true;
    [SerializeField] private CharacterData[] allHeroes;
    [SerializeField] private float coins;
    [SerializeField] private int curLevel;
    private Stack<CharacterData> _curHeroesContainer;
    private List<GameObject> _heroInfoContainer;
    private Transform _transform;
    private IEnumerator _wait;
    private GameObject _heroInfo;
    private Canvas _canvas;
    private bool _isOpen;
    
    public bool IsOnFight {get;private set;}
    
    public int FightLevel
    {
        get { return curLevel; } 
        set{curLevel = value;}
        
    }

    private void Awake()
    {
        IsOnFight = false;
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
        _isOpen = false;
        FightLevel = 0;
        _curHeroesContainer = new Stack<CharacterData>();
        _heroInfoContainer = new List<GameObject>();
        foreach (var hero in allHeroes)
        {
            hero.ResetStats();
        }
        _canvas = GetComponent<Canvas>();
        AddHeroToGame(0);
    }

    private void AddHeroToGame(int heroIndex)
    {
        Assert.IsFalse(heroIndex >= allHeroes.Length);
        _curHeroesContainer.Push(allHeroes[heroIndex]);
        GameObject heroInfo = Instantiate(heroInfoPrefab, heroesParent.transform);
        HeroInfoScript script = heroInfo.GetComponent<HeroInfoScript>();
        script.SetHeroData(allHeroes[heroIndex]);
        _heroInfoContainer.Add(script.GetCenterObject());
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
        FightLevel++;
        _curHeroesContainer.Push(allHeroes[FightLevel]);
        switch (FightLevel)
        {
            case 1:
               break;
        }
        
    }
    
    public CharacterData[] GetHeroes()
    {
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
        if (!IsOnFight)
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
        InventoryManager.Instance.ApplyItem(_heroInfo.GetComponent<HeroInfoScript>().GetData());
        InventoryManager.Instance.CloseInventory();
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

        StartCoroutine(StallDisableX());

    }

    private IEnumerator StallDisableX()
    {
        yield return new WaitForSeconds(0.1f);
        InventoryManager.Instance.disableX = false;
    }
}
