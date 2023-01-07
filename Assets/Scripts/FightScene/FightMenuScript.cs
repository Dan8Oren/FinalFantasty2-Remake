using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FightMenuScript : MonoBehaviour
{
    private const int NUM_ATTACKS_IN_A_ROW = 2;
    public static FightMenuScript Instance { get; private set; }
    public MeleeAttackData[] Attacks { get; private set; }
    public GameObject[] ObjectsToDisplay;
    public TextMeshProUGUI[] attacksTexts;

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

    public void ShowMenu(MeleeAttackData[] attacks)
    {
        Attacks = attacks;
        if (attacksTexts.Length == 0)
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
        for (int i = 0; i <attacksTexts.Length; i++)
        {
            if (i>= Attacks.Length)
            {
                attacksTexts[i].enabled = false;
                continue;
            }
            attacksTexts[i].SetText(Attacks[i].name);
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
        PointerBehavior.Instance.SetNewTexts(attacksTexts,NUM_ATTACKS_IN_A_ROW);
    }
}
