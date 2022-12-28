using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FightManager : MonoBehaviour
{
    private const string FIGHT_TEXT = "fight";
    private const string RUN_TEXT = "run";
    private const string ITEMS_TEXT = "items";
    private const string MAGIC_TEXT = "magic";
    public static FightManager Instance = null;
    public TextMeshProUGUI[] mainFightMenuButtons;
    public int numOfButtonsInARow;
    
    public CharacterScriptableObject[] EnemiesLevel1;
    public CharacterScriptableObject[][] EnemiesByLevels ;
    public enum Rows{Enemy1,Enemy2,Heroes};

    private bool _actionHasTaken;
    private bool _isBackEnabled;
    [SerializeField] private GameObject pointerPrefab;
    [SerializeField] private CharacterDisplayScript[] enemyRow1;
    [SerializeField] private CharacterDisplayScript[] enemyRow2;
    [SerializeField] private CharacterDisplayScript[] heroRow;
    private int _fightLevel;
    
    private CharacterScriptableObject[] _heroes;
    
    private CharacterDisplayScript[] _charactersFightOrder;
    private int _curFighterIndex;
    private int _lastFighterIndex;
    private List<CharacterDisplayScript> _allFighters;
    private string _selectedObjName;
    private Stack<String> _actionDepth;

    private void Awake()
    {
        //singleton pattern the prevent two pointers
        if (Instance == null || Instance == this)
        {
            Instance = this;
            return;
        }
        Destroy(gameObject);
    }
    
    private void Start()
    {
        EnemiesByLevels = new []{ EnemiesLevel1 };
        _actionHasTaken = false;
        _isBackEnabled = false;
        _lastFighterIndex = 0;
        _curFighterIndex = 0;
        _fightLevel = GameManager.Instance.FightLevel;
        _heroes = GameManager.Instance.heroes;
        SetBattleRow(heroRow, _heroes,0);
        SetBattleRow(enemyRow1, EnemiesByLevels[_fightLevel],0);
        SetBattleRow(enemyRow2, EnemiesByLevels[_fightLevel],enemyRow1.Length);
        _allFighters = new List<CharacterDisplayScript>();
        _allFighters.AddRange(enemyRow1);
        _allFighters.AddRange(enemyRow2);
        _allFighters.AddRange(heroRow);
        PointerBehavior.Instance.gameObject.SetActive(false);
        GetBattleOrder();
        ContinueFight();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("Back!");
            if (_isBackEnabled)
            {
                PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons,numOfButtonsInARow);
                StartCoroutine(ActivatePlayerTurn());
            }

        }
    }

    private void ContinueFight()
    {
        CharacterScriptableObject curFighter = _charactersFightOrder[_curFighterIndex].scriptableObject;
        if (_heroes.Contains(curFighter))
        {
            _isBackEnabled = true;
            PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons,numOfButtonsInARow);
            StartCoroutine(ActivatePlayerTurn());
        }
        else
        {
            _isBackEnabled = false;
            MakeEnemyAction(curFighter);
        }

        if (_actionHasTaken)
        {
            _curFighterIndex += 1;
            if (_curFighterIndex >= _charactersFightOrder.Length)
            {
                _curFighterIndex = 0;
            }
            _actionHasTaken = false;
        }
    }


    private IEnumerator ActivatePlayerTurn()
    {
        PointerBehavior.Instance.gameObject.SetActive(true);
        yield return new WaitUntil(() => _actionHasTaken);
        PointerBehavior.Instance.gameObject.SetActive(false);
    }
    
    private void GetBattleOrder()
    {
        Dictionary<CharacterDisplayScript, int> temp = new Dictionary<CharacterDisplayScript, int>();
        
        foreach (var fighter in _allFighters)
        {
            if (!fighter.isActiveAndEnabled)
            {
                continue;
            }
            temp.Add(fighter,fighter.scriptableObject.speed);
        }
        //need to reverse?
        _charactersFightOrder = 
            temp.OrderBy(x => -x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray();
    }
    
    private void SetBattleRow(CharacterDisplayScript[] battleRow, CharacterScriptableObject[] infoRow,int startInd)
    {   
        for (int i = 0; i < battleRow.Length; i++)
        {
            if (i+startInd >= infoRow.Length)
            {
                battleRow[i].gameObject.SetActive(false);
                continue;
            }
            battleRow[i].SetScriptable(infoRow[i + startInd]);
        }
    }

    private void MakeEnemyAction(CharacterScriptableObject fighter)
    {
        MagicScriptableObject[] magics = fighter.magics;
        AttackScriptableObject[] attacks = fighter.attacks;
        int magicOrAttack = Random.Range(0, 2);
        if (fighter.magics.Length == 0)
        {
            magicOrAttack = 0;
        }
        if (fighter.attacks.Length == 0)
        {
            magicOrAttack = 1;
        }
        
        int damage = 0;
        if (magicOrAttack == 0)
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
        print("ENEMY: "+fighter.name+"Hero's health: "+heroToAttack.currentHP);
        _actionHasTaken = true;
    }

    public void DoChosenAction(String action)
    {
        CharacterScriptableObject curFighter = _charactersFightOrder[_curFighterIndex].scriptableObject;
        _actionHasTaken = true;
        float damageCalc = 0;
        switch (action)
        {
            case FIGHT_TEXT:
                damageCalc = curFighter.attack * (1 + curFighter.strength / 100);
                DoAttack((int)Mathf.Ceil(damageCalc));
                break;
            case RUN_TEXT:
                
                break;
            case ITEMS_TEXT:
                
                break;
            case MAGIC_TEXT:
                MagicScriptableObject chosenMagic = SelectMagic(curFighter);
                damageCalc = chosenMagic.pointsOfEffect * (1 + curFighter.intelligence / 100);
                DoAttack((int)Mathf.Ceil(damageCalc));
                break;
        }
    }

    private MagicScriptableObject SelectMagic(CharacterScriptableObject curFighter)
    {
        // GameObject magicMenu = Instantiate();
        // PointerBehavior.Instance.SetNewObjects(magicMenu.GetComponentInChildren<GameObject>(),3,false);
        StartCoroutine(ActivatePlayerTurn());
        return curFighter.magics[0];

    }

    private void DoAttack(int damage)
    {
        GameObject[] allFighters = new GameObject[_allFighters.Count];
        int i = 0;
        foreach (CharacterDisplayScript character in _allFighters)
        {
            allFighters[i] = character.gameObject;
            i++;
        }

        _actionHasTaken = false;
        CharacterDisplayScript toAttack = SelectCharacter(allFighters);
        // toAttack.Hit(damage);
    }

    public void SetSelectedObject(String name)
    {
        _actionHasTaken = true;
        _selectedObjName = name;
    }
    
    private CharacterDisplayScript SelectCharacter(GameObject[] allObjects)
    {
        PointerBehavior.Instance.SetNewObjects(allObjects,3,false);
        StartCoroutine(ActivatePlayerTurn());
        foreach (var fighter in _allFighters)
        {
            if (fighter.isActiveAndEnabled && fighter.name == _selectedObjName)
            {
                return fighter;
            }
        }
        return null;
    }
}
