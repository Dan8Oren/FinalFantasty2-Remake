using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroInfoScript : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    [SerializeField] private CharacterData heroData;
    [SerializeField] private SpriteRenderer image;

    public CharacterData GetData()
    {
        return heroData;
    }
    
    public void SetHeroData(CharacterData data)
    {
        image.sprite = data.characterIcon;
        heroData = data;
        nameText.SetText(data.name);
        String info = $"Health Points: {data.currentHp}/{data.MaxHp}\n" +
                      $"Mana Points: {data.currentMp}/{data.MaxMp}\n" +
                      $"Attack: {data.Attack}\n" +
                      $"Magic attack: {data.Magic}\n";
        infoText.SetText(info);
        healthBar.maxValue = data.MaxHp;
        healthBar.minValue = 0;
        healthBar.value = data.currentHp;
    }

    private void Awake()
    {
        if (heroData != null)
        {
            SetHeroData(heroData);
        }
    }
}
