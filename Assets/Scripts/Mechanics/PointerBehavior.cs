using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(SpriteRenderer))]
public class PointerBehavior : MonoBehaviour
{
    public static PointerBehavior Instance;
    public bool disableSpace;

    private int _firstHeroIndex;
    private bool _isMenu;
    private int _maxIndex;
    private int _numOfObjectsInARow;
    private GameObject[] _objects;
    private SpriteRenderer _spriteRenderer;
    private TextMeshProUGUI[] _textMenu;

    //a flag to indicate if the pointer points at Game-object or text.
    public bool IsText { get; private set; }

    //the current index the pointer points at.
    public int CurIndex { get; private set; }

    //last Gameobject selected by the pointer
    public GameObject SelectedObj { get; set; }

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

    private void Update()
    {
        if (_isMenu)
            MenuInputHandler();
        else
            CharactersInputHandler();
        if (Input.GetKeyDown(KeyCode.Space) && !disableSpace)
        {
            if (!MySceneManager.Instance.IsInFight)
            {
                SelectedObj = _objects[CurIndex];
                return;
            }

            if (IsText)
                FightManager.Instance.DoChosenAction(_textMenu[CurIndex].name);
            else
                FightManager.Instance.SetSelectedObject(_objects[CurIndex]);
        }
    }

    /**
     * Updates the pointer position by the player's input to a new Character at the fight Scene.
     * (tailored specifically to the fight scene in accordance to the real game.
     */
    private void CharactersInputHandler()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) && CurIndex + 1 < _maxIndex)
        {
            CurIndex++;
            FixEmptyCharacters(1);
            UpdatePointerLocation();
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) && CurIndex - 1 >= 0)
        {
            CurIndex--;
            FixEmptyCharacters(-1);
            UpdatePointerLocation();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CurIndex += _numOfObjectsInARow;
            if (CurIndex >= _maxIndex) CurIndex -= _maxIndex;
            FixEmptyCharacters(1);
            UpdatePointerLocation();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CurIndex -= _numOfObjectsInARow;
            if (CurIndex < 0) CurIndex *= -1;
            FixEmptyCharacters(-1);
            UpdatePointerLocation();
        }
    }

    /**
     * Setting the pointer next index by the jump value skipping all Game objects that are inactive.
     */
    private void FixEmptyCharacters(int jump)
    {
        while (!_objects[CurIndex].activeSelf)
        {
            CurIndex += jump;
            if (CurIndex == _maxIndex)
                CurIndex = 0;
            else if (CurIndex < 0) CurIndex = _maxIndex - 1;
        }
    }

    /**
     * Updates the pointer position by the player's input.
     */
    private void MenuInputHandler()
    {
        var jump = 0;
        if (Input.GetKeyDown(KeyCode.RightArrow) && CurIndex + 1 < _maxIndex)
        {
            CurIndex++;
            jump = 1;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && CurIndex - 1 >= 0)
        {
            CurIndex--;
            jump = -1;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) && CurIndex + _numOfObjectsInARow < _maxIndex)
        {
            CurIndex += _numOfObjectsInARow;
            jump = 1;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && CurIndex - _numOfObjectsInARow >= 0)
        {
            CurIndex -= _numOfObjectsInARow;
            jump = -1;
        }

        if (jump != 0)
        {
            if (IsText)
                FixEmptyTexts(jump);
            else
                FixEmptyCharacters(jump);
        }

        UpdatePointerLocation();
    }

    /**
     * Setting the pointer next index by the jump value skipping all TextMeshProUGUI that are inactive.
     */
    private void FixEmptyTexts(int jump)
    {
        while (!_textMenu[CurIndex].isActiveAndEnabled)
        {
            CurIndex += jump;
            if (CurIndex == _maxIndex)
                CurIndex = 0;
            else if (CurIndex < 0) CurIndex = _maxIndex - 1;
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

    /**
     * Sets the pointer's world position by the TextMeshProUGUI it points at.
     */
    private void UpdatePositionByTexts()
    {
        var pos = _textMenu[CurIndex].transform.TransformPoint(
            new Vector3(-_textMenu[CurIndex].rectTransform.rect.width / 2,
                0, 0));
        if (pos.x > 0)
            pos.x -= _spriteRenderer.bounds.size.x / 2;
        else
            pos.x += _spriteRenderer.bounds.size.x / 2;

        transform.position = pos;
    }

    /**
     * Sets the pointer's world position by the object it points at.
     */
    private void UpdatePositionByObject()
    {
        var image = _objects[CurIndex].GetComponent<SpriteRenderer>();
        var pos = image.transform.position;
        pos.x -= (_spriteRenderer.bounds.size.x + image.size.x) / 2;
        transform.position = pos;
    }

    /**
     * Sets new TextMeshProUGUI to point at, at points at the first active object at the array.
     */
    public void SetNewTexts(TextMeshProUGUI[] newButtons, int numTextsInARow)
    {
        Assert.IsFalse(newButtons.Length == 0);
        IsText = true;
        _isMenu = true;
        _textMenu = newButtons;
        _numOfObjectsInARow = numTextsInARow;
        CurIndex = 0;
        _maxIndex = newButtons.Length;
        FixEmptyTexts(1);
        UpdatePointerLocation();
    }

    /**
     * Sets new Objects to point at, at points at the first active object at the array.
     */
    public void SetNewObjects(GameObject[] newObjects, int numObjectsInARow, bool isMenu)
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