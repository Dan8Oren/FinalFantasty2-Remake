using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PointerBehavior : MonoBehaviour
{
    public static PointerBehavior Instance = null;
    public GameObject SelectedObj { get; set;}
    private TextMeshProUGUI[] _textMenu;
    private GameObject[] _objects;
    private int _numOfObjectsInARow;
    private int _curIndex;
    private SpriteRenderer _spriteRenderer;
    private bool _isText = true; //a flag to indicate if the pointer points at images or text.
    private int _maxIndex;
    private bool _isMenu;
    private int _firstHeroIndex;

    private void Awake()
    {
        //singleton pattern the prevent two pointers
        if (Instance == null || Instance == this)
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            Instance = this;
            SelectedObj = null;
            return;
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    private void Update()
    {
        if (_isMenu)
        {
            MenuInputHandler();
        }
        else
        {
            CharactersInputHandler();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!MySceneManager.Instance.IsInFight)
            {
                SelectedObj = _objects[_curIndex];
                return;
            }
            if (_isText)
            {
                FightManager.Instance.DoChosenAction(_textMenu[_curIndex].text);
            }
            else
            {
                FightManager.Instance.SetSelectedObject(_objects[_curIndex]);
            }
        }
        
    }

    private void CharactersInputHandler()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && _curIndex+1 < _maxIndex)
        {
            _curIndex++;
            FixEmptyCharacters(1);
            UpdatePointerLocation();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)&& _curIndex-1 >= 0)
        {
            _curIndex--;
            FixEmptyCharacters(-1);
            UpdatePointerLocation();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _curIndex+=_numOfObjectsInARow;
            if (_curIndex>=_maxIndex)
            {
                _curIndex -= _maxIndex;
            }
            FixEmptyCharacters(1);
            UpdatePointerLocation();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _curIndex-=_numOfObjectsInARow;
            if (_curIndex < 0)
            {
                _curIndex *= -1;
            }
            FixEmptyCharacters(-1);
            UpdatePointerLocation();
        }
    }

    private void FixEmptyCharacters(int jump)
    {
        while (!_objects[_curIndex].activeSelf)
        {
            _curIndex+=jump;
            if (_curIndex == _maxIndex)
            {
                _curIndex = 0;
            }
            else if(_curIndex < 0)
            {
                _curIndex = _maxIndex - 1;
            }
        }
    }

    private void MenuInputHandler()
    {
        int jump = 0;
        if (Input.GetKeyDown(KeyCode.RightArrow) && _curIndex+1 < _maxIndex)
        {
            _curIndex++;
            jump = 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)&& _curIndex-1 >= 0)
        {
            _curIndex--;
            jump = -1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && _curIndex+_numOfObjectsInARow<_maxIndex)
        {
            _curIndex+=_numOfObjectsInARow;
            jump = 1;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && _curIndex-_numOfObjectsInARow >= 0)
        {
            _curIndex-=_numOfObjectsInARow;
            jump = -1;
        }

        if (jump != 0)
        {
            if (_isText){
                FixEmptyTexts(jump);
            }else{
                FixEmptyCharacters(jump);
            }
        }
        UpdatePointerLocation();
    }
    
    private void FixEmptyTexts(int jump)
    {
        while (!_textMenu[_curIndex].isActiveAndEnabled)
        {
            _curIndex+=jump;
            if (_curIndex == _maxIndex)
            {
                _curIndex = 0;
            }
            else if(_curIndex < 0)
            {
                _curIndex = _maxIndex - 1;
            }
        }
    }
    
    /**
     * Updates the pointer's location by the menuButton location of the current index.
     */
    private void UpdatePointerLocation()
    {
        if (_isText)
        {
            UpdatePositionByTexts();
            return;
        }
        UpdatePositionByObject();
    }

    private void UpdatePositionByTexts()
    {
        Vector3 pos = _textMenu[_curIndex].transform.TransformPoint(
            new Vector3(-_textMenu[_curIndex].rectTransform.rect.width / 2,
                0, 0));
        if (pos.x > 0)
        {
            pos.x -= _spriteRenderer.bounds.size.x / 2;
        }
        else
        {
            pos.x += _spriteRenderer.bounds.size.x / 2;
        }

        transform.position = pos;
    }
    
    private void UpdatePositionByObject()
    {
        // Vector3 pos = _objects[_curIndex].transform.position;
        Image image = _objects[_curIndex].GetComponent<Image>();
        Vector3 pos = image.transform.TransformPoint(
            new Vector3(-image.rectTransform.rect.width,
                0, 0));
        if (pos.x > 0)
        {
            pos.x -= _spriteRenderer.bounds.size.x / 2;
        }
        else
        {
            pos.x += _spriteRenderer.bounds.size.x / 2;
        }
        transform.position = pos;
    }

    public void SetNewTexts(TextMeshProUGUI[] newButtons,int numTextsInARow)
    {
        Assert.IsFalse(newButtons.Length == 0);
        _isText = true;
        _isMenu = true;
        _textMenu = newButtons;
        _numOfObjectsInARow = numTextsInARow;
        _curIndex = 0;
        _maxIndex = newButtons.Length;
        UpdatePointerLocation();
    }
    
    public void SetNewObjects(GameObject[] newObjects,int numObjectsInARow,bool isMenu)
    {
        Assert.IsFalse(newObjects.Length == 0);
        _isText = false;
        _isMenu = isMenu;
        _numOfObjectsInARow = numObjectsInARow;
        _curIndex = 0;
        _maxIndex = newObjects.Length;
        _objects = newObjects;
        FixEmptyCharacters(1);
        UpdatePointerLocation();
    }

    // private void OnEnable()
    // {
    //     gameObject.SetActive(true);
    // }
    //
    // private void OnDisable()
    // {
    //     gameObject.SetActive(false);
    // }

}
