using System;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider health;
    [SerializeField] private Slider damagedBar;
    [SerializeField] private float shrinkTime;
    [SerializeField] private float shrinkSpeed;
    private float _curShrinkTimer;
    private bool _isHeal;
    private int _maxHealth;


    public void InitializeSlider(int curHealth,int maxHealth)
    {
        _maxHealth = maxHealth;
        damagedBar.minValue = 0;
        health.minValue = 0;
        health.maxValue = maxHealth;
        damagedBar.maxValue = maxHealth;
        health.value = curHealth;
        damagedBar.value = curHealth;
    }

    private void Update()
    {
        _curShrinkTimer -= Time.deltaTime;
        if (_curShrinkTimer < 0)
        {
            if (damagedBar.value > health.value && !_isHeal)
            {
                damagedBar.value -= shrinkSpeed * Time.deltaTime;
            }
            else if (damagedBar.value > health.value && _isHeal)
            {
                health.value += shrinkSpeed * Time.deltaTime;
            }

        }
    }

    public void SetHealth(float healthToSet)
    {
        _curShrinkTimer = shrinkTime;
        _isHeal = false;
        if (healthToSet >= health.value)
        {
            SoundManager.Instance.PlayAddHealth(FightManager.Instance.fightAudio);
            _isHeal = true;
            damagedBar.value = healthToSet;
            return;
        }
        SoundManager.Instance.PlayHit(FightManager.Instance.fightAudio);
        if (healthToSet < 0)
        {
            health.value = 0;
            return;
        }
        health.value = healthToSet;
    }
}
