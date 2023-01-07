using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance = null;
    [SerializeField] public bool isStartOfGame = true;

    public CharacterData[] GetHeroes()
    {
        return _curHeroesContainer.ToArray(); 
    }

    public GameObject[] items;
    public bool isOnFight {get;private set;}
    public int FightLevel
    {
        get { return curLevel; } 
        set{curLevel = value;}
        
    }
    
    [SerializeField] private CharacterData[] allHeroes;
    [SerializeField] private float coins;
    [SerializeField] private int curLevel;
    private Stack<CharacterData> _curHeroesContainer;
    
    private void Awake()
    {
        isOnFight = false;
        //singleton pattern the prevent two scene managers
        if ( Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        FightLevel = 0;
        _curHeroesContainer = new Stack<CharacterData>();
        _curHeroesContainer.Push(allHeroes[0]);
    }

    private void Update()
    {
        isStartOfGame = false;
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
}
