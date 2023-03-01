using System.Collections;
using TMPro;
using UnityEngine;

public class FightMenuScript : MonoBehaviour
{
    private const int NUM_ATTACKS_IN_A_ROW = 2;
    public GameObject[] ObjectsToDisplay;
    public TextMeshProUGUI[] attacksTexts;
    public TextMeshProUGUI[] InfoTexts;
    private CharacterData _curFighter;
    private int _curPointerAttackIndex;
    public static FightMenuScript Instance { get; private set; }
    public RegularAttackData[] Attacks { get; private set; }

    private void Awake()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
            CloseMenu();
            return;
        }

        Destroy(this);
    }


    private void Update()
    {
        if (ObjectsToDisplay[0].activeSelf && PointerBehavior.Instance.IsText &&
            _curPointerAttackIndex != PointerBehavior.Instance.CurIndex)
        {
            _curPointerAttackIndex = PointerBehavior.Instance.CurIndex;
            UpdateInfo();
        }
    }

    /**
     * Updates the info displayed about the current magic the pointer points at.
     */
    private void UpdateInfo()
    {
        var data = Attacks[_curPointerAttackIndex];
        InfoTexts[0].SetText($"Attack Power: {Mathf.Abs(data.pointsOfEffect) + _curFighter.Attack}");
        InfoTexts[1].SetText($"Sequence level: {data.sequence}");
        InfoTexts[2].SetText(data.info);
    }

    /**
     * Displays the fight menu by the given character melee attacks.
     * returns and displays a message if there are none.
     */
    public void ShowMenu(CharacterData curHero)
    {
        Attacks = curHero.attacks;
        _curFighter = curHero;
        if (Attacks.Length == 0)
        {
            StartCoroutine(DisplayNoAttacks());
            return;
        }

        foreach (var obj in ObjectsToDisplay) obj.SetActive(true);

        SetDisplay();
        SetPointerToMenu();
    }

    private IEnumerator DisplayNoAttacks()
    {
        var msgBox = FightManager.Instance.messageBox;
        msgBox.ShowDialogs(new[] { "\t no attacks!" }, false);
        msgBox.enableSpace = true;
        PointerBehavior.Instance.gameObject.SetActive(false);
        yield return new WaitUntil(() => !msgBox.gameObject.activeSelf);
        PointerBehavior.Instance.gameObject.SetActive(true);
        FightManager.Instance.DoGoBack();
    }

    private void SetDisplay()
    {
        for (var i = 0; i < attacksTexts.Length; i++)
        {
            attacksTexts[i].enabled = true;
            if (i >= Attacks.Length)
            {
                attacksTexts[i].enabled = false;
                continue;
            }

            attacksTexts[i].name = Attacks[i].name;
            attacksTexts[i].SetText(Attacks[i].displayName);
            _curPointerAttackIndex = 0;
            UpdateInfo();
        }
    }

    public void CloseMenu()
    {
        foreach (var obj in ObjectsToDisplay) obj.SetActive(false);
    }

    /**
     * Sets the pointer to point at the fight's actions.
     */
    public void SetPointerToMenu()
    {
        PointerBehavior.Instance.SetNewTexts(attacksTexts, NUM_ATTACKS_IN_A_ROW);
    }
}