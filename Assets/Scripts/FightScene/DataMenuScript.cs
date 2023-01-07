using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DataMenuScript : MonoBehaviour
{
    public TextMeshProUGUI[] namesTexts;
    public TextMeshProUGUI[] HPTexts;
    public TextMeshProUGUI[] ManaTexts;

    private CharacterData[] _heroesDatas;
    private void Start()
    {
        _heroesDatas = GameManager.Instance.GetHeroes();
        for (int i=0;i<namesTexts.Length;i++)
        {
            if (i>=_heroesDatas.Length)
            {
                namesTexts[i].enabled = false;
                HPTexts[i].enabled = false;
                ManaTexts[i].enabled = false;
                continue;
            }
            namesTexts[i].SetText(_heroesDatas[i].name);
            HPTexts[i].SetText($"{_heroesDatas[i].currentHP}/{_heroesDatas[i].maxHP}");
            ManaTexts[i].SetText($"{_heroesDatas[i].currentMP}");
        }
    }

    public void UpdateDisplay()
    {
        if (_heroesDatas == null)
        {
            _heroesDatas = GameManager.Instance.GetHeroes();
        }
        for (int i=0;i<_heroesDatas.Length;i++)
        {
            int hp = _heroesDatas[i].currentHP;
            HPTexts[i].SetText($"{hp}/{_heroesDatas[i].maxHP}");
            ManaTexts[i].SetText($"{_heroesDatas[i].currentMP}");
            if (hp <=0)
            {
                HPTexts[i].SetText("DEAD");
            }
            
        }
    }
}
