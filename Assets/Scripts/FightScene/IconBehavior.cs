using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IconBehavior : MonoBehaviour
{
    private Image _sprite;
    private void Awake()
    {
        _sprite = gameObject.GetComponent<Image>();
    }

    public void UpdateSprite(Sprite newIcon)
    {
        _sprite.sprite = newIcon;
    }
}
