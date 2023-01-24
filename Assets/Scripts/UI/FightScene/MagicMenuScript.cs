using System.Collections;
using TMPro;
using UnityEngine;

public class MagicMenuScript : MonoBehaviour
{
    private const int NUM_MAGICS_IN_A_ROW = 2;
    public GameObject[] ObjectsToDisplay;
    public TextMeshProUGUI[] magicsText;
    public TextMeshProUGUI[] InfoTexts;
    private CharacterData _curFighter;
    private int _curPointerAttackIndex;
    public static MagicMenuScript Instance { get; private set; }
    public MagicAttackData[] Magics { get; private set; }

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
        var data = Magics[_curPointerAttackIndex];
        InfoTexts[0].SetText($"Points of effect: {Mathf.Abs(data.pointsOfEffect) + _curFighter.Magic}," +
                             $" Mana Cost {data.manaPointsToConsume}");
        InfoTexts[1].SetText($"Sequence level: {data.sequence}");
        InfoTexts[2].SetText(data.info);
    }

    /**
     * Displays the magic menu by the given character magic attacks.
     * returns and displays a message if there are none.
     */
    public void ShowMenu(CharacterData curHero)
    {
        _curFighter = curHero;
        Magics = curHero.magics;
        if (Magics.Length == 0)
        {
            StartCoroutine(DisplayNoMagics());
            ;
            return;
        }

        foreach (var obj in ObjectsToDisplay) obj.SetActive(true);

        SetDisplay();
        SetPointerToMenu();
    }

    private IEnumerator DisplayNoMagics()
    {
        var msgBox = FightManager.Instance.messageBox;
        msgBox.ShowDialogs(new[] { "\t no magics!" }, false);
        msgBox.enableSpace = true;
        PointerBehavior.Instance.gameObject.SetActive(false);
        yield return new WaitUntil(() => !msgBox.gameObject.activeSelf);
        PointerBehavior.Instance.gameObject.SetActive(true);
        FightManager.Instance.DoGoBack();
    }

    private void SetDisplay()
    {
        for (var i = 0; i < magicsText.Length; i++)
        {
            magicsText[i].enabled = true;
            if (i >= Magics.Length)
            {
                magicsText[i].enabled = false;
                continue;
            }

            magicsText[i].name = Magics[i].name;
            magicsText[i].SetText(Magics[i].displayName);
        }

        _curPointerAttackIndex = 0;
        UpdateInfo();
    }

    public void CloseMenu()
    {
        foreach (var obj in ObjectsToDisplay) obj.SetActive(false);
    }

    /**
     * Sets the pointer to point at the menu's magics.
     */
    public void SetPointerToMenu()
    {
        PointerBehavior.Instance.SetNewTexts(magicsText, NUM_MAGICS_IN_A_ROW);
    }
}