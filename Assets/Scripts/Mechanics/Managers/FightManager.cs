using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class FightManager : MonoBehaviour
{
    public static FightManager Instance { get; private set; }

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
        SetEnemyRowDisplay(heroRow, _heroes, 0);
        SetEnemyRowDisplay(enemyRow1, _enemiesByLevels[_fightLevel], 0);
        SetEnemyRowDisplay(enemyRow2, _enemiesByLevels[_fightLevel], enemyRow1.Length);
        _allFighters = new List<CharacterDisplayScript>();
        _allFighters.AddRange(enemyRow1);
        _allFighters.AddRange(enemyRow2);
        _allFighters.AddRange(heroRow);
        PointerBehavior.Instance.gameObject.SetActive(false);
        SetBattleOrder();
        _startFight = true; // to call continue fight at update and let other objects to start.
    }

    private void Update()
    {
        if (isFightOver) return;
        if (_startFight)
        {
            ContinueFight();
            _startFight = false;
            return;
        }

        if (Input.GetKeyDown(KeyCode.X) && !_actionHasTaken)
            if (_isBackEnabled)
                DoGoBack();
        HandleSelection();
        if (_actionHasTaken && _finishedSequence)
        {
            _finishedSequence = false;
            StartCoroutine(WaitForAnimation(_charactersFightOrder[curFighterIndex].AnimateEndTurn,
                () =>
                {
                    UpdateRound();
                    StartCoroutine(NextTurn());
                }
            ));
        }
    }

    private void InitializeValues()
    {
        messageBox.gameObject.SetActive(false);
        _enemiesByLevels = new[] { enemiesLevel1, enemiesLevel2, enemiesLevel3 };
        _actionHasTaken = false;
        _isBackEnabled = false;
        isFightOver = false;
        _selectedObjName = null;
        _selectedObj = null;
        curFighterIndex = 0;
        _finishedSequence = false;
        _fightLevel = GameManager.Instance.CurFightLevel;
        _heroes = GameManager.Instance.GetHeroes();
        _charactersFightOrder = new List<CharacterDisplayScript>();
    }

    /**
     * Waits for the turn animation to end and displays new messages according to the current turn's actions.
     * after that plays the next turn at the fight.
     */
    private IEnumerator NextTurn()
    {
        yield return new WaitForSeconds(timeBetweenTurns);
        actionsLogScript.ShowLog();
    }

    /**
     * Sets the menu display the one step backwords.
     */
    public void DoGoBack()
    {
        if (_lastChosenAction == null) return;
        if (_lastChosenAction.Equals(MAGIC_TEXT))
        {
            if (HandleGoBackAtMagic()) return;
        }
        else if (_lastChosenAction.Equals(FIGHT_TEXT))
        {
            if (HandleGoBackAtFight()) return;
        }
        else if (_lastChosenAction.Equals(ITEMS_TEXT))
        {
            InventoryManager.Instance.CloseInventory();
        }

        _lastChosenAction = null;
        PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons, numOfButtonsInARow);
        StartCoroutine(ActivatePlayerTurn());
    }

    private bool HandleGoBackAtFight()
    {
        if (_attackSelected)
        {
            actionsLogScript.RemoveLastLog();
            _selectedObjName = null;
            _attackSelected = false;
            FightMenuScript.Instance.SetPointerToMenu();
            return true;
        }

        FightMenuScript.Instance.CloseMenu();
        return false;
    }

    private bool HandleGoBackAtMagic()
    {
        if (_attackSelected)
        {
            actionsLogScript.RemoveLastLog();
            _selectedObjName = null;
            _attackSelected = false;
            MagicMenuScript.Instance.SetPointerToMenu();
            return true;
        }

        MagicMenuScript.Instance.CloseMenu();
        return false;
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
            if (_lastChosenAction.Equals(MAGIC_TEXT))
                _charactersFightOrder[curFighterIndex].EffectMana(-_manaToConsume); //consume mana
            _finishedSequence = false;
            StartCoroutine(DoAttack());
            CloseAll();
        }
    }

    /**
     * Updates the fight by the current attack selected.
     */
    private void HandleAttackSelected()
    {
        var curFighter = _charactersFightOrder[curFighterIndex].data;
        var chosenAttack = GetAttack(curFighter);
        Assert.IsFalse(chosenAttack == null);
        var damageCalc = CalculateMeleeDamage(chosenAttack, curFighter);
        selectedSequenceLevel = chosenAttack.sequence;
        if (chosenAttack.effectAllEnemyGroup)
        {
            actionsLogScript.AddToLog($" uses {chosenAttack.displayName}");
            _finishedSequence = false;
            StartCoroutine(DoEffectAll(damageCalc, false));
            _actionHasTaken = true;
            CloseAll();
            return;
        }

        SetAttack(damageCalc, chosenAttack.displayName);
    }

    /**
     * Calculates a Melee fight action points of effect by the player's stats.
     */
    private int CalculateMeleeDamage(MeleeAttackData chosenAttack, CharacterData curFighter)
    {
        if (chosenAttack.damage < 0) return (int)Mathf.Floor(chosenAttack.damage - curFighter.Attack);
        return (int)Mathf.Ceil(chosenAttack.damage + curFighter.Attack);
    }

    /**
     * Calculates a Magic fight action points of effect by the player's stats.
     */
    private int CalculateMagicEffect(MagicAttackData chosenAttack, CharacterData curFighter)
    {
        if (chosenAttack.pointsOfEffect < 0) return (int)Mathf.Floor(chosenAttack.pointsOfEffect - curFighter.Attack);
        return (int)Mathf.Ceil(chosenAttack.pointsOfEffect + curFighter.Attack);
    }

    /**
     * Updates the fight by the current magic selected.
     */
    private void HandleMagicSelected()
    {
        var curFighter = _charactersFightOrder[curFighterIndex];
        var chosenMagic = GetMagic(curFighter.data);
        Assert.IsFalse(chosenMagic == null);
        if (curFighter.data.currentMp < chosenMagic.manaPointsToConsume)
        {
            _actionHasTaken = false;
            actionsLogScript.ClearLog();
            StartCoroutine(DisplayNotEnoughMana());
            CloseAll();
            return;
        }

        selectedSequenceLevel = chosenMagic.sequence;
        var damageCalc = CalculateMagicEffect(chosenMagic, curFighter.data);
        if (chosenMagic.isOnSelf || chosenMagic.effectAllSameGroup || chosenMagic.effectAllEnemyGroup)
        {
            HandleNoTargetActions(curFighter, chosenMagic, damageCalc);
            return;
        }

        _manaToConsume = chosenMagic.manaPointsToConsume;
        SetAttack(damageCalc, chosenMagic.displayName);
    }

    private IEnumerator DisplayNotEnoughMana()
    {
        PointerBehavior.Instance.disableSpace = true;
        messageBox.ShowDialogs(new[] { NOT_ENOUGH_MANA }, false);
        yield return new WaitUntil(() => messageBox.gameObject.activeSelf);
        PointerBehavior.Instance.disableSpace = false;
    }

    private void HandleNoTargetActions(CharacterDisplayScript curFighter, MagicAttackData chosenMagic, int damageCalc)
    {
        curFighter.EffectMana(-chosenMagic.manaPointsToConsume); //consume mana
        actionsLogScript.AddToLog($" uses {chosenMagic.displayName} for {Mathf.Abs(damageCalc)}");
        _finishedSequence = false;
        if (chosenMagic.effectAllSameGroup)
            StartCoroutine(DoEffectAll(damageCalc, true));
        else if (chosenMagic.effectAllEnemyGroup)
            StartCoroutine(DoEffectAll(damageCalc, false));
        else
            StartCoroutine(DoEffectSelf(damageCalc, curFighter));

        _actionHasTaken = true;
        CloseAll();
    }

    /**
     * closes all display's and resets the turn's actions.
     */
    private void CloseAll()
    {
        MagicMenuScript.Instance.CloseMenu();
        FightMenuScript.Instance.CloseMenu();
        _attackSelected = false;
        _selectedObjName = null;
    }

    /**
     * gets the selected magic attack of the current fighter by the name given from the pointer.
     * below is where the name is being set.
     * <see cref="SetSelectedObject" />
     */
    private MagicAttackData GetMagic(CharacterData curFighter)
    {
        foreach (var magic in curFighter.magics)
            if (magic.name.Equals(_selectedObjName))
                return magic;
        return null;
    }

    /**
     * gets the selected melee attack of the current fighter by the name given from the pointer.
     * below is where the name is being set.
     * <see cref="SetSelectedObject" />
     */
    private MeleeAttackData GetAttack(CharacterData curFighter)
    {
        foreach (var attack in curFighter.attacks)
            if (attack.name.Equals(_selectedObjName))
                return attack;
        return null;
    }

    /**
     * Activates the sequence of the current attack selected and acts after it is finished.
     */
    private IEnumerator DoAttack()
    {
        SequenceManager.Instance.isSequenceActive = true;
        SequenceManager.Instance.StartSequenceByLevel(selectedSequenceLevel);
        var nameToAttack = _selectedObjName;
        yield return new WaitUntil(() => !SequenceManager.Instance.isSequenceActive);
        if (SequenceManager.Instance.IsGood)
        {
            foreach (var fighter in _allFighters)
                if (fighter.isActiveAndEnabled && fighter.name == nameToAttack)
                {
                    _allFighters[curFighterIndex].AnimateAttack();
                    actionsLogScript.AddToLog($" '{fighter.data.displayName}'" +
                                              $" for {Mathf.Abs(_selectedCharDamage)} points");
                    fighter.EffectHealth(_selectedCharDamage, actionsLogScript);
                    _selectedObjName = null;
                    break;
                }
        }
        else
        {
            actionsLogScript.AddToLog(FAILED_SEQUENCE_MSG);
        }

        _finishedSequence = true;
    }

    /**
     * Activates and updates the game after an item action in fight.
     */
    private void DoItem()
    {
        var res = InventoryManager.Instance.GetItem(_selectedObj);
        Assert.IsFalse(res == null);
        actionsLogScript.AddToLog($" Uses {res.Data.displayName} Health");
        InventoryManager.Instance.Remove(res.Data);
        switch (res.Data.id)
        {
            case InventoryItemData.k_HEALTH_POTION_ID:
                _charactersFightOrder[curFighterIndex].EffectHealth(res.Data.pointsOfEffect, actionsLogScript);
                actionsLogScript.AddToLog($" {res.Data.pointsOfEffect} Health");
                break;
            case InventoryItemData.k_MANA_POTION_ID:
                _charactersFightOrder[curFighterIndex].EffectMana(res.Data.pointsOfEffect);
                actionsLogScript.AddToLog($" {res.Data.pointsOfEffect} Mana");
                break;
        }

        _selectedObj = null;
        InventoryManager.Instance.CloseInventory();
    }

    /**
     * Gets the next fighter alive by the fight order.
     */
    private void UpdateNextAttacker()
    {
        curFighterIndex += 1;
        if (curFighterIndex >= _charactersFightOrder.Count)
        {
            curFighterIndex = 0;
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
            if (obj.isActiveAndEnabled) continue;
            _charactersFightOrder.Remove(obj);
        }
    }

    /**
     * Checks for win/loss in the battle.
     */
    private void CheckWinLoss()
    {
        if (isFightOver) return;
        bool activeHero = false, activeEnemy = false;
        foreach (var obj in _charactersFightOrder.ToArray())
            if (obj.isActiveAndEnabled)
            {
                if (obj.data.isHero)
                    activeHero = true;
                else
                    activeEnemy = true;
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
        gameOver.SetActive(true);
        var text = SequenceManager.Instance.textDisplay;
        text.gameObject.SetActive(true);
        text.SetText("You Died! \n Game Over");
        SoundManager.Instance.PlayBattleResult(false);
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        Destroy(SoundManager.Instance.gameObject);
        SceneManager.LoadScene("MainMenu");
        Destroy(MySceneManager.Instance.gameObject);
        Destroy(InventoryManager.Instance.gameObject);
        Destroy(GameManager.Instance.gameObject);
    }

    private IEnumerator DoGameWon()
    {
        var text = SequenceManager.Instance.textDisplay;
        text.gameObject.SetActive(true);
        text.SetText(WON_BATTLE_TEXT);
        SoundManager.Instance.PlayBattleResult(true);
        var timeToWait = 5f; //temporary
        foreach (var obj in _charactersFightOrder.ToArray())
            if (obj.isActiveAndEnabled)
                if (obj.data.isHero)
                    timeToWait = obj.AnimateGameWon();
        GameManager.Instance.FightWon();
        yield return new WaitForSeconds(timeToWait);
        MySceneManager.Instance.LoadNormalScene(MySceneManager.Instance.CurrentEntrance + 1,
            MySceneManager.Instance.LastSceneName);
    }

    /**
     * Sets the next fighter to take action.
     */
    public void ContinueFight()
    {
        CheckWinLoss();
        if (isFightOver) return;
        var curFighter = _charactersFightOrder[curFighterIndex];
        StartCoroutine(WaitForAnimation(curFighter.AnimateTurn, () =>
        {
            dataMenuScript.UpdateDisplay();
            if (_heroes.Contains(curFighter.data))
            {
                actionsLogScript.AddToLog("'" + curFighter.data.displayName + "'");
                _isBackEnabled = true;
                PointerBehavior.Instance.SetNewTexts(mainFightMenuButtons, numOfButtonsInARow);
                StartCoroutine(ActivatePlayerTurn());
            }
            else
            {
                _isBackEnabled = false;
                actionsLogScript.AddToLog($"'{curFighter.data.displayName}' - (Enemy {curFighter.characterNum.text}),");
                MakeEnemyAction(curFighter);
            }
        }));
    }

    /**
     * Checks if the fight is over and updates the fight order by the fight events this round.
     */
    private void UpdateRound()
    {
        CheckWinLoss();
        do
        {
            UpdateNextAttacker();
        } while (!_charactersFightOrder[curFighterIndex].isActiveAndEnabled);
    }

    /**
     * Stalls the progra
     */
    private IEnumerator WaitForAnimation(Func<float> curAnimation, Action afterAnimationAction)
    {
        var time = curAnimation.Invoke();
        yield return new WaitForSeconds(time);
        afterAnimationAction.Invoke();
    }


    /**
     * Enables the pointer until the player finished his current turn action.
     */
    private IEnumerator ActivatePlayerTurn()
    {
        PointerBehavior.Instance.gameObject.SetActive(true);
        yield return new WaitUntil(() => _actionHasTaken);
        PointerBehavior.Instance.gameObject.SetActive(false);
    }

    /**
     * Sets the current battle fight order by the fighter's speed.
     */
    private void SetBattleOrder()
    {
        var temp = new Dictionary<CharacterDisplayScript, int>();
        var i = -1;
        foreach (var fighter in _allFighters)
        {
            i++;
            if (!fighter.isActiveAndEnabled) continue;
            temp.Add(fighter, fighter.data.speed);
            if (i < 6) _totalEnemiesSpeed += fighter.data.speed;
            _totalHeroesSpeed += fighter.data.speed;
        }

        _charactersFightOrder.AddRange(
            temp.OrderBy(x => -x.Value).ToDictionary(x => x.Key, x => x.Value).Keys.ToArray());
    }

    private void SetEnemyRowDisplay(CharacterDisplayScript[] battleRow, CharacterData[] infoRow, int startInd)
    {
        for (var i = 0; i < battleRow.Length; i++)
        {
            if (i + startInd >= infoRow.Length || infoRow[i + startInd] == null)
            {
                battleRow[i].gameObject.SetActive(false);
                continue;
            }

            if (!infoRow[i + startInd].isHero) infoRow[i + startInd].ResetStats();
            battleRow[i].SetScriptable(infoRow[i + startInd]);
        }
    }

    /**
     * makes a randomize enemy action by his attacks and magics
     */
    private void MakeEnemyAction(CharacterDisplayScript curFighter)
    {
        var fighter = curFighter.data;
        var damage = RandomizeEnemyAttack(fighter, fighter.magics, fighter.attacks);
        if (damage == 0) return;
        var attackIndex = Random.Range(6, _allFighters.Count);
        while (!_allFighters[attackIndex].isActiveAndEnabled) attackIndex = Random.Range(6, _allFighters.Count);
        var heroToAttack = _allFighters[attackIndex];
        actionsLogScript.AddToLog($" on '{heroToAttack.data.displayName}' for {Mathf.Abs(damage)} points");
        heroToAttack.EffectHealth(damage, actionsLogScript);
        _actionHasTaken = true;
        _finishedSequence = true;
    }

    /**
     * gets a random enemy attack and initializes it if no hero needs to be selected.
     * other wise return the attack damage.
     * <returns> The chosen attack's calculated damage or zero if attack all-ready accord. </returns>
     * >
     */
    private int RandomizeEnemyAttack(CharacterData fighter,
        MagicAttackData[] magics, MeleeAttackData[] attacks)
    {
        bool isOnSelf = false, effectAllEnemyGroup = false;
        var magicOrAttack = Random.Range(0, 2);
        if (fighter.magics.Length == 0) magicOrAttack = 0;

        if (fighter.attacks.Length == 0) magicOrAttack = 1;
        var damage = 0;
        if (magicOrAttack == 0)
        {
            var actionInd = Random.Range(0, attacks.Length);
            actionsLogScript.AddToLog($" Uses {attacks[actionInd].displayName}");
            damage = CalculateMeleeDamage(attacks[actionInd], fighter);
            effectAllEnemyGroup = attacks[actionInd].effectAllEnemyGroup;
        }
        else
        {
            var actionInd = Random.Range(0, magics.Length);
            actionsLogScript.AddToLog($" Uses {magics[actionInd].displayName}");
            damage = CalculateMagicEffect(magics[actionInd], fighter);
            isOnSelf = magics[actionInd].isOnSelf;
            effectAllEnemyGroup = magics[actionInd].effectAllEnemyGroup;
        }

        if (HandleNoTargetEnemyAction(isOnSelf, effectAllEnemyGroup, damage)) return 0;

        return damage;
    }

    private bool HandleNoTargetEnemyAction(bool isOnSelf, bool effectAllEnemyGroup, int damage)
    {
        _finishedSequence = true;
        _actionHasTaken = true;
        if (isOnSelf)
        {
            actionsLogScript.AddToLog($" for {Mathf.Abs(damage)}");
            _charactersFightOrder[curFighterIndex].EffectHealth(damage, actionsLogScript);
            return true;
        }

        if (effectAllEnemyGroup)
        {
            actionsLogScript.AddToLog($" on all heroes for {Mathf.Abs(damage)} points");
            foreach (var character in _charactersFightOrder)
                if (character.data.isHero)
                    character.EffectHealth(damage, actionsLogScript);
            return true;
        }

        return false;
    }

    /**
     * Sets the next display by the player menu action.
     */
    public void DoChosenAction(string action)
    {
        var curFighter = _charactersFightOrder[curFighterIndex].data;
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
                return; //when choosing an attack
        }

        _lastChosenAction = action;
    }

    /**
     * activates and updates the fight by run action.
     */
    private void DoRun()
    {
        var runAttemptValue = Random.Range(0, _totalEnemiesSpeed + _totalHeroesSpeed);
        if (runAttemptValue > _totalEnemiesSpeed)
        {
            PointerBehavior.Instance.gameObject.SetActive(false);
            MySceneManager.Instance.LoadNormalScene(MySceneManager.Instance.CurrentEntrance,
                MySceneManager.k_YELLOW_FLOOR, true);
            return;
        }

        actionsLogScript.AddToLog(RUN_FAILED_LOG);
        _finishedSequence = true;
        _actionHasTaken = true;
    }

    /**
     * sets the damage of the selected attack and sends the pointer to select a target.
     */
    private void SetAttack(int damage, string actionName)
    {
        actionsLogScript.AddToLog($" uses {actionName} on");
        _attackSelected = true;
        _selectedObjName = null;
        var allFighters = new GameObject[_allFighters.Count];
        var i = 0;
        foreach (var character in _allFighters)
        {
            allFighters[i] = character.gameObject;
            i++;
        }

        StopCoroutine(ActivatePlayerTurn());
        _actionHasTaken = false;
        SelectCharacter(allFighters);
        _selectedCharDamage = damage; //saved for after player selected an enemy.
    }

    /**
     * Effects all enemies or heroes health by the given amount.
     * <param name="amount">  amount to effect all health with. (can be negative) </param>
     * <param name="isOnHeroes"> flag to determine which group to effect. </param>
     */
    private IEnumerator DoEffectAll(int amount, bool isOnHeroes)
    {
        SequenceManager.Instance.isSequenceActive = true;
        SequenceManager.Instance.StartSequenceByLevel(selectedSequenceLevel);
        yield return new WaitUntil(() => !SequenceManager.Instance.isSequenceActive);
        if (SequenceManager.Instance.IsGood)
        {
            var groupToAttack = "enemies!";
            if (isOnHeroes) groupToAttack = "heroes!";
            actionsLogScript.AddToLog($" to Effects All {groupToAttack} for {Mathf.Abs(amount)} points!");
            foreach (var fighter in _charactersFightOrder)
                if (isOnHeroes == fighter.data.isHero)
                    fighter.EffectHealth(amount, actionsLogScript);
        }
        else
        {
            actionsLogScript.AddToLog(FAILED_SEQUENCE_MSG);
        }

        _finishedSequence = true;
        _actionHasTaken = true;
    }

    /**
     * Effects the current fighter by a given amount if the sequence is completed successfully.
     */
    private IEnumerator DoEffectSelf(int amount, CharacterDisplayScript caster)
    {
        SequenceManager.Instance.isSequenceActive = true;
        SequenceManager.Instance.StartSequenceByLevel(selectedSequenceLevel);
        yield return new WaitUntil(() => !SequenceManager.Instance.isSequenceActive);
        if (SequenceManager.Instance.IsGood)
            caster.EffectHealth(amount, actionsLogScript);
        else
            actionsLogScript.AddToLog(FAILED_SEQUENCE_MSG);
        _finishedSequence = true;
        _actionHasTaken = true;
    }

    /**
     * Sets the selected object by the player. (called by the pointer)
     */
    public void SetSelectedObject(GameObject obj)
    {
        _actionHasTaken = true;
        _finishedSequence = true;
        if (_lastChosenAction == ITEMS_TEXT)
        {
            _selectedObj = obj;
            return;
        }

        _selectedObjName = obj.name;
    }

    /**
     * sets the pointer to point at the game character displays and waits for the player's action
     */
    private void SelectCharacter(GameObject[] allObjects)
    {
        PointerBehavior.Instance.SetNewObjects(allObjects, 3, false);
        StartCoroutine(ActivatePlayerTurn());
    }

    #region Constants

    private const string FIGHT_TEXT = "fight";
    private const string RUN_TEXT = "run";
    private const string ITEMS_TEXT = "items";
    private const string MAGIC_TEXT = "magic";
    private const string RUN_FAILED_LOG = " Run attempt has failed !!";
    private const string NOT_ENOUGH_MANA = "Not Enough Mana";
    private const string FAILED_SEQUENCE_MSG = "\n !! but, sequence was failed !!";
    private const string WON_BATTLE_TEXT = "You Won \n All enemies are dead!";
    private const int POINTER_FIGHT_SCALE = 50;

    #endregion

    #region Inspector

    public AudioSource fightAudio;
    public TextMeshProUGUI[] mainFightMenuButtons;
    public int numOfButtonsInARow;
    public DataMenuScript dataMenuScript;
    public ActionsLogScript actionsLogScript;
    public MessageBoxScript messageBox;
    public GameObject gameOver;
    public CharacterData[] enemiesLevel1;
    public CharacterData[] enemiesLevel2;
    public CharacterData[] enemiesLevel3;

    [SerializeField] private CharacterDisplayScript[] enemyRow1;
    [SerializeField] private CharacterDisplayScript[] enemyRow2;
    [SerializeField] private CharacterDisplayScript[] heroRow;
    [SerializeField] private float timeBetweenTurns = 0.7f;
    [SerializeField] private bool isFightOver;
    [SerializeField] private int curFighterIndex;
    [SerializeField] private int selectedSequenceLevel;

    #endregion

    #region Private Fields

    private CharacterData[][] _enemiesByLevels;
    private int _fightLevel;
    private CharacterData[] _heroes;
    private List<CharacterDisplayScript> _charactersFightOrder;
    private List<CharacterDisplayScript> _allFighters;
    private string _selectedObjName;
    private GameObject _selectedObj;
    private Stack<string> _actionDepth;
    private int _totalHeroesSpeed;
    private int _totalEnemiesSpeed;
    private int _selectedCharDamage;
    private string _lastChosenAction;
    private bool _attackSelected;
    private bool _startFight;
    private bool _finishedSequence;
    private bool _actionHasTaken;
    private bool _isBackEnabled;
    private int _manaToConsume;

    #endregion
}