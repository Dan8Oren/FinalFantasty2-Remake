using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoScript : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    [SerializeField]private CharacterData heroData;
    [SerializeField] private Image image;

    public CharacterData GetData()
    {
        return heroData;
    }
    
    public void SetHeroData(CharacterData data)
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        image.sprite = data.characterIcon;
        heroData = data;
        nameText.SetText(data.name);
        String info = $"Health Points: {data.currentHP}/{data.maxHP}\n"+
                      $"Mana Points: {data.currentMP}/{data.maxMP}\n"+
                      $"Attack: {data.attack}\n"+
                      $"Magic attack: {data.magic}\n"+
                      $"defence: {data.defence}\n";
        infoText.SetText(info);
        healthBar.maxValue = data.maxHP;
        healthBar.minValue = 0;
        healthBar.value = data.currentHP;
    }
}
