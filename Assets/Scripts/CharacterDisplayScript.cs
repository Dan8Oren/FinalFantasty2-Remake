using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplayScript : MonoBehaviour
{
    public CharacterScriptableObject scriptableObject;
    [SerializeField] private Slider _slider;
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    public void SetScriptable(CharacterScriptableObject newScript)
    {
        scriptableObject = newScript;
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }
        _image.sprite = newScript.characterImage;
        _slider.maxValue = scriptableObject.maxHP;
        _slider.value = scriptableObject.currentHP;
    }

    public void EffectHealth(int pointsOfEffect)
    {
        print(this.name +" Got Effected by "+pointsOfEffect);
        scriptableObject.currentHP += pointsOfEffect;
        if (scriptableObject.currentHP <= 0)
        {
            _slider.value = 0;
            gameObject.SetActive(false);
        }
        else if (scriptableObject.currentHP > scriptableObject.maxHP)
        {
            scriptableObject.currentHP = scriptableObject.maxHP;
        }
        _slider.value = scriptableObject.currentHP;
    }
    
    public void EffectMana(int pointsOfEffect)
    {
        print(this.name +" Got Mana Effected!");
        scriptableObject.currentMP += pointsOfEffect;
        if (scriptableObject.currentMP > scriptableObject.maxMP)
        {
            scriptableObject.currentMP = scriptableObject.maxMP;
        }
        _slider.value = scriptableObject.currentHP;
    }
}
