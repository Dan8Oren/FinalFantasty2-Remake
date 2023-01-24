using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider health;
    [SerializeField] private Slider damagedBar;
    [SerializeField] private float shrinkTime;
    [SerializeField] private float shrinkSpeed;
    private float _curShrinkTimer;
    private bool _isHeal;
    private int _maxHealth;

    private void Update()
    {
        _curShrinkTimer -= Time.deltaTime;
        if (_curShrinkTimer < 0)
        {
            if (damagedBar.value > health.value && !_isHeal)
                damagedBar.value -= shrinkSpeed * Time.deltaTime;
            else if (damagedBar.value > health.value && _isHeal) health.value += shrinkSpeed * Time.deltaTime;
        }
    }


    public void InitializeSlider(int curHealth, int maxHealth)
    {
        _maxHealth = maxHealth;
        damagedBar.minValue = 0;
        health.minValue = 0;
        health.maxValue = maxHealth;
        damagedBar.maxValue = maxHealth;
        health.value = curHealth;
        damagedBar.value = curHealth;
    }

    public void SetHealth(float healthToSet)
    {
        _curShrinkTimer = shrinkTime;
        _isHeal = false;
        if (healthToSet >= health.value)
        {
            _isHeal = true;
            damagedBar.value = healthToSet;
            return;
        }

        if (healthToSet < 0)
        {
            health.value = 0;
            return;
        }

        health.value = healthToSet;
    }
}