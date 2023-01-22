using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class FightMenuScript : MonoBehaviour
{
    private const int NUM_ATTACKS_IN_A_ROW = 2;
    public static FightMenuScript Instance { get; private set; }
    public MeleeAttackData[] Attacks { get; private set; }
    public GameObject[] ObjectsToDisplay;
    public TextMeshProUGUI[] attacksTexts;
    public TextMeshProUGUI[] InfoTexts;
    private int _curPointerAttackIndex;
    private CharacterData _curFighter;
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

    private void UpdateInfo()
    {
        MeleeAttackData data = Attacks[_curPointerAttackIndex];
        InfoTexts[0].SetText($"Attack Power: {Mathf.Abs(data.damage)+_curFighter.Attack}");
        InfoTexts[1].SetText($"Sequence level: {data.sequence}");
        InfoTexts[2].SetText(data.info);
    }


    public void ShowMenu(CharacterData curHero)
    {
        Attacks = curHero.attacks;
        _curFighter = curHero;
        if (Attacks.Length == 0)
        {
            StartCoroutine(WaitForPlayer());
            return;
        }
        foreach (GameObject obj in ObjectsToDisplay)
        {
            obj.SetActive(true);
        }

        setDisplay();
        SetPointerToMenu();
    }

    private IEnumerator WaitForPlayer()
    {
        MessageBoxScript msgBox =FightManager.Instance.messageBox;
        msgBox.ShowDialogs(new String[]{"\t no attacks!"},false);
        msgBox.enableSpace = true;
        PointerBehavior.Instance.gameObject.SetActive(false);
        yield return new WaitUntil(() => !msgBox.gameObject.activeSelf);
        PointerBehavior.Instance.gameObject.SetActive(true);
        FightManager.Instance.DoGoBack();
    }

    private void setDisplay()
    {
        for (int i = 0; i <attacksTexts.Length; i++)
        {
            if (i>= Attacks.Length)
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
        foreach (GameObject obj in ObjectsToDisplay)
        {
            obj.SetActive(false);
        }
    }

    public void SetPointerToMenu()
    {
        PointerBehavior.Instance.SetNewTexts(attacksTexts,NUM_ATTACKS_IN_A_ROW);
    }
}
