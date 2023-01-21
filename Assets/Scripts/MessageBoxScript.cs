using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class MessageBoxScript : MonoBehaviour
{
    public bool enableSpace;
    [SerializeField] private List<String> dialogs;
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
        if (dialogs.Count > 0)
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
            if (_curDialogIndex >= dialogs.Count)
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
                dialogs.Clear();
                _curDialogIndex = 0;
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
        bool isOnHtml = false;
        int closing = 0;
        foreach (var c in chars)
        {
            if (closing == 2)
            {
                isOnHtml = false;
            }
            tempToShow += c;
            textDisplay.SetText(tempToShow);
            if (!isOnHtml)
            {
                yield return new WaitForSeconds(timeBetweenChars);
            }
            if (c == '<' && !isOnHtml)
            {
                closing = 0;
                isOnHtml = true;
            }
            
            else
            {
                if (c == '>')
                {
                    closing++;
                }
            }
            
        }
        if (isLoop)
        {
            yield return new WaitForSeconds(timeAfterLoop);
            textDisplay.SetText("");
            _curDialogIndex++;
            if (_curDialogIndex >= dialogs.Count)
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
            dialogs.AddRange(newDialogs);
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
            Assert.IsFalse(dialogs.Count == 0);
            _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
            StartCoroutine(_activeDialog);
        }
        this.enabled = true;
        
        
    }
    
    
}
