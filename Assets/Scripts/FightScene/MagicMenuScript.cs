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

    public void ShowMenu(MagicAttackData[] magics)
    {
        Magics = magics;
        if (Magics.Length == 0)
        {
            print("no magics!");
            FightManager.Instance.DoGoBack();
            //TODO: popupMassage
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
            magicsText[i].SetText(Magics[i].name);
            //TODO: description
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
        PointerBehavior.Instance.SetNewTexts(magicsText,NUM_MAGICS_IN_A_ROW);
    }

    // private void Update()
    // {
    //     throw new NotImplementedException();
    // }
}
