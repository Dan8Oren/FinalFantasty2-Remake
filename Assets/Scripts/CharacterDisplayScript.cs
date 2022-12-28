using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDisplayScript : MonoBehaviour
{
    public CharacterScriptableObject scriptableObject;
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    public void SetScriptable(CharacterScriptableObject newScript)
    {
        scriptableObject = newScript;
        _image.sprite = newScript.characterImage;
    }
}
