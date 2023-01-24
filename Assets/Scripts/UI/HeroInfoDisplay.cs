using TMPro;
using UnityEngine;

public class HeroInfoDisplay : MonoBehaviour
{
    public HealthBar healthBar;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    [SerializeField] private CharacterData heroData;
    [SerializeField] private SpriteRenderer image;

    private void OnEnable()
    {
        if (heroData != null) SetHeroData(heroData);
        UpdateInfo();
    }

    public void UpdateInfo()
    {
        var info = $"Health Points: {heroData.currentHp}/{heroData.MaxHp}\n" +
                   $"Mana Points: {heroData.currentMp}/{heroData.MaxMp}\n" +
                   $"Attack: {heroData.Attack}\n" +
                   $"Magic attack: {heroData.Magic}\n";
        infoText.SetText(info);
        healthBar.SetHealth(heroData.currentHp);
    }

    public CharacterData GetData()
    {
        return heroData;
    }

    /**
     * sets the display to a given hero's CharacterData ScriptableObject.
     */
    public void SetHeroData(CharacterData data)
    {
        image.sprite = data.characterIcon;
        heroData = data;
        nameText.SetText(data.name);
        var info = $"Health Points: {data.currentHp}/{data.MaxHp}\n" +
                   $"Mana Points: {data.currentMp}/{data.MaxMp}\n" +
                   $"Attack: {data.Attack}\n" +
                   $"Magic attack: {data.Magic}\n";
        infoText.SetText(info);
        healthBar.InitializeSlider(data.currentHp, data.MaxHp);
    }
}