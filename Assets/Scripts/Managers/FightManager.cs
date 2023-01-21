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
    private const string NOT_ENOUGH_MANA = "Not Enough Mana";
    private const string FAILED_SEQUENCE_MSG = "\n !! but, sequence was failed !!";
    public static FightManager Instance { get; private set; }
    public int SelectedSequenceLevel { get; private set; }
    public TextMeshProUGUI[] mainFightMenuButtons;
    public int numOfButtonsInARow;
    public DataMenuScript dataMenuScript;
    public ActionsLogScript actionsLogScript;
    public MessageBoxScript messageBox;
    public CharacterData[] EnemiesLevel1;
    public CharacterData[] EnemiesLevel2;
    public CharacterData[] EnemiesLevel3;
    public CharacterData[] EnemiesLevel4;
    public CharacterData[] EnemiesLevel5;
    public CharacterData[][] EnemiesByLevels ;

    [SerializeField] private CharacterDisplayScript[] enemyRow1;
    [SerializeField] private CharacterDisplayScript[] enemyRow2;
    [SerializeField] private CharacterDisplayScript[] heroRow;
    [SerializeField] private float timeBetweenTurns = 0.7f;
    [SerializeField] private bool isFightOver;
    [SerializeField] private int _curFighterIndex;
    
    private int _fightLevel;
    private CharacterData[] _heroes;
    private List<CharacterDisplayScript> _charactersFightOrder;
    
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
    private bool _finishedSequence;
    private bool _actionHasTaken;
    private bool _isBackEnabled;
    

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
        InitializeValues();
        SetBattleRow(heroRow, _heroes,0);
        SetBattleRow(enemyRow1, EnemiesByLevels[_fightLevel],0);
        SetBattleRow(enemyRow2, EnemiesByLevels[_fightLevel],enemyRow1.Length);
        _allFighters = new List<CharacterDisplayScript>();
        _allFighters.AddRange(enemyRow1);
        _allFighters.AddRange(enemyRow2);
        _allFighters.AddRange(heroRow);
        InitializePointer();
        GetBattleOrder();
        _startFight = true; // to call continue fight at update and let other objects to start.
    }

    private static void InitializePointer()
    {
        PointerBehavior.Instance.transform.SetParent(null);
        PointerBehavior.Instance.gameObject.SetActive(false);
    }

    private void InitializeValues()
    {
        EnemiesByLevels = new[] { EnemiesLevel1, EnemiesLevel2, EnemiesLevel3, EnemiesLevel4, EnemiesLevel5 };
        _actionHasTaken = false;
        _isBackEnabled = false;
        isFightOver = false;
        _selectedObjName = null;
        _selectedObj = null;
        _curFighterIndex = 0;
        _finishedSequence = false;
        _fightLevel = GameManager.Instance.CurFightLevel;
        _heroes = GameManager.Instance.GetHeroes();
        _charactersFightOrder = new List<CharacterDisplayScript>();
    }

    private void Update()
    {
        if (isFightOver)
        {
            return;
        }
        if (_startFight)
        {
            ContinueFight();
            _startFight = false;
            return;
        }
        if (Input.GetKeyDown(KeyCode.X) && _finishedSequence)
        {
            if (_isBackEnabled)
            {
                DoGoBack();
            }

        }
        HandleSelection();
        if (_actionHasTaken && _finishedSequence)
        {
            _finishedSequence = false;
            StartCoroutine(WaitForAnimation(_charactersFightOrder[_curFighterIndex].AnimateEndTurn()));
            UpdateRound();
            StartCoroutine(NextTurn());
        }
    }

    private IEnumerator NextTurn()
    {
        yield return new WaitForSeconds(timeBetweenTurns);
        actionsLogScript.ShowLog();
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
            HandleAttackSelected();
        }
        else if (_selectedObjName != null && _lastChosenAction.Equals(MAGIC_TEXT) && !_attackSelected)
        {
            HandleMagicSelected();
        }
        else if (_selectedObjName != null && _attackSelected)
        {
            _finishedSequence = false;
            StartCoroutine(DoAttack());
            CloseAll();
        }
    }
    
    private void HandleAttackSelected()
    {
        CharacterData curFighter = _charactersFightOrder[_curFighterIndex].data;
        MeleeAttackData chosenAttack = GetAttack(curFighter);
        Assert.IsFalse(chosenAttack == null);
        float damageCalc = 0;
        damageCalc = curFighter.Attack + chosenAttack.damage;
        SelectedSequenceLevel = chosenAttack.sequence;
        if (chosenAttack.effectAllEnemyGroup)
        {
            actionsLogScript.AddToLog($" uses {chosenAttack.displayName}");
            _actionHasTaken = true;
            StartCoroutine(DoEffectAll(-(int)Mathf.Ceil(damageCalc), false));
            CloseAll();
            return;
        }
        _attackSelected = true;
        actionsLogScript.AddToLog($" uses {chosenAttack.displayName} to attack");
        _selectedObjName = null;
        SetAttack((int)Mathf.Ceil(damageCalc),false);
        _actionHasTaken = false;
    }

    private void HandleMagicSelected()
    {
        CharacterDisplayScript curFighter = _charactersFightOrder[_curFighterIndex];
        MagicAttackData chosenMagic = GetMagic(curFighter.data);
        Assert.IsFalse(chosenMagic == null);
        if (curFighter.data.currentMp < chosenMagic.manaPointsToConsume)
        {
            _actionHasTaken = false;
            actionsLogScript.ClearLog();
            actionsLogScript.messageBox.ShowDialogs(new string[] { NOT_ENOUGH_MANA }, false);
            CloseAll();
            return;
        }
        SelectedSequenceLevel = chosenMagic.sequence;
        float damageCalc = Mathf.Ceil(chosenMagic.pointsOfEffect + curFighter.data.Magic);
        if (chosenMagic.isOnSelf||chosenMagic.effectAllSameGroup)
        {
            curFighter.EffectMana(-chosenMagic.manaPointsToConsume); //consume mana
            actionsLogScript.AddToLog($" uses {chosenMagic.displayName} for {damageCalc}");
            if (chosenMagic.effectAllSameGroup) {
                _finishedSequence = false;
                StartCoroutine(DoEffectAll((int)damageCalc, true));
            }else
            {
                _finishedSequence = false;
                StartCoroutine(DoEffectSelf((int)damageCalc,curFighter));
            }
            _actionHasTaken = true;
            CloseAll();
            return;
        }

        actionsLogScript.AddToLog($" uses {chosenMagic.displayName} to attack");
        _attackSelected = true;
        _selectedObjName = null;
        SetAttack((int)damageCalc,chosenMagic.effectAllEnemyGroup);
        _actionHasTaken = false;
    }

    private void CloseAll()
    {
        MagicMenuScript.Instance.CloseMenu();
        FightMenuScript.Instance.CloseMenu();
        _attackSelected = false;
        _selectedObjName = null;
    }

    private MagicAttackData GetMagic(CharacterData curFighter)
    {
        foreach (var magic in curFighter.magics)
        {
            if (magic.name.Equals(_selectedObjName))
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

    private IEnumerator DoAttack()
    {
        SequenceManager.Instance.isSequenceActive = true;
        SequenceManager.Instance.StartSequenceByLevel(SelectedSequenceLevel);
        String nameToAttack = _selectedObjName;
        yield return new WaitUntil(()=>!SequenceManager.Instance.isSequenceActive);
        if (SequenceManager.Instance.IsGood)
        {
            foreach (var fighter in _allFighters)
            {
                if (fighter.isActiveAndEnabled && fighter.name == nameToAttack)
                {
                    _allFighters[_curFighterIndex].AnimateAttack();
                    actionsLogScript.AddToLog($" '{fighter.data.displayName}'" +
                                              $" for {Mathf.Abs(_selectedCharDamage)} damage");
                    fighter.EffectHealth(_selectedCharDamage,actionsLogScript);
                    _selectedObjName = null;
                    break;
                }
            }
        }
        else
        {
            actionsLogScript.AddToLog(FAILED_SEQUENCE_MSG);
        }
        _finishedSequence = true;
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
    private void CheckWinLoss()
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
            StartCoroutine(DoGameLoss());
            isFightOver = true;
        }

        if (activeEnemy == false)
        {
            StartCoroutine(DoGameWon());
            isFightOver = true;
        }
    }

    private IEnumerator DoGameLoss()
    {
        TextMeshPro text = SequenceManager.Instance.textDisplay;
        text.gameObject.SetActive(true);
        text.SetText("You Died! \n Game Over");
        yield return new WaitForSeconds(3f);
        Destroy(GameManager.Instance);
        Destroy(MySceneManager.Instance);
        Destroy(InventoryManager.Instance);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator DoGameWon()
    {
        TextMeshPro text = SequenceManager.Instance.textDisplay;
        text.gameObject.SetActive(true);
        text.SetText("You Won \n All enemies are dead!");
        float timeToWait = 5f; //temporary
        foreach (var obj in _charactersFightOrder.ToArray())
        {
            if (obj.isActiveAndEnabled)
            {
                if (obj.data.isHero)
                {
                    timeToWait = obj.AnimateGameWon();
                }
            }
        }
        GameManager.Instance.FightWon();
        yield return new WaitForSeconds(timeToWait);
        MySceneManager.Instance.LoadNormalScene(MySceneManager.Instance.CurrentEntrance+1,MySceneManager.Instance.LastSceneName);
    }


    public void ContinueFight()
    {
        CheckWinLoss();
        if (isFightOver)
        {
            return;
        }
        CharacterDisplayScript curFighter = _charactersFightOrder[_curFighterIndex];
        StartCoroutine(WaitForAnimation(curFighter.AnimateTurn()));
        dataMenuScript.UpdateDisplay();
        if (_heroes.Contains(curFighter.data))
        {
            actionsLogScript.AddToLog("'"+curFighter.data.displayName+"'");
            _isBackEnabled = true;
            PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons,numOfButtonsInARow);
            StartCoroutine(ActivatePlayerTurn());
        }
        else
        {
            _isBackEnabled = false;
            actionsLogScript.AddToLog($"'{curFighter.data.displayName}' - (Enemy {curFighter.characterNum.text}),");
            MakeEnemyAction(curFighter);
        }
    }

    private void UpdateRound()
    {
        CheckWinLoss();
        do
        {
            UpdateNextAttacker();
        } while (!_charactersFightOrder[_curFighterIndex].isActiveAndEnabled);
    }

    private IEnumerator WaitForAnimation(float time)
    {
        yield return new WaitForSeconds(time);
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

            if (!infoRow[i + startInd].isHero)
            {
                infoRow[i + startInd].ResetStats();
            }
            battleRow[i].SetScriptable(infoRow[i + startInd]);
        }
    }

    private void MakeEnemyAction(CharacterDisplayScript curFighter)
    {
        CharacterData fighter = curFighter.data;
        int damage = RandomizeEnemyAttack(fighter,fighter.magics,fighter.attacks);
        if (damage == 0)
        {
            return;
        }
        int attackIndex = Random.Range(6,_allFighters.Count);
        while (!_allFighters[attackIndex].isActiveAndEnabled)
        {
            attackIndex = Random.Range(6,_allFighters.Count);
        }
        CharacterDisplayScript heroToAttack = _allFighters[attackIndex];
        actionsLogScript.AddToLog($" to attack '{heroToAttack.data.displayName}' for {damage} damage");
        heroToAttack.EffectHealth(-damage,actionsLogScript);
        _actionHasTaken = true;
        _finishedSequence = true;
    }

    private int RandomizeEnemyAttack(CharacterData fighter,
        MagicAttackData[] magics,MeleeAttackData[] attacks)
    {
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
            actionsLogScript.AddToLog($" Uses {attacks[actionInd].displayName}");
            damage = attacks[actionInd].damage;
        }
        else
        {
            int actionInd = Random.Range(0, magics.Length);
            actionsLogScript.AddToLog($" Uses {magics[actionInd].displayName}");
            damage = magics[actionInd].pointsOfEffect;
            if (magics[actionInd].isOnSelf)
            {
                actionsLogScript.AddToLog($" for {damage}");
                _allFighters[_curFighterIndex].EffectHealth(-damage,actionsLogScript);
                _finishedSequence = true;
                _actionHasTaken = true;
                return 0;
            }

            if (magics[actionInd].effectAllEnemyGroup)
            {
                actionsLogScript.AddToLog($"to attack all heroes for {damage} damage");
                foreach (var character in _charactersFightOrder)
                {
                    if (character.data.isHero)
                    {
                        character.EffectHealth(-damage,actionsLogScript);
                    }
                }
                _actionHasTaken = true;
                _finishedSequence = true;
                return 0;
            }
        }
        return damage;
    }

    public void DoChosenAction(String action)
    {
        CharacterData curFighter = _charactersFightOrder[_curFighterIndex].data;
        switch (action)
        {
            case FIGHT_TEXT: 
                FightMenuScript.Instance.ShowMenu(curFighter);
                break;
            case RUN_TEXT:
                DoRun();
                break;
            case ITEMS_TEXT:
                InventoryManager.Instance.OpenInventory();
                break;
            case MAGIC_TEXT:
                MagicMenuScript.Instance.ShowMenu(curFighter);
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
            MySceneManager.Instance.LoadNormalScene(MySceneManager.Instance.CurrentEntrance,MySceneManager.k_YELLOW_FLOOR);
            return;
        }
        actionsLogScript.AddToLog(RUN_FAILED_LOG);
        _actionHasTaken = true;
    }

    private void SetAttack(int damage,bool isEffectAllEnemyGroup)
    {
        if (isEffectAllEnemyGroup)
        {
            _finishedSequence = false;
            StartCoroutine(DoEffectAll(-damage,false));
            return;
        }
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
        _selectedCharDamage = -damage; //Minus + saved for after player selected an enemy.
    }
    
    /**
     * Effects all enemies or heroes health by the given amount. 
     * amount - amount to effect all health with. (can be negative)
     * isOnHeroes - flag to determine which group to effect.
     */
    private IEnumerator DoEffectAll(int amount,bool isOnHeroes)
    {
        SequenceManager.Instance.isSequenceActive = true;
        SequenceManager.Instance.StartSequenceByLevel(SelectedSequenceLevel);
        yield return new WaitUntil(()=>!SequenceManager.Instance.isSequenceActive);
        if (SequenceManager.Instance.IsGood)
        {
            String groupToAttack = "enemies!";
            if (isOnHeroes)
            {
                groupToAttack = "heroes!";
            }
            actionsLogScript.AddToLog($" to Effects All {groupToAttack} for {Mathf.Abs(amount)} points!");
            foreach (var fighter in _charactersFightOrder)
            {
                if (isOnHeroes == fighter.data.isHero)
                {
                    fighter.EffectHealth(amount,actionsLogScript);
                }
            }
        }
        else
        {
            actionsLogScript.AddToLog(FAILED_SEQUENCE_MSG);
        }
        _finishedSequence = true;
    }

    private IEnumerator DoEffectSelf(int amount, CharacterDisplayScript caster)
    {
        SequenceManager.Instance.isSequenceActive = true;
        SequenceManager.Instance.StartSequenceByLevel(SelectedSequenceLevel);
        yield return new WaitUntil(()=>!SequenceManager.Instance.isSequenceActive);
        if (SequenceManager.Instance.IsGood)
        {
            caster.EffectHealth(amount,actionsLogScript);
        }else {
            actionsLogScript.AddToLog(FAILED_SEQUENCE_MSG);
        }
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
        _finishedSequence = true;
    }

    private void SelectCharacter(GameObject[] allObjects)
    {
        PointerBehavior.Instance.SetNewObjects(allObjects,3,false);
        StartCoroutine(ActivatePlayerTurn());
        
    }
}
