using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class CharacterDisplayScript : MonoBehaviour
{
    private Animator _animator;

    private SpriteRenderer _image;
    private SpriteLibrary _spriteLibrary;

    private void Start()
    {
        _image = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _spriteLibrary = GetComponent<SpriteLibrary>();
    }

    /**
     * Sets the scriptable object of the character at the display.
     * shows it's health,name and image.
     */
    public void SetScriptable(CharacterData newScript)
    {
        data = newScript;
        Start();
        if (data.isHero) _spriteLibrary.spriteLibraryAsset = data.spriteLibrary;
        _image.sprite = newScript.idleImage;
        healthDisplay.InitializeSlider(data.currentHp, data.MaxHp);
        gameObject.name = data.name + characterNum;
    }

    /**
     * Adds to the character health points the given pointsOfEffect. (could be negative)
     * and uses 'actionsLogScript' inorder to log if the character has died.
     */
    public void EffectHealth(int pointsOfEffect, ActionsLogScript actionsLogScript)
    {
        HandleAttackEffects(pointsOfEffect);
        data.currentHp += pointsOfEffect;
        if (data.currentHp <= 0)
        {
            SoundManager.Instance.PlayDeath(FightManager.Instance.fightAudio);
            LogDeadCharacter(actionsLogScript);
            actionsLogScript.ShowLog();
            healthDisplay.SetHealth(0);
            if (_animator == null)
            {
                gameObject.SetActive(false);
                return;
            }

            enabled = false;
            _animator.SetBool(DEAD_ANIMATOR_TRIGGER, true);
        }
        else if (data.currentHp > data.MaxHp)
        {
            data.currentHp = data.MaxHp;
        }

        healthDisplay.SetHealth(data.currentHp);
    }

    private void HandleAttackEffects(int pointsOfEffect)
    {
        if (pointsOfEffect < 0)
        {
            SoundManager.Instance.PlayHit(FightManager.Instance.fightAudio);
            if (data.isHero) StartCoroutine(MySceneManager.Instance.Shake(screenShakeTime, screenShakeForce));
        }
        else
        {
            SoundManager.Instance.PlayAddHealth(FightManager.Instance.fightAudio);
        }
    }

    private void LogDeadCharacter(ActionsLogScript actionsLogScript)
    {
        actionsLogScript.ShowLog();
        if (data.isHero)
        {
            actionsLogScript.AddToLog($"'{data.name}' is DEAD!");
            return;
        }

        actionsLogScript.AddToLog($"'{data.name}' - (Enemy {characterNum.text}),  is DEAD!");
    }

    /**
     * Adds to the character mana points the given pointsOfEffect. (could be negative)
     */
    public void EffectMana(int pointsOfEffect)
    {
        data.currentMp += pointsOfEffect;
        if (data.currentMp > data.MaxMp) data.currentMp = data.MaxMp;
    }

    public float AnimateTurn()
    {
        if (_animator == null) return 0;
        _animator.SetBool(TURN_ANIMATOR_TRIGGER, true);
        StartCoroutine(MoveToPosition(turnDistance, turnWaitTime));
        return regularAnimationTime;
    }

    public float AnimateEndTurn()
    {
        if (_animator == null) return 0;
        _animator.SetBool(TURN_ANIMATOR_TRIGGER, false);
        StartCoroutine(MoveToPosition(-turnDistance, turnWaitTime));
        return regularAnimationTime;
    }

    public float AnimateGameWon()
    {
        if (_animator == null) return 0;
        _animator.SetBool(WON_ANIMATOR_TRIGGER, true);
        StartCoroutine(MoveToPosition(winDistance, winWaitTime));
        return gameWonAnimationTime;
    }

    public float AnimateAttack()
    {
        if (_animator == null) return 0;
        _animator.SetTrigger(ATTACK_ANIMATOR_TRIGGER);
        return regularAnimationTime;
    }

    private IEnumerator MoveToPosition(float distance, float waitBeforeMovement)
    {
        var curPos = transform.position;
        var target = curPos.x + distance;
        yield return new WaitForSeconds(waitBeforeMovement);
        while (Mathf.Abs(curPos.x - target) > 0.01f)
        {
            curPos.x = Mathf.MoveTowards(curPos.x, target, moveSpeed);
            transform.position = curPos;
            yield return null;
        }
    }

    #region Constants
    private const string TURN_ANIMATOR_TRIGGER = "Turn";
    private const string WON_ANIMATOR_TRIGGER = "Won";
    private const string ATTACK_ANIMATOR_TRIGGER = "Attack";
    private const string DEAD_ANIMATOR_TRIGGER = "Dead";

    #endregion
    
    
    #region Inspector

    public TextMeshPro characterNum;
    public CharacterData data;
    
    [SerializeField] private HealthBar healthDisplay;
    [SerializeField] private float gameWonAnimationTime = 3f;
    [SerializeField] private float regularAnimationTime = 1f;
    [SerializeField] private float screenShakeTime;
    [SerializeField] private float screenShakeForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnDistance;
    [SerializeField] private float winDistance;
    [SerializeField] private float turnWaitTime;
    [SerializeField] private float winWaitTime;

    #endregion
}