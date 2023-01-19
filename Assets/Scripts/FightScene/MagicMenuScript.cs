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
        if ( ObjectsToDisplay[0].activeSelf &&
             _curPointerAttackIndex != PointerBehavior.Instance.CurIndex)
        {
            _curPointerAttackIndex = PointerBehavior.Instance.CurIndex;
            UpdateInfo();
        }
    }

    private void UpdateInfo()
    {
        MagicAttackData data = Magics[_curPointerAttackIndex];
        InfoTexts[0].SetText($"Points of effect: {data.pointsOfEffect+_curFighter.Attack}," +
                             $" Mana Cost {data.manaPointsToConsume}");
        InfoTexts[1].SetText($"Sequence length: {data.sequence}");
        InfoTexts[2].SetText(data.info);

    }


    public void ShowMenu(CharacterData curHero)
    {
        _curFighter = curHero;
        Magics = curHero.magics;
        if (Magics.Length == 0)
        {
            FightManager.Instance.messageBox.ShowDialogs(new String[]{"no magics!"},false);
            FightManager.Instance.DoGoBack();
            return;
        }
        foreach (GameObject obj in ObjectsToDisplay)
        {
            obj.SetActive(true);
        }

        setDisplay();
        SetPointerToMenu();
    }

    private void setDisplay()
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

    // private void Update()
    // {
    //     throw new NotImplementedException();
    // }
}
