using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterDisplayScript : MonoBehaviour
{
    private static readonly float GameWonAnimationTime = 4f;
    private static readonly float RegularAnimationTime = 1f;
    public TextMeshPro characterNum;
    public CharacterData data;
    public bool IsStun { get; private set;}
    [SerializeField] private Slider slider;
    [SerializeField] private float screenShakeTime;
    [SerializeField] private float screenShakeForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnDistance;
    [SerializeField] private float winDistance;
    [SerializeField] private float turnWaitTime;
    [SerializeField] private float winWaitTime;
    private SpriteRenderer _image;
    private Animator _animator;

    private void Start()
    {
        _image = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
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
            _image = GetComponent<SpriteRenderer>();
        }
        _image.sprite = newScript.idleImage;
        slider.maxValue = data.MaxHp;
        slider.value = data.currentHp;
    }
    
    /**
     * Adds to the character health points the given pointsOfEffect. (could be negative)
     * and uses 'actionsLogScript' inorder to log if the character has died.
     */
    public void EffectHealth(int pointsOfEffect,ActionsLogScript actionsLogScript)
    {
        if (pointsOfEffect < 0 && data.isHero)
        {
            StartCoroutine(MySceneManager.Instance.Shake(screenShakeTime, screenShakeForce));
        }
        data.currentHp += pointsOfEffect;
        if (data.currentHp <= 0)
        {
            LogDeadCharacter(actionsLogScript);
            slider.value = 0;
            if (_animator == null)
            {
                gameObject.SetActive(false);
                return;
            }
            enabled = false;
            _animator.SetBool("Dead",true);
        }
        else if (data.currentHp > data.MaxHp)
        {
            data.currentHp = data.MaxHp;
        }
        slider.value = data.currentHp;
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
        print(data.currentMp);
        if (data.currentMp > data.MaxMp)
        {
            data.currentMp = data.MaxMp;
        }
        slider.value = data.currentHp;
    }

    public float AnimateTurn()
    {
        if (_animator == null)
        {
            return 0;
        }
        if (IsStun)
        {
            IsStun = false;
            _animator.SetBool("Stun",false);
            return RegularAnimationTime;
        }
        _animator.SetBool("Turn",true);
        StartCoroutine(MoveToPosition(turnDistance,turnWaitTime));
        return RegularAnimationTime;
    }
    public float AnimateEndTurn()
    {
        if (_animator == null)
        {
            return 0;
        }
        _animator.SetBool("Turn",false);
        StartCoroutine(MoveToPosition(-turnDistance,turnWaitTime));
        return RegularAnimationTime;
    }
    public float AnimateGameWon()
    {
        if (_animator == null)
        {
            return 0;
        }
        _animator.SetBool("Won",false);
        StartCoroutine(MoveToPosition(winDistance,winWaitTime));
        return GameWonAnimationTime;
    }
    
    public float AnimateAttack()
    {
        if (_animator == null)
        {
            return 0;
        }
        _animator.SetTrigger("Attack");
        return RegularAnimationTime;
    }

    public float StunCharacter()
    {
        if (_animator == null)
        {
            
            return 0;
        }
        IsStun = true;
        _animator.SetBool("Stun",true);
        return RegularAnimationTime;
    }

    private IEnumerator MoveToPosition(float distance,float waitBeforeMovement)
    {
        Vector3 curPos = transform.position;
        float target = curPos.x + distance;
        yield return new WaitForSeconds(waitBeforeMovement);
        while (Mathf.Abs(curPos.x - target) > 0.01f)
        {
            curPos.x = Mathf.MoveTowards(curPos.x,target,moveSpeed);
            transform.position = curPos;
            yield return null;
        }
    }

}
