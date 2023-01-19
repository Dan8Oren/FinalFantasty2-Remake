using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class MessageBoxScript : MonoBehaviour
{
    public bool enableSpace;
    [SerializeField] private String[] dialogs;
    [SerializeField] private TextMeshPro textDisplay;
    [SerializeField] private float timeBetweenChars;
    [SerializeField] private bool freezeHero;
    [SerializeField] private bool isNPC;
    [SerializeField] private bool isLoop = false;
    [SerializeField] private float timeBeforeLoop;
    [SerializeField] private float timeAfterLoop;
    
    private Rigidbody2D _heroRigid;
    private IEnumerator _activeDialog;
    private int _curDialogIndex;
    private bool _isContinue;

    private void OnEnable()
    {
        gameObject.SetActive(true);
        _curDialogIndex = 0;
        // textDisplay.SetText(dialogs[_curDialogIndex]);
        if (dialogs.Length > 0)
        {
            _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
            StartCoroutine(_activeDialog); 
        }
    }
    
    private void FreezeHero()
    {
        Assert.IsFalse(_heroRigid == null);
        _heroRigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }
    

    private void Start()
    {
        _heroRigid = MySceneManager.Instance.hero.GetComponent<Rigidbody2D>();
        if (freezeHero)
        {
            FreezeHero();
        }
        if (!GameManager.Instance.isStartOfGame && ! isNPC)
        {
            if (_activeDialog != null)
            {
                StopCoroutine(_activeDialog);
            }
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (_heroRigid != null)
        {
            _heroRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) && !isLoop) && enableSpace)
        {
            if (_activeDialog != null)
            {
                StopCoroutine(_activeDialog);
            }
            _curDialogIndex++;
            if (_curDialogIndex >= dialogs.Length)
            {
                if (MySceneManager.Instance.IsInFight)
                {
                    enableSpace = false;
                    PointerBehavior.Instance.disableSpace = false;
                    if (_isContinue)
                    {
                        FightManager.Instance.ContinueFight();
                    }
                }
                gameObject.SetActive(false);
                this.enabled = false;
                return;
            }
            _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
            StartCoroutine(_activeDialog);
        }
    }

    private IEnumerator AnimateDialog(string dialog)
    {
        char[] chars = dialog.Replace("\\n", "\n").Replace("\\t", "\t").ToCharArray();
        String tempToShow = new string("");
        foreach (var c in chars)
        {
            tempToShow += c;
            textDisplay.SetText(tempToShow);
            yield return new WaitForSeconds(timeBetweenChars);
        }
        if (isLoop)
        {
            yield return new WaitForSeconds(timeAfterLoop);
            textDisplay.SetText("");
            _curDialogIndex++;
            if (_curDialogIndex >= dialogs.Length)
            {
                _curDialogIndex = 0;
            }
            yield return new WaitForSeconds(timeBeforeLoop);
            StartCoroutine(AnimateDialog(dialogs[_curDialogIndex]));
        }
    }
    
    public void ShowDialogs(string[] newDialogs,bool toFreezeOrContinue)
    {
        if (newDialogs != null)
        {
            dialogs = newDialogs;
        }
        if (!MySceneManager.Instance.IsInFight && toFreezeOrContinue)
        {
            FreezeHero();
        }
        _isContinue = toFreezeOrContinue;
        gameObject.SetActive(true);
        if (enabled)
        {
            _curDialogIndex = 0;
            Assert.IsFalse(dialogs.Length == 0);
            _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
            StartCoroutine(_activeDialog);
        }
        this.enabled = true;
        
        
    }
    
    
}
