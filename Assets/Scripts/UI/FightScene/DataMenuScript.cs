using TMPro;
using UnityEngine;

/**
 * Displays the hero's by health and mana points, at the fight scene.
 */
public class DataMenuScript : MonoBehaviour
{
    private const string DEAD_TEXT = "DEAD";
    public TextMeshProUGUI[] namesTexts;
    public TextMeshProUGUI[] hpTexts;
    public TextMeshProUGUI[] manaTexts;

    private CharacterData[] _heroesDatas;

    private void Start()
    {
        _heroesDatas = GameManager.Instance.GetHeroes();
        for (var i = 0; i < namesTexts.Length; i++)
        {
            if (i >= _heroesDatas.Length)
            {
                namesTexts[i].enabled = false;
                hpTexts[i].enabled = false;
                manaTexts[i].enabled = false;
                continue;
            }

            namesTexts[i].SetText(_heroesDatas[i].name);
            hpTexts[i].SetText($"{_heroesDatas[i].currentHp}/{_heroesDatas[i].MaxHp}");
            manaTexts[i].SetText($"{_heroesDatas[i].currentMp}");
        }
    }

    public void UpdateDisplay()
    {
        if (_heroesDatas == null)
        {
            Start();
            return;
        }

        for (var i = 0; i < _heroesDatas.Length; i++)
        {
            var hp = _heroesDatas[i].currentHp;
            hpTexts[i].SetText($"{hp}/{_heroesDatas[i].MaxHp}");
            manaTexts[i].SetText($"{_heroesDatas[i].currentMp}");
            if (hp <= 0) hpTexts[i].SetText(DEAD_TEXT);
        }
    }
}