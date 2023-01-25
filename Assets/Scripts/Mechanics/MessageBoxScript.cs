using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class MessageBoxScript : MonoBehaviour
{
    private void Awake()
    {
        if (!GameManager.Instance.isStartOfGame && !isNpc)
        {
            if (_activeDialog != null) StopCoroutine(_activeDialog);
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        _heroRigid = MySceneManager.Instance.hero.GetComponent<Rigidbody2D>();
        if (freezeHero) FreezeHero();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isLoop && enableSpace)
        {
            if (_activeDialog != null) StopCoroutine(_activeDialog);
            _curDialogIndex++;
            if (_curDialogIndex >= dialogs.Count)
            {
                _isPlaying = false;
                if (MySceneManager.Instance.IsInFight)
                {
                    enableSpace = false;
                    PointerBehavior.Instance.disableSpace = false;
                    if (_isContinue) FightManager.Instance.ContinueFight();
                }

                dialogs.Clear();
                _curDialogIndex = 0;
                gameObject.SetActive(false);
                enabled = false;
                return;
            }

            _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
            StartCoroutine(_activeDialog);
        }
    }

    private void OnEnable()
    {
        gameObject.SetActive(true);
        _curDialogIndex = 0;
        if (dialogs.Count > 0)
        {
            _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
            StartCoroutine(_activeDialog);
        }
    }

    private void OnDisable()
    {
        if (_heroRigid != null) _heroRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FreezeHero()
    {
        Assert.IsFalse(_heroRigid == null);
        _heroRigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    /**
     * Shows the dialogs to the screen character character, with a delay.
     * (Skips html code.)
     */
    private IEnumerator AnimateDialog(string dialog)
    {
        _isPlaying = true;
        var chars = dialog.Replace("\\n", "\n").Replace("\\t", "\t").ToCharArray();
        var tempToShow = new string("");
        var isOnHtml = false;
        var closing = 0;
        foreach (var c in chars)
        {
            if (closing == 2) isOnHtml = false;
            tempToShow += c;
            textDisplay.SetText(tempToShow);
            if (!isOnHtml) yield return new WaitForSeconds(timeBetweenChars);
            isOnHtml = HandleHtmlText(c, isOnHtml, ref closing);
        }

        if (isLoop)
        {
            yield return new WaitForSeconds(timeAfterLoop);
            textDisplay.SetText("");
            _curDialogIndex++;
            if (_curDialogIndex >= dialogs.Count) _curDialogIndex = 0;
            yield return new WaitForSeconds(timeBeforeLoop);
            StartCoroutine(AnimateDialog(dialogs[_curDialogIndex]));
        }
    }

    /**
     * Used to skip the display of html code (<>)
     * as a character character.
     */
    private static bool HandleHtmlText(char c, bool isOnHtml, ref int closing)
    {
        if (c == '<' && !isOnHtml)
        {
            closing = 0;
            isOnHtml = true;
        }

        else
        {
            if (c == '>') closing++;
        }

        return isOnHtml;
    }

    /**
     * sets new dialogs toy queue, and displays them.
     * @toFreezeOrContinue - double meaning flag:
     * -In fight to know if the battle should continue to the next attacker after the dialogs.
     * -In world to know if the freeze the hero's rigid body on message display.
     */
    public void ShowDialogs(string[] newDialogs, bool toFreezeOrContinue)
    {
        if (newDialogs != null) dialogs.AddRange(newDialogs);
        if (!MySceneManager.Instance.IsInFight && toFreezeOrContinue) FreezeHero();
        _isContinue = toFreezeOrContinue;
        gameObject.SetActive(true);
        if (!_isPlaying)
        {
            _curDialogIndex = 0;
            Assert.IsFalse(dialogs.Count == 0);
            _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
            StartCoroutine(_activeDialog);
        }

        enabled = true;
    }

    #region Inspector

    public bool enableSpace;
    [SerializeField] private List<string> dialogs;
    [SerializeField] private TextMeshPro textDisplay;
    [SerializeField] private float timeBetweenChars;
    [SerializeField] private bool freezeHero;
    [SerializeField] private bool isNpc;
    [SerializeField] private bool isLoop;
    [SerializeField] private float timeBeforeLoop;
    [SerializeField] private float timeAfterLoop;

    #endregion

    #region Fields

    private Rigidbody2D _heroRigid;
    private IEnumerator _activeDialog;
    private int _curDialogIndex;
    private bool _isContinue;
    private bool _isPlaying;

    #endregion
}