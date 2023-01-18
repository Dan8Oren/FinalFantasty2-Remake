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
    private const string RUN_FAILED_LOG = "Run failed";
    public static FightManager Instance { get; private set; }
    public TextMeshProUGUI[] mainFightMenuButtons;
    public int numOfButtonsInARow;
    public DataMenuScript dataMenuScript;
    public ActionsLogScript actionsLogScript;
    // public NextToFightScript nextToFightScript;
    public CharacterData[] EnemiesLevel1;
    public CharacterData[] EnemiesLevel2;
    public CharacterData[] EnemiesLevel3;
    public CharacterData[] EnemiesLevel4;
    public CharacterData[] EnemiesLevel5;
    public CharacterData[][] EnemiesByLevels ;
    private bool _actionHasTaken;
    private bool _isBackEnabled;
    
    [SerializeField] private GameObject pointerPrefab;  //TODO: Remove Me!
    [SerializeField] private CharacterDisplayScript[] enemyRow1;
    [SerializeField] private CharacterDisplayScript[] enemyRow2;
    [SerializeField] private CharacterDisplayScript[] heroRow;
    private int _fightLevel;
    
    private CharacterData[] _heroes;
    
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
    private bool _attackSelected;
    private bool _startFight;

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
        EnemiesByLevels = new []{ EnemiesLevel1,EnemiesLevel2,EnemiesLevel3,EnemiesLevel4,EnemiesLevel5};
        _actionHasTaken = false;
        _isBackEnabled = false;
        _selectedObjName = null;
        _selectedObj = null;
        _curFighterIndex = 0;
        _fightLevel = GameManager.Instance.FightLevel;
        _heroes = GameManager.Instance.GetHeroes();
        SetBattleRow(heroRow, _heroes,0);
        SetBattleRow(enemyRow1, EnemiesByLevels[_fightLevel],0);
        SetBattleRow(enemyRow2, EnemiesByLevels[_fightLevel],enemyRow1.Length);
        _charactersFightOrder = new List<CharacterDisplayScript>();
        _allFighters = new List<CharacterDisplayScript>();
        _allFighters.AddRange(enemyRow1);
        _allFighters.AddRange(enemyRow2);
        _allFighters.AddRange(heroRow);
        PointerBehavior.Instance.transform.SetParent(null);
        PointerBehavior.Instance.gameObject.SetActive(false);
        GetBattleOrder();
        // nextToFightScript.SetFightOrder(_charactersFightOrder);
        _startFight = true; // to call continue fight at update and let other objects to start.
    }

    private void Update()
    {
        if (_startFight)
        {
            ContinueFight();
            _startFight = false;
            return;
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isBackEnabled)
            {
                DoGoBack();
            }

        }
        HandleSelection();
        if (_actionHasTaken)
        {
            if (CheckWinLoss())
            {
                //do win/loss animation.
                MySceneManager.Instance.LoadScene(MySceneManager.Instance.CurrentEntrance,MySceneManager.k_YELLOW_FLOOR);
                return;
            }
            do
            {
                UpdateNextAttacker();
            } while (!_charactersFightOrder[_curFighterIndex].isActiveAndEnabled);
            actionsLogScript.ShowLog();
        }
    }

    public void DoGoBack()
    {
        if (_lastChosenAction == null)
        {
            return;
        }
        if (_lastChosenAction.Equals(MAGIC_TEXT))
        {
            if (_attackSelected)
            {
                actionsLogScript.RemoveLastLog();
                _selectedObjName = null;
                _attackSelected = false;
                MagicMenuScript.Instance.SetPointerToMenu();
                return;
            }
            MagicMenuScript.Instance.CloseMenu();
        }
        else if(_lastChosenAction.Equals(FIGHT_TEXT))
        {
            if (_attackSelected)
            {
                actionsLogScript.RemoveLastLog();
                _selectedObjName = null;
                _attackSelected = false;
                FightMenuScript.Instance.SetPointerToMenu();
                return;
            }
            FightMenuScript.Instance.CloseMenu();
        }
        else if(_lastChosenAction.Equals(ITEMS_TEXT))
        {
            InventoryManager.Instance.CloseInventory();
        }
        _lastChosenAction = null;
        PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons,numOfButtonsInARow);
        StartCoroutine(ActivatePlayerTurn());
    }

    private void HandleSelection()
    {
        if (_selectedObj != null && _lastChosenAction.Equals(ITEMS_TEXT))
        {
            DoItem();
        }
        else if (_selectedObjName != null && _lastChosenAction.Equals(FIGHT_TEXT) && !_attackSelected)
        {
            CharacterData curFighter = _charactersFightOrder[_curFighterIndex].data;
            MeleeAttackData chosenAttack = GetAttack(curFighter);
            Assert.IsFalse(chosenAttack == null);
            actionsLogScript.AddToLog($" uses {chosenAttack.name} to attack");
            float damageCalc = 0;
            damageCalc = (curFighter.attack+chosenAttack.damage) * (1 + curFighter.strength / 100);
            _attackSelected = true;
            _selectedObjName = null;
            //TODO: add multiple enemies?
            SetAttack((int)Mathf.Ceil(damageCalc));
            _actionHasTaken = false;
        }
        else if (_selectedObjName != null && _lastChosenAction.Equals(MAGIC_TEXT) && !_attackSelected)
        {
            CharacterDisplayScript curFighter = _charactersFightOrder[_curFighterIndex];
            MagicAttackData chosenMagic = GetMagic(curFighter.data);
            Assert.IsFalse(chosenMagic == null);
            float damageCalc = Mathf.Ceil(chosenMagic.pointsOfEffect * (1 + curFighter.data.intelligence / 100));
            curFighter.EffectMana(-chosenMagic.manaPointsToConsume); //consume mana
            if (chosenMagic.isOnSelf)
            {
                actionsLogScript.AddToLog($" uses {chosenMagic.characterName} for {damageCalc}");
                curFighter.EffectHealth((int)damageCalc,actionsLogScript);
                MagicMenuScript.Instance.CloseMenu();
                FightMenuScript.Instance.CloseMenu();
                _attackSelected = false;
                _selectedObjName = null;
                return;
            }
            actionsLogScript.AddToLog($" uses {chosenMagic.characterName} to attack");
            _attackSelected = true;
            _selectedObjName = null;
            //TODO: add multiple enemies?
            SetAttack((int)damageCalc);
            _actionHasTaken = false;
        }
        else if (_selectedObjName != null && _attackSelected)
        {
            MagicMenuScript.Instance.CloseMenu();
            FightMenuScript.Instance.CloseMenu();
            DoAttack();
            _attackSelected = false;
            _selectedObjName = null;
        }
    }

    private MagicAttackData GetMagic(CharacterData curFighter)
    {
        foreach (var magic in curFighter.magics)
        {
            if (magic.characterName.Equals(_selectedObjName))
            {
                return magic;
            }
        }
        return null;
    }
    
    private MeleeAttackData GetAttack(CharacterData curFighter)
    {
        foreach (var attack in curFighter.attacks)
        {
            if (attack.name.Equals(_selectedObjName))
            {
                return attack;
            }
        }
        return null;
    }

    private void DoAttack()
    {
        foreach (var fighter in _allFighters)
        {
            if (fighter.isActiveAndEnabled && fighter.name == _selectedObjName)
            {
                actionsLogScript.AddToLog($" {fighter.name} for {_selectedCharDamage} damage");
                fighter.EffectHealth(_selectedCharDamage,actionsLogScript);
                _selectedObjName = null;
                break;
            }
        }
    }

    private void DoItem()
    {
        InventoryItemDisplay res = InventoryManager.Instance.GetItem(_selectedObj);
        Assert.IsFalse(res == null);
        actionsLogScript.AddToLog($" Uses {res.Data.displayName} Health");
        InventoryManager.Instance.Remove(res.Data);
        switch (res.Data.id)
        {
            case InventoryItemData.k_HEALTH_POTION_ID:
                _charactersFightOrder[_curFighterIndex].EffectHealth(res.Data.pointsOfEffect,actionsLogScript);
                actionsLogScript.AddToLog($" {res.Data.pointsOfEffect} Health");
                break;
            case InventoryItemData.k_MANA_POTION_ID:
                _charactersFightOrder[_curFighterIndex].EffectMana(res.Data.pointsOfEffect);
                actionsLogScript.AddToLog($" {res.Data.pointsOfEffect} Mana");
                break;
        }
        
        _selectedObj = null;
        InventoryManager.Instance.CloseInventory();
    }

    private void UpdateNextAttacker()
    {
        _curFighterIndex += 1;
        // nextToFightScript.DisplayNextFighter();
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
        // nextToFightScript.SetFightOrder(_charactersFightOrder);
    }

    /**
     * Checks for win/loss in the battle.
     */
    private bool CheckWinLoss()
    {
        bool activeHero = false, activeEnemy = false;
        foreach (var obj in _charactersFightOrder.ToArray())
        {
            if (obj.isActiveAndEnabled)
            {
                if (obj.data.isHero)
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
            //TODO: Add animations
            
            return true;
        }

        if (activeEnemy == false)
        {
            print("you win");
            //TODO: Add animations
            MySceneManager.Instance.LoadScene(MySceneManager.Instance.CurrentEntrance-1,MySceneManager.Instance.LastSceneName);
            return true;
        }

        return false;
    }
    

    public void ContinueFight()
    {
        CharacterDisplayScript curFighter = _charactersFightOrder[_curFighterIndex];
        dataMenuScript.UpdateDisplay();
        if (_heroes.Contains(curFighter.data))
        {
            actionsLogScript.AddToLog(curFighter.data.name);
            _isBackEnabled = true;
            PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons,numOfButtonsInARow);
            StartCoroutine(ActivatePlayerTurn());
        }
        else
        {
            _isBackEnabled = false;
            actionsLogScript.AddToLog($"{curFighter.data.name}, Enemy number {curFighter.characterNum.text}");
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
            temp.Add(fighter,fighter.data.speed);
            if (i<6)
            {
                _totalEnemiesSpeed += fighter.data.speed;
            }
            _totalHeroesSpeed += fighter.data.speed;
        }
        _charactersFightOrder.AddRange(
            temp.OrderBy(x => -x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray());
    }
    
    private void SetBattleRow(CharacterDisplayScript[] battleRow, CharacterData[] infoRow,int startInd)
    {   
        for (int i = 0; i < battleRow.Length; i++)
        {
            if (i+startInd >= infoRow.Length)
            {
                battleRow[i].gameObject.SetActive(false);
                continue;
            }
            infoRow[i + startInd].ResetStats();
            battleRow[i].SetScriptable(infoRow[i + startInd]);
        }
    }

    private void MakeEnemyAction(CharacterDisplayScript curFighter)
    {
        CharacterData fighter = curFighter.data;
        MagicAttackData[] magics = fighter.magics;
        MeleeAttackData[] attacks = fighter.attacks;
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
            actionsLogScript.AddToLog($" Uses {attacks[actionInd].name}");
            damage = attacks[actionInd].damage;
        }
        else
        {
            int actionInd = Random.Range(0, magics.Length);
            actionsLogScript.AddToLog($" Uses {magics[actionInd].characterName}");
            damage = magics[actionInd].pointsOfEffect;
            if (magics[actionInd].isOnSelf)
            {
                actionsLogScript.AddToLog($" for {damage}");
                curFighter.EffectHealth(damage,actionsLogScript);
                _actionHasTaken = true;
                return;
            }
        }
        //TODO: make an enemy fight tactics at the future.
        int attackIndex = Random.Range(6,_allFighters.Count);
        while (!_allFighters[attackIndex].isActiveAndEnabled)
        {
            attackIndex = Random.Range(6,_allFighters.Count);
        }
        CharacterDisplayScript heroToAttack = _allFighters[attackIndex];
        actionsLogScript.AddToLog($" to attack {heroToAttack.name} for {damage} damage");
        heroToAttack.EffectHealth(-damage,actionsLogScript);
        _actionHasTaken = true;
    }

    public void DoChosenAction(String action)
    {
        CharacterData curFighter = _charactersFightOrder[_curFighterIndex].data;
        switch (action)
        {
            case FIGHT_TEXT:
                FightMenuScript.Instance.ShowMenu(curFighter.attacks);
                break;
            case RUN_TEXT:
                DoRun();
                break;
            case ITEMS_TEXT:
                InventoryManager.Instance.OpenInventory();
                break;
            case MAGIC_TEXT:
                MagicMenuScript.Instance.ShowMenu(curFighter.magics);
                break;
            default:
                _selectedObjName = action;
                return;  //when choosing an attack
        }
        _lastChosenAction = action;
    }

    private void DoRun()
    {
        _actionHasTaken = true;
        int runAttemptValue = Random.Range(0, _totalEnemiesSpeed + _totalHeroesSpeed);
        if (runAttemptValue > _totalEnemiesSpeed)
        {
            MySceneManager.Instance.LoadScene(MySceneManager.Instance.CurrentEntrance,MySceneManager.k_YELLOW_FLOOR);
            return;
        }
        actionsLogScript.AddToLog(RUN_FAILED_LOG);
        _actionHasTaken = true;
    }

    private void SetAttack(int damage) //effectAllGroup?
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
            return;
        }
        _selectedObjName = obj.name;

    }

    private void SelectCharacter(GameObject[] allObjects)
    {
        PointerBehavior.Instance.SetNewObjects(allObjects,3,false);
        StartCoroutine(ActivatePlayerTurn());
        
    }
}
