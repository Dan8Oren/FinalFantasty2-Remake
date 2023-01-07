using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterDisplayScript : MonoBehaviour
{
    public TextMeshProUGUI characterNum;
    [FormerlySerializedAs("scriptableObject")] public CharacterData data;
    [FormerlySerializedAs("_slider")] [SerializeField] private Slider slider;
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }
    
    /**
     * Sets the scriptable object of the character at the display.
     * shows it's health,name and image.
     */
    public void SetScriptable(CharacterData newScript)
    {
        data = newScript;
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }
        _image.sprite = newScript.characterImage;
        slider.maxValue = data.maxHP;
        slider.value = data.currentHP;
    }
    
    /**
     * Adds to the character health points the given pointsOfEffect. (could be negative)
     * and uses 'actionsLogScript' inorder to log if the character has died.
     */
    public void EffectHealth(int pointsOfEffect,ActionsLogScript actionsLogScript)
    {
        data.currentHP += pointsOfEffect;
        print(this.name +" Health "+data.currentHP);
        if (data.currentHP <= 0)
        {
            LogDeadCharacter(actionsLogScript);
            slider.value = 0;
            gameObject.SetActive(false);
        }
        else if (data.currentHP > data.maxHP)
        {
            data.currentHP = data.maxHP;
        }
        slider.value = data.currentHP;
    }

    private void LogDeadCharacter(ActionsLogScript actionsLogScript)
    {
        actionsLogScript.ShowLog();
        if (data.isHero)
        {
            actionsLogScript.AddToLog($"{data.name} is DEAD!");
            return;
        }
        actionsLogScript.AddToLog($"{data.name}, Enemy number {characterNum.text} is DEAD!");
    }

    /**
     * Adds to the character mana points the given pointsOfEffect. (could be negative)
     */
    public void EffectMana(int pointsOfEffect)
    {
        print(this.name +" Got Mana Effected!");
        data.currentMP += pointsOfEffect;
        print(data.currentMP);
        if (data.currentMP > data.maxMP)
        {
            data.currentMP = data.maxMP;
        }
        slider.value = data.currentHP;
    }
}
