using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

public class ActionsLogScript : MonoBehaviour
{
    public MessageBoxScript messageBox;
    [SerializeField] private GameObject _listContent;
    [SerializeField] private GameObject _logPrefab;
    private string _log;
    private Stack<string> _data;
    private int _roundCounter;
    private void Start()
    {
        _data = new Stack<string>();
        _roundCounter = 0;

    }

    public void ShowLog ()
    {
        _roundCounter++;
        GameObject obj = Instantiate(_logPrefab, _listContent.transform);
        Assert.IsFalse(obj == null);
        TextMeshProUGUI logText = obj.GetComponent<TextMeshProUGUI>();
        Assert.IsFalse(logText == null);
        logText.SetText(_log);
        messageBox.enableSpace = true;
        messageBox.ShowDialogs(new string[]{_log},false);
        print(_log); //TODO: remove me!
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
    
}

