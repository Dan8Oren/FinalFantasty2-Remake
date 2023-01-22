using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class MagicMenuScript : MonoBehaviour
{
    private const int NUM_MAGICS_IN_A_ROW = 2;
    public static MagicMenuScript Instance { get; private set; }
    public MagicAttackData[] Magics { get; private set; }
    public GameObject[] ObjectsToDisplay;
    public TextMeshProUGUI[] magicsText;
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
        if ( ObjectsToDisplay[0].activeSelf && PointerBehavior.Instance.IsText &&
             _curPointerAttackIndex != PointerBehavior.Instance.CurIndex)
        {
            _curPointerAttackIndex = PointerBehavior.Instance.CurIndex;
            UpdateInfo();
        }
    }

    private void UpdateInfo()
    {
        MagicAttackData data = Magics[_curPointerAttackIndex];
        InfoTexts[0].SetText($"Points of effect: {Mathf.Abs(data.pointsOfEffect)+_curFighter.Magic}," +
                             $" Mana Cost {data.manaPointsToConsume}");
        InfoTexts[1].SetText($"Sequence level: {data.sequence}");
        InfoTexts[2].SetText(data.info);

    }


    public void ShowMenu(CharacterData curHero)
    {
        _curFighter = curHero;
        Magics = curHero.magics;
        if (Magics.Length == 0)
        {
            StartCoroutine(WaitForPlayer());;
            return;
        }
        foreach (GameObject obj in ObjectsToDisplay)
        {
            obj.SetActive(true);
        }

        SetDisplay();
        SetPointerToMenu();
    }
    
    private IEnumerator WaitForPlayer()
    {
        MessageBoxScript msgBox =FightManager.Instance.messageBox;
        msgBox.ShowDialogs(new String[]{"\t no magics!"},false);
        msgBox.enableSpace = true;
        PointerBehavior.Instance.gameObject.SetActive(false);
        yield return new WaitUntil(() => !msgBox.gameObject.activeSelf);
        PointerBehavior.Instance.gameObject.SetActive(true);
        FightManager.Instance.DoGoBack();
    }
    
    private void SetDisplay()
    {
        for (int i = 0; i <magicsText.Length; i++)
        {
            if (i>= Magics.Length)
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
        foreach (GameObject obj in ObjectsToDisplay)
        {
            obj.SetActive(false);
        }
    }

    public void SetPointerToMenu()
    {
        PointerBehavior.Instance.SetNewTexts(magicsText,NUM_MAGICS_IN_A_ROW);
    }
    
}
