using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Color = System.Drawing.Color;

public class ActionsLogScript : MonoBehaviour
{
    public MessageBoxScript messageBox;
    [SerializeField] private GameObject _listContent;
    [SerializeField] private GameObject _logPrefab;
    [SerializeField] private String logIndexColorCode;
    private string _log;
    private Stack<string> _data;
    private int _roundCounter;
    private void Start()
    {
        messageBox.gameObject.SetActive(false);
        _data = new Stack<string>();
        _roundCounter = 1;
        _log = $"{_roundCounter}. ";
    }

    public void ShowLog ()
    {
        _roundCounter++;
        GameObject obj = Instantiate(_logPrefab, _listContent.transform);
        Assert.IsFalse(obj == null);
        TextMeshProUGUI logText = obj.GetComponent<TextMeshProUGUI>();
        Assert.IsFalse(logText == null);
        int logCounterIndex = _log.IndexOf('.')+1;
        logText.SetText("<color="+logIndexColorCode+">"+_log.Substring(0,logCounterIndex)+"</color>"
                        + _log.Substring(logCounterIndex+1));
        messageBox.enableSpace = true;
        PointerBehavior.Instance.disableSpace = true;
        messageBox.ShowDialogs(new string[]{_log},true);
        _log = $"{_roundCounter}. ";
        _data.Clear();
    }
    
    public void AddToLog(string info)
    {
        _data.Push(info);
        _log+=info;
    }
    
    
    public void RemoveLastLog()
    {
        _log = _log.Remove(_log.Length - _data.Pop().Length);
    }
    
    public void ClearLog()
    {
        _log = $"{_roundCounter}. ";
    }
    
}

