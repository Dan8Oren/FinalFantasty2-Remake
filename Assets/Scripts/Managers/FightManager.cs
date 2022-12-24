using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FightManager : MonoBehaviour
{
    public CharacterScriptableObject[][] EnemiesByLevels;
    public enum Rows{Enemy1,Enemy2,Heroes};

    public bool ActionHasTaken{get; set;}
    [SerializeField] private GameObject[] enemyRow1;
    [SerializeField] private GameObject[] enemyRow2;
    [SerializeField] private GameObject[] heroRow;
    private int _fightLevel;
    private CharacterScriptableObject[] _heroes;
    private CharacterScriptableObject[] _charactersFightOrder;
    private int _curFighterIndex;
    private int _lastFighterIndex;
    private void Start()
    {
        ActionHasTaken = false;
        _lastFighterIndex = 0;
        _curFighterIndex = 0;
        _fightLevel = GameManager.Instance.fightLevel;
        _heroes = GameManager.Instance.heroes;
        SetBattleRow(heroRow, _heroes,0);
        SetBattleRow(enemyRow1, EnemiesByLevels[_fightLevel],0);
        SetBattleRow(enemyRow2, EnemiesByLevels[_fightLevel],enemyRow1.Length-1);
        GetBattleOrder();
        ContinueFight();
    }

    private void Update()
    {
        if (_lastFighterIndex != _curFighterIndex)
        {
            ContinueFight();
        }
    }

    private void ContinueFight()
    {
        CharacterScriptableObject curFighter = _charactersFightOrder[_curFighterIndex];
        if (_heroes.Contains(curFighter))
        {
            StartCoroutine(ActivatePlayerTurn(curFighter));
        }
        else
        {
            MakeEnemyAction(curFighter);
        }

        if (ActionHasTaken)
        {
            _curFighterIndex += 1;
            if (_curFighterIndex >= _charactersFightOrder.Length)
            {
                _curFighterIndex = 0;
            }
            ActionHasTaken = false;
        }
    }


    private IEnumerator ActivatePlayerTurn(CharacterScriptableObject curFighter)
    {
        yield return new WaitUntil(() => ActionHasTaken);
    }
    
    private void GetBattleOrder()
    {
        Dictionary<CharacterScriptableObject, int> temp = new Dictionary<CharacterScriptableObject, int>();
        var allFighters = new List<CharacterScriptableObject>();
        allFighters.AddRange(_heroes);
        allFighters.AddRange(EnemiesByLevels[_fightLevel]);
        foreach (var fighter in allFighters)
        {
            temp.Add(fighter,fighter.speed);
        }
        _charactersFightOrder = 
            temp.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();
        
    }
    
    private void SetBattleRow(GameObject[] battleRow, CharacterScriptableObject[] infoRow,int startInd)
    {   
        for (int i = 0; i < battleRow.Length; i++)
        {
            if (i >= startInd+infoRow.Length)
            {
                battleRow[i].SetActive(false);
                continue;
            }
            
            battleRow[i].GetComponent<Image>().sprite = infoRow[i + startInd].characterImage;
        }
    }

    private void MakeEnemyAction(CharacterScriptableObject fighter)
    {
        MagicScriptableObject[] magics = fighter.magics;
        AttackScriptableObject[] attacks = fighter.attacks;
        int magicOrAttack = Random.Range(0, 2);
        int damage = 0;
        if (magicOrAttack == 0 && attacks.Length != 0)
        {
            int actionInd = Random.Range(0, attacks.Length);
            damage = attacks[actionInd].damage;
        }
        else
        {
            int actionInd = Random.Range(0, magics.Length);
            damage = magics[actionInd].pointsOfEffect;
        }
        //TODO: make an enemy fight tactics at the future.
        CharacterScriptableObject heroToAttack = _heroes[Random.Range(0, _heroes.Length)];
        heroToAttack.currentHP -= damage;
        ActionHasTaken = true;
    }
    
}
