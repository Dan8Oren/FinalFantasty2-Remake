using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class PointerBehavior : MonoBehaviour
{
    public static PointerBehavior Instance = null;
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
            return;
        }
        Destroy(gameObject);
    }

    // private void OnEnable()
    // {
    //     _textMenu = FightManager.Instance.mainFightMenuButtons;
    //     _numOfButtonsInARow = FightManager.Instance.numOfButtonsInARow;
    //     _curIndex = 0;
    //     UpdatePointerLocation();
    // }

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
            if (_isText)
            {
                FightManager.Instance.DoChosenAction(_textMenu[_curIndex].text);
            }
            else
            {
                FightManager.Instance.SetSelectedObject(_objects[_curIndex].name);
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
                _curIndex += _maxIndex;
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
        }
    }

    private void MenuInputHandler()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && _curIndex+1 < _maxIndex)
        {
            _curIndex++;
            UpdatePointerLocation();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)&& _curIndex-1 >= 0)
        {
            _curIndex--;
            UpdatePointerLocation();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && _curIndex+_numOfObjectsInARow<_maxIndex)
        {
            _curIndex+=_numOfObjectsInARow;
            UpdatePointerLocation();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && _curIndex-_numOfObjectsInARow >= 0)
        {
            _curIndex-=_numOfObjectsInARow;
            UpdatePointerLocation();
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
        // float val = -image.sprite.bounds.size.x;
        // if (pos.x > 0)
        // {
        //     pos.x -= val - _spriteRenderer.bounds.size.x;
        // }
        // else
        // {
        //     pos.x += val + _spriteRenderer.bounds.size.x;
        // }

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
        UpdatePointerLocation();
    }
}
