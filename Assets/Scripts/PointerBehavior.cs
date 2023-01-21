using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PointerBehavior : MonoBehaviour
{
    public bool disableSpace;
    public static PointerBehavior Instance = null;
    public GameObject SelectedObj { get; set;}
    private TextMeshProUGUI[] _textMenu;
    private GameObject[] _objects;
    private int _numOfObjectsInARow;
    public int CurIndex { get; private set;}
    private SpriteRenderer _spriteRenderer;
    public bool IsText { get; private set;} //a flag to indicate if the pointer points at images or text.
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
        if (Input.GetKeyDown(KeyCode.Space) && !disableSpace)
        {
            if (!MySceneManager.Instance.IsInFight)
            {
                SelectedObj = _objects[CurIndex];
                return;
            }
            if (IsText)
            {
                FightManager.Instance.DoChosenAction(_textMenu[CurIndex].name);
            }
            else
            {
                FightManager.Instance.SetSelectedObject(_objects[CurIndex]);
            }
        }
        
    }

    private void CharactersInputHandler()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && CurIndex+1 < _maxIndex)
        {
            CurIndex++;
            FixEmptyCharacters(1);
            UpdatePointerLocation();
        }
        if (Input.GetKeyDown(KeyCode.UpArrow)&& CurIndex-1 >= 0)
        {
            CurIndex--;
            FixEmptyCharacters(-1);
            UpdatePointerLocation();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CurIndex+=_numOfObjectsInARow;
            if (CurIndex>=_maxIndex)
            {
                CurIndex -= _maxIndex;
            }
            FixEmptyCharacters(1);
            UpdatePointerLocation();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CurIndex-=_numOfObjectsInARow;
            if (CurIndex < 0)
            {
                CurIndex *= -1;
            }
            FixEmptyCharacters(-1);
            UpdatePointerLocation();
        }
    }

    private void FixEmptyCharacters(int jump)
    {
        while (!_objects[CurIndex].activeSelf)
        {
            CurIndex+=jump;
            if (CurIndex == _maxIndex)
            {
                CurIndex = 0;
            }
            else if(CurIndex < 0)
            {
                CurIndex = _maxIndex - 1;
            }
        }
    }

    private void MenuInputHandler()
    {
        int jump = 0;
        if (Input.GetKeyDown(KeyCode.RightArrow) && CurIndex+1 < _maxIndex)
        {
            CurIndex++;
            jump = 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)&& CurIndex-1 >= 0)
        {
            CurIndex--;
            jump = -1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && CurIndex+_numOfObjectsInARow<_maxIndex)
        {
            CurIndex+=_numOfObjectsInARow;
            jump = 1;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && CurIndex-_numOfObjectsInARow >= 0)
        {
            CurIndex-=_numOfObjectsInARow;
            jump = -1;
        }

        if (jump != 0)
        {
            if (IsText){
                FixEmptyTexts(jump);
            }else{
                FixEmptyCharacters(jump);
            }
        }
        UpdatePointerLocation();
    }
    
    private void FixEmptyTexts(int jump)
    {
        while (!_textMenu[CurIndex].isActiveAndEnabled)
        {
            CurIndex+=jump;
            if (CurIndex == _maxIndex)
            {
                CurIndex = 0;
            }
            else if(CurIndex < 0)
            {
                CurIndex = _maxIndex - 1;
            }
        }
    }
    
    /**
     * Updates the pointer's location by the menuButton location of the current index.
     */
    private void UpdatePointerLocation()
    {
        if (IsText)
        {
            UpdatePositionByTexts();
            return;
        }
        UpdatePositionByObject();
    }

    private void UpdatePositionByTexts()
    {
        Vector3 pos = _textMenu[CurIndex].transform.TransformPoint(
            new Vector3(-_textMenu[CurIndex].rectTransform.rect.width / 2,
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
        SpriteRenderer image = _objects[CurIndex].GetComponent<SpriteRenderer>();
        Vector3 pos = image.transform.position;
        // pos.x -= (_spriteRenderer.bounds.size.x + image.sprite.rect.width) / 2;
        pos.x -= (_spriteRenderer.bounds.size.x+image.size.x) / 2;
        transform.position = pos;
    }

    public void SetNewTexts(TextMeshProUGUI[] newButtons,int numTextsInARow)
    {
        Assert.IsFalse(newButtons.Length == 0);
        IsText = true;
        _isMenu = true;
        _textMenu = newButtons;
        _numOfObjectsInARow = numTextsInARow;
        CurIndex = 0;
        _maxIndex = newButtons.Length;
        UpdatePointerLocation();
    }
    
    public void SetNewObjects(GameObject[] newObjects,int numObjectsInARow,bool isMenu)
    {
        Assert.IsFalse(newObjects.Length == 0);
        IsText = false;
        _isMenu = isMenu;
        _numOfObjectsInARow = numObjectsInARow;
        CurIndex = 0;
        _maxIndex = newObjects.Length;
        _objects = newObjects;
        FixEmptyCharacters(1);
        UpdatePointerLocation();
    }

}
