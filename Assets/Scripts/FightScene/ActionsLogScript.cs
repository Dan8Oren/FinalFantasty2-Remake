using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class ActionsLogScript : MonoBehaviour
{
    private const string EMPTY_LOG_PATTERN = "^\\d*. $";
    public MessageBoxScript messageBox;
    [SerializeField] private GameObject listContent;
    [SerializeField] private GameObject logPrefab;
    [SerializeField] private string logIndexColorCode;
    private Stack<string> _data;
    private string _log;
    private int _roundCounter;
    private Regex _emptyLog;

    private void Start()
    {
        _emptyLog = new Regex(EMPTY_LOG_PATTERN);  
        messageBox.gameObject.SetActive(false);
        _data = new Stack<string>();
        _roundCounter = 1;
        _log = $"{_roundCounter}. ";
    }

    /**
     * display the current log at the log menu and the fight's messageBox and resets the log.
     */
    public void ShowLog()
    {
        if (_emptyLog.IsMatch(_log))
        {
            return;
        }
        _roundCounter++;
        var obj = Instantiate(logPrefab, listContent.transform);
        Assert.IsFalse(obj == null);
        var logText = obj.GetComponent<TextMeshProUGUI>();
        Assert.IsFalse(logText == null);
        var logCounterIndex = _log.IndexOf('.') + 1;
        logText.SetText("<color=" + logIndexColorCode + ">" + _log.Substring(0, logCounterIndex) + "</color>"
                        + _log.Substring(logCounterIndex + 1));
        messageBox.enableSpace = true;
        PointerBehavior.Instance.disableSpace = true;
        messageBox.ShowDialogs(new[] { _log }, true);
        ClearLog();
    }

    public void AddToLog(string info)
    {
        _data.Push(info);
        _log += info;
    }


    public void RemoveLastLog()
    {
        _log = _log.Remove(_log.Length - _data.Pop().Length);
    }

    public void ClearLog()
    {
        _log = $"{_roundCounter}. ";
        _data.Clear();
    }
}