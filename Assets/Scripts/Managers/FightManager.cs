using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class FightManager : MonoBehaviour
{
    private const string FIGHT_TEXT = "fight";
    private const string RUN_TEXT = "run";
    private const string ITEMS_TEXT = "items";
    private const string MAGIC_TEXT = "magic";
    private const int FIGHT_ENTRANCE = 4; 
    public static FightManager Instance = null;
    public TextMeshProUGUI[] mainFightMenuButtons;
    public int numOfButtonsInARow;
    
    public CharacterScriptableObject[] EnemiesLevel1;
    public CharacterScriptableObject[][] EnemiesByLevels ;

    private bool _actionHasTaken;
    private bool _isBackEnabled;
    
    [SerializeField] private GameObject pointerPrefab;  //TODO: Remove Me!
    [SerializeField] private CharacterDisplayScript[] enemyRow1;
    [SerializeField] private CharacterDisplayScript[] enemyRow2;
    [SerializeField] private CharacterDisplayScript[] heroRow;
    private int _fightLevel;
    
    private CharacterScriptableObject[] _heroes;
    
    private List<CharacterDisplayScript> _charactersFightOrder;
    private int _curFighterIndex;
    private List<CharacterDisplayScript> _allFighters;
    private string _selectedObjName;
    private GameObject _selectedObj;
    private Stack<String> _actionDepth;
    private int _totalHeroesSpeed;
    private int _totalEnemiesSpeed;
    private int _selectedCharDamage;
    private String _lastChosenAction;

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
        _curFighterIndex = 0;
        _fightLevel = GameManager.Instance.FightLevel;
        _heroes = GameManager.Instance.heroes;
        SetBattleRow(heroRow, _heroes,0);
        SetBattleRow(enemyRow1, EnemiesByLevels[_fightLevel],0);
        SetBattleRow(enemyRow2, EnemiesByLevels[_fightLevel],enemyRow1.Length);
        _charactersFightOrder = new List<CharacterDisplayScript>();
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
            if (_isBackEnabled)
            {
                InventoryManager.Instance.CloseInventory();
                _lastChosenAction = null;
                PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons,numOfButtonsInARow);
                StartCoroutine(ActivatePlayerTurn());
            }

        }
        HandleSelection();
        if (_actionHasTaken)
        {
            if (CheckWinLoss())
            {
                //do win/loss animation.
                MySceneManager.Instance.LoadScene(MySceneManager.Instance.CurrentEntrance,"YellowFloor");
                return;
            }
            do
            {
                UpdateNextAttacker();
            } while (!_charactersFightOrder[_curFighterIndex].isActiveAndEnabled);
            ContinueFight();
        }
    }

    private void HandleSelection()
    {
        if (_selectedObj != null)
        {
            DoItem();
        }
        if (_selectedObjName != null)
        {
            DoAttack();
        }
    }

    private void DoAttack()
    {
        foreach (var fighter in _allFighters)
        {
            if (fighter.isActiveAndEnabled && fighter.name == _selectedObjName)
            {
                fighter.EffectHealth(_selectedCharDamage);
                _selectedObjName = null;
                break;
            }
        }
    }

    private void DoItem()
    {
        InventoryItemDisplay res = InventoryManager.Instance.GetItem(_selectedObj);
        Assert.IsFalse(res == null);
        InventoryManager.Instance.Remove(res.Data);
        //what about mana!?
        _charactersFightOrder[_curFighterIndex].EffectHealth(res.Data.pointsOfEffect);
        _selectedObj = null;
        InventoryManager.Instance.CloseInventory();
    }

    private void UpdateNextAttacker()
    {
        _curFighterIndex += 1;
        if (_curFighterIndex >= _charactersFightOrder.Count)
        {
            _curFighterIndex = 0;
            UpdateFightOrder();
        } 
        _actionHasTaken = false;
    }
    
    /**
     * Removes all dead characters from fight order.
     */
    private void UpdateFightOrder()
    {
        foreach (var obj in _charactersFightOrder.ToArray())
        {
            if (obj.isActiveAndEnabled)
            {
                continue;
            }
            _charactersFightOrder.Remove(obj);
        }
    }

    /**
     * Checks for win/loss in the battle.
     */
    private bool CheckWinLoss()
    {
        bool activeHero = false;
        bool activeEnemy = false;
        foreach (var obj in _charactersFightOrder.ToArray())
        {
            if (obj.isActiveAndEnabled)
            {
                if (obj.scriptableObject.isHero)
                {
                    activeHero = true;
                }
                else
                {
                    activeEnemy = true;
                }
            }
        }

        if (activeHero == false)
        {
            print("you loss");
            return true;
        }

        if (activeEnemy == false)
        {
            print("you win");
            //Do Win
            return true;
        }

        return false;
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
        int i = -1;
        foreach (var fighter in _allFighters)
        {
            i++;
            if (!fighter.isActiveAndEnabled)
            {
                continue;
            }
            temp.Add(fighter,fighter.scriptableObject.speed);
            if (i<6)
            {
                _totalEnemiesSpeed += fighter.scriptableObject.speed;
            }
            _totalHeroesSpeed += fighter.scriptableObject.speed;
        }
        _charactersFightOrder.AddRange(
            temp.OrderBy(x => -x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray());
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
        int attackIndex = Random.Range(6,_allFighters.Count);
        while (!_allFighters[attackIndex].isActiveAndEnabled)
        {
            attackIndex = Random.Range(6,_allFighters.Count);
        }
        CharacterDisplayScript heroToAttack = _allFighters[attackIndex];
        heroToAttack.EffectHealth(-damage);
        print("ENEMY: "+fighter.name+" Hero's health: "+heroToAttack.scriptableObject.currentHP);
        _actionHasTaken = true;
    }

    public void DoChosenAction(String action)
    {
        CharacterScriptableObject curFighter = _charactersFightOrder[_curFighterIndex].scriptableObject;
        float damageCalc = 0;
        _lastChosenAction = action;
        switch (action)
        {
            case FIGHT_TEXT:
                damageCalc = curFighter.attack * (1 + curFighter.strength / 100);
                SetAttack((int)Mathf.Ceil(damageCalc));
                break;
            case RUN_TEXT:
                DoRun();
                break;
            case ITEMS_TEXT:
                InventoryManager.Instance.OpenInventory();
                break;
            case MAGIC_TEXT:
                MagicScriptableObject chosenMagic = SelectMagic(curFighter);
                damageCalc = chosenMagic.pointsOfEffect * (1 + curFighter.intelligence / 100);
                SetAttack((int)Mathf.Ceil(damageCalc));
                break;
        }
    }

    private void DoRun()
    {
        _actionHasTaken = true;
        int runAttemptValue = Random.Range(0, _totalEnemiesSpeed + _totalHeroesSpeed);
        if (runAttemptValue > _totalEnemiesSpeed)
        {
            MySceneManager.Instance.LoadScene(MySceneManager.Instance.CurrentEntrance,"YellowFloor");
            return;
        }
        _actionHasTaken = true;
    }

    private MagicScriptableObject SelectMagic(CharacterScriptableObject curFighter)
    {
        // GameObject magicMenu = Instantiate();
        // PointerBehavior.Instance.SetNewObjects(magicMenu.GetComponentInChildren<GameObject>(),3,false);
        StartCoroutine(ActivatePlayerTurn());
        return curFighter.magics[0];

    }

    private void SetAttack(int damage)
    {
        GameObject[] allFighters = new GameObject[_allFighters.Count];
        int i = 0;
        foreach (CharacterDisplayScript character in _allFighters)
        {
            allFighters[i] = character.gameObject;
            i++;
        }
        StopCoroutine(ActivatePlayerTurn());
        _actionHasTaken = false;
        SelectCharacter(allFighters);
        _selectedCharDamage = -damage; //Minus
    }

    public void SetSelectedObject(GameObject obj)
    {
        _actionHasTaken = true;
        if (_lastChosenAction == ITEMS_TEXT)
        {
            _selectedObj = obj;
        }
        _selectedObjName = obj.name;

    }

    private void SelectCharacter(GameObject[] allObjects)
    {
        PointerBehavior.Instance.SetNewObjects(allObjects,3,false);
        StartCoroutine(ActivatePlayerTurn());
        
    }
}
