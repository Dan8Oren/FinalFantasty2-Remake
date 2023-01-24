using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SequenceManager : MonoBehaviour
{
    private const string FAILED_MSG = "Failed";
    private const string SUCCESS_MSG = "Success";
    private const string START_SEQUENCE_MSG = "Sequence starts in";
    public static SequenceManager Instance { get; private set; }


    private void Start()
    {
        if (Instance != null && this != Instance) Destroy(this);
        Instance = this;
        isSequenceActive = false;
        _imageRenderer = GetComponent<Image>();
        _activeColor = _imageRenderer.color;
        _imageRenderer.color = Color.clear;
    }

    private void Update()
    {
        if (isSequenceActive)
        {
            foreach (var pad in _pads)
                if (pad.isActiveAndEnabled)
                    return;
            isSequenceActive = false;
            var percentage = (int)((float)_goodTap / _allTaps * 100);
            if (percentage >= _curLevelPercentage)
            {
                IsGood = true;
                StartCoroutine(FinishAndResult(SUCCESS_MSG + $"\n {percentage}%"));
                return;
            }

            IsGood = false;
            StartCoroutine(FinishAndResult(FAILED_MSG + $"\n {percentage}%"));
        }
    }

    /**
     * Ends the sequence activity and display's the results.
     */
    private IEnumerator FinishAndResult(string result)
    {
        textDisplay.gameObject.SetActive(true);
        textDisplay.SetText(result);
        yield return new WaitForSeconds(1f);
        _imageRenderer.color = Color.clear;
        textDisplay.gameObject.SetActive(false);
        DestroyPads();
        completePercentageDisplay.gameObject.SetActive(false);
    }

    private void DestroyPads()
    {
        foreach (var pad in _pads) Destroy(pad.gameObject);
    }

    /**
     * Starts a Sequence event by a given level
     * <param name="levelNum"> level of the sequence (should be a number between 1-5 includes.)</param>
     * >
     */
    public void StartSequenceByLevel(int levelNum)
    {
        InitializeValues(levelNum);
        PointerBehavior.Instance.gameObject.SetActive(false);
        switch (levelNum)
        {
            case 1:
                _curLevelPercentage = level1CompletePercentage;
                CreateLevel(level1Pads, level1Speed, level1MaxTime, level1MinTime, level1ChanceForTwo);
                break;
            case 2:
                _curLevelPercentage = level2CompletePercentage;
                CreateLevel(level2Pads, level2Speed, level2MaxTime, level2MinTime, level2ChanceForTwo);
                break;
            case 3:
                _curLevelPercentage = level3CompletePercentage;
                CreateLevel(level3Pads, level3Speed, level3MaxTime, level3MinTime, level3ChanceForTwo);
                break;
            case 4:
                _curLevelPercentage = level4CompletePercentage;
                CreateLevel(level4Pads, level4Speed, level4MaxTime, level4MinTime, level4ChanceForTwo);
                break;
            case 5:
                _curLevelPercentage = level5CompletePercentage;
                CreateLevel(level5Pads, level5Speed, level5MaxTime, level5MinTime, level5ChanceForTwo);
                break;
        }

        completePercentageDisplay.SetText($"Percentage To Complete: {_curLevelPercentage}%");
        StartCoroutine(CountDownToSequence());
    }

    private void InitializeValues(int levelNum)
    {
        completePercentageDisplay.gameObject.SetActive(true);
        _finalCountDownTime = 1 + levelNum + defaultCountDownTime / 2;
        _imageRenderer.color = _activeColor;
        _goodTap = 0;
        _allTaps = 0;
        isSequenceActive = true;
        _pads = new List<PadScript>();
        _oneSizeTaken = new HashSet<int>();
        _twoSizeTaken = new HashSet<int>();
    }

    private void CreateLevel(int numOfPads, float speed, float maxTime, float minTime, float chanceForTwo)
    {
        var ind = 0;
        while (ind < numOfPads)
        {
            var rand = Random.Range(0, 100); //precentage
            if (rand > chanceForTwo || numOfPads - ind == 1)
            {
                var oneSize = GetRandomOneSizeKey(speed, maxTime, minTime);
                _pads.Add(oneSize);
                ind += 1;
                continue;
            }

            var twoSize = GetRandomTwoSizeKey(speed, maxTime, minTime);
            _pads.Add(twoSize);
            ind += 2;
        }
    }

    private PadScript GetRandomOneSizeKey(float speed, float endTime, float startTime)
    {
        int rand;
        do
        {
            rand = Random.Range(0, oneSizeSprites.Length);
        } while (_oneSizeTaken.Contains(rand));

        _oneSizeTaken.Add(rand);
        var obj = Instantiate(oneSizePadPrefab, transform);
        var script = obj.GetComponent<PadScript>();
        var randomTime = Random.Range(startTime, endTime);
        var maxSpawn = GetAmountToSpawn(randomTime, endTime);
        _allTaps += maxSpawn;
        script.SetPad(oneSizeKeys[rand], oneSizeSprites[rand], speed, randomTime, maxSpawn, _finalCountDownTime);
        return script;
    }

    private static int GetAmountToSpawn(float time, float endTime)
    {
        var maxNum = (int)Mathf.Ceil(endTime / time);
        return Random.Range(1, maxNum);
    }

    private PadScript GetRandomTwoSizeKey(float speed, float endTime, float startTime)
    {
        int rand;
        do
        {
            rand = Random.Range(0, twoSizeSprites.Length);
        } while (_twoSizeTaken.Contains(rand));

        _twoSizeTaken.Add(rand);
        var obj = Instantiate(twoSizePadPrefab, transform);
        var script = obj.GetComponent<PadScript>();
        var randomTime = Random.Range(startTime, endTime);
        var maxSpawn = GetAmountToSpawn(randomTime, endTime);
        _allTaps += maxSpawn;
        script.SetPad(twoSizeKeys[rand], twoSizeSprites[rand], speed, randomTime, maxSpawn, _finalCountDownTime);
        return script;
    }

    /**
     * Creates a countdown before the sequence.
     */
    private IEnumerator CountDownToSequence()
    {
        textDisplay.gameObject.SetActive(true);
        textDisplay.SetText(START_SEQUENCE_MSG);
        yield return new WaitForSeconds(afterFirstMessageTime);
        while (_finalCountDownTime > 0)
        {
            textDisplay.SetText(_finalCountDownTime.ToString());
            yield return new WaitForSeconds(1f);
            _finalCountDownTime--;
        }

        textDisplay.SetText("Go");
        yield return new WaitForSeconds(afterGoTime);
        textDisplay.gameObject.SetActive(false);
    }

    /**
     * Increases by 1 the number of good taps at this sequence event.
     */
    public void GoodTapIncrease()
    {
        _goodTap++;
    }

    #region Inspector

    public bool isSequenceActive;
    public bool IsGood { get; private set; }
    public Sprite[] oneSizeSprites;

    [Tooltip("Indexes should match the sprites above.")]
    public KeyCode[] oneSizeKeys;

    public Sprite[] twoSizeSprites;

    [Tooltip("Indexes should match the sprites above.")]
    public KeyCode[] twoSizeKeys;

    public int defaultCountDownTime;
    public TextMeshPro textDisplay;
    public TextMeshPro completePercentageDisplay;
    [SerializeField] private GameObject oneSizePadPrefab;
    [SerializeField] private GameObject twoSizePadPrefab;
    [SerializeField] private float afterFirstMessageTime = 0.7f;
    [SerializeField] private float afterGoTime = 0.3f;

    #region Sequance Levels Properties

    [Header("===== Levels Fields =======")] [SerializeField]
    private int level1Pads;

    [SerializeField] private int level1Speed;
    [SerializeField] private float level1MaxTime;
    [SerializeField] private float level1MinTime;

    [Range(0, 100)] [SerializeField] private int level1ChanceForTwo;

    [Range(0, 100)] [SerializeField] private int level1CompletePercentage;

    [Header("===== Level 2 =======")] [SerializeField]
    private int level2Pads;

    [SerializeField] private int level2Speed;
    [SerializeField] private float level2MaxTime;
    [SerializeField] private float level2MinTime;

    [Range(0, 100)] [SerializeField] private float level2ChanceForTwo;

    [Range(0, 100)] [SerializeField] private int level2CompletePercentage;

    [Header("===== Level 3 =======")] [SerializeField]
    private int level3Pads;

    [SerializeField] private int level3Speed;
    [SerializeField] private float level3MaxTime;
    [SerializeField] private float level3MinTime;

    [Range(0, 100)] [SerializeField] private float level3ChanceForTwo;

    [Range(0, 100)] [SerializeField] private int level3CompletePercentage;

    [Header("===== Level 4 =======")] [SerializeField]
    private int level4Pads;

    [SerializeField] private int level4Speed;
    [SerializeField] private float level4MaxTime;
    [SerializeField] private float level4MinTime;

    [Range(0, 100)] [SerializeField] private float level4ChanceForTwo;

    [Range(0, 100)] [SerializeField] private int level4CompletePercentage;

    [Header("===== Level 5 =======")] [SerializeField]
    private int level5Pads;

    [SerializeField] private int level5Speed;
    [SerializeField] private float level5MaxTime;
    [SerializeField] private float level5MinTime;

    [Range(0, 100)] [SerializeField] private float level5ChanceForTwo;

    [Range(0, 100)] [SerializeField] private int level5CompletePercentage;

    [Header("===== End of levels fields =======")]

    #endregion

    #endregion

    #region Private Fields

    private List<PadScript> _pads;

    private HashSet<int> _oneSizeTaken;
    private HashSet<int> _twoSizeTaken;
    private int _allTaps;
    private int _goodTap;
    private Color _activeColor;
    private Image _imageRenderer;
    private int _finalCountDownTime;
    private int _curLevelPercentage;

    #endregion
}