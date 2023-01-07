using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class MessageBoxScript : MonoBehaviour
{
    // public static MessageBoxScript Instance { get; private set; }
    [SerializeField] private String[] dialogs;
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private float timeBetweenChars;
    private Rigidbody2D _heroRigid;
    private IEnumerator _activeDialog;

    private int _curDialogIndex;

    // private void Awake()
    // {
    //     //singleton pattern the prevent two scene managers
    //     if ( Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);
    //     }
    //     DontDestroyOnLoad(gameObject);
    //     Instance = this;
    // }

    private void OnEnable()
    {
        gameObject.SetActive(true);
        _curDialogIndex = 0;
        // textDisplay.SetText(dialogs[_curDialogIndex]);
        Assert.IsFalse(dialogs.Length == 0);
        _activeDialog = AnimateDialog(dialogs[_curDialogIndex]);
        StartCoroutine(_activeDialog);
    }

    private void Start()
    {
        _heroRigid = MySceneManager.Instance.hero.GetComponent<Rigidbody2D>();
        Assert.IsFalse(_heroRigid == null);
        if (!GameManager.Instance.isStartOfGame)
        {
            StopCoroutine(_activeDialog);
            gameObject.SetActive(false);
            return;
        }
        _heroRigid.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    private void OnDisable()
    {
        _heroRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopCoroutine(_activeDialog);
            _curDialogIndex++;
            if (_curDialogIndex >= dialogs.Length)
            {
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
    }
    
    public void ShowDialogs(string[] newDialogs=null)
    {
        if (newDialogs != null)
        {
            dialogs = newDialogs;
        }
        _heroRigid.constraints = RigidbodyConstraints2D.FreezeAll;
        gameObject.SetActive(true);
        this.enabled = true;
    }
    
    
}
