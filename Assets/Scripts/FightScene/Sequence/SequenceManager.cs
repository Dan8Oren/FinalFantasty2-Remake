using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SequenceManager : MonoBehaviour
{
    private const string FAILED_MSG = "Failed";
    private const string SUCCESS_MSG = "Success";
    public static SequenceManager Instance { get; private set;}
    public bool isSequenceActive;
    public bool IsGood { get; private set;}
    public Sprite[] oneSizeSprites;
    [Tooltip("Indexes should match the sprites above.")]
    public KeyCode[] oneSizeKeys;
    public Sprite[] twoSizeSprites;
    public KeyCode[] twoSizeKeys;
    public int defaultCountDownTime;
    public TextMeshPro textDisplay;
    public TextMeshPro completePercentageDisplay;
    [SerializeField] private GameObject oneSizePadPrefab;
    [SerializeField] private GameObject twoSizePadPrefab;

    [Header("===== Levels Fields =======")] 
    [SerializeField] private int level1Pads;
    [SerializeField] private int level1Speed;
    [SerializeField] private float level1MaxTime;
    [SerializeField] private float level1MinTime;
    [Range(0,100)]
    [SerializeField] private int level1ChanceForTwo;
    [Range(0,100)]
    [SerializeField] private int level1CompletePercentage;
    [Header("===== Level 2 =======")]
    [SerializeField] private int level2Pads;
    [SerializeField] private int level2Speed;
    [SerializeField] private float level2MaxTime;
    [SerializeField] private float level2MinTime;
    [Range(0,100)]
    [SerializeField] private float level2ChanceForTwo;
    [Range(0,100)]
    [SerializeField] private int level2CompletePercentage;
    [Header("===== Level 3 =======")]
    [SerializeField] private int level3Pads;
    [SerializeField] private int level3Speed;
    [SerializeField] private float level3MaxTime;
    [SerializeField] private float level3MinTime;
    [Range(0,100)]
    [SerializeField] private float level3ChanceForTwo;
    [Range(0,100)]
    [SerializeField] private int level3CompletePercentage;
    [Header("===== Level 4 =======")]
    [SerializeField] private int level4Pads;
    [SerializeField] private int level4Speed;
    [SerializeField] private float level4MaxTime;
    [SerializeField] private float level4MinTime;
    [Range(0,100)]
    [SerializeField] private float level4ChanceForTwo;
    [Range(0,100)]
    [SerializeField] private int level4CompletePercentage;
    [Header("===== Level 5 =======")]
    [SerializeField] private int level5Pads;
    [SerializeField] private int level5Speed;
    [SerializeField] private float level5MaxTime;
    [SerializeField] private float level5MinTime;
    [Range(0,100)]
    [SerializeField] private float level5ChanceForTwo;
    [Range(0,100)]
    [SerializeField] private int level5CompletePercentage;
    [Header("===== End of levels fields =======")]
    
    
    [SerializeField] private List<PadScript> pads;
    private HashSet<int> _oneSizeTaken;
    private HashSet<int> _twoSizeTaken;
    private int _allTaps;
    private int _goodTap;
    private Color _activeColor;
    private Image _imageRenderer;
    private int _finalCountDownTime;
    private int _curLevelPercentage;


    public int level = 0;//TODO: Remove me!
    

    private void Start()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(this);
        }
        Instance = this;
        isSequenceActive = false;
        _imageRenderer = GetComponent<Image>();
        _activeColor = _imageRenderer.color;
        _imageRenderer.color = Color.clear;
    }
    
    //TODO: remove me
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartSequenceByLevel(level);
        }
        if (isSequenceActive)
        {
            foreach (var pad in pads)
            {
                if (pad.isActiveAndEnabled)
                {
                    return;
                }
            }
            isSequenceActive = false;
            int percentage = (int)(((float)_goodTap / _allTaps) * 100);
            if(percentage >= _curLevelPercentage)
            {
                IsGood = true;
                StartCoroutine(FinishAndResult(SUCCESS_MSG+$"\n {percentage}%"));
                return;
            }
            IsGood = false;
            StartCoroutine(FinishAndResult(FAILED_MSG+$"\n {percentage}%"));
        }
    }
    

    private IEnumerator FinishAndResult(String result)
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
        foreach (var pad in pads)
        {
            Destroy(pad.gameObject);
        }
    }

    public void StartSequenceByLevel(int levelNum)
    {
        InitializeValues(levelNum);
        PointerBehavior.Instance.gameObject.SetActive(false);
        switch (levelNum)
        {
            case 1:
                _curLevelPercentage = level1CompletePercentage;
                CreateLevel(level1Pads,level1Speed,level1MaxTime,level1MinTime,level1ChanceForTwo);
                break;
            case 2:
                _curLevelPercentage = level2CompletePercentage;
                CreateLevel(level2Pads,level2Speed,level2MaxTime,level2MinTime,level2ChanceForTwo);
                break;
            case 3:
                _curLevelPercentage = level3CompletePercentage;
                CreateLevel(level3Pads,level3Speed,level3MaxTime,level3MinTime,level3ChanceForTwo);
                break;
            case 4:
                _curLevelPercentage = level4CompletePercentage;
                CreateLevel(level4Pads,level4Speed,level4MaxTime,level4MinTime,level4ChanceForTwo);
                break;
            case 5:
                _curLevelPercentage = level5CompletePercentage;
                CreateLevel(level5Pads,level5Speed,level5MaxTime,level5MinTime,level5ChanceForTwo);
                break;
        }
        completePercentageDisplay.SetText($"Percentage To Complete: {_curLevelPercentage}%");
        StartCoroutine(CountDownToSequence());
    }

    private void InitializeValues(int levelNum)
    {
        completePercentageDisplay.gameObject.SetActive(true);
        _finalCountDownTime = 1 + levelNum + (defaultCountDownTime / 2);
        _imageRenderer.color = _activeColor;
        _goodTap = 0;
        _allTaps = 0;
        isSequenceActive = true;
        pads = new List<PadScript>();
        _oneSizeTaken = new HashSet<int>();
        _twoSizeTaken = new HashSet<int>();
    }

    private void CreateLevel(int numOfPads,float speed, float maxTime,float minTime, float chanceForTwo)
    {
        int ind = 0;
        while(ind < numOfPads)
        {
            int rand = Random.Range(0, 100);
            if (rand > chanceForTwo || (numOfPads - ind) == 1 )
            {
                PadScript oneSize = GetRandomOneSizeKey(speed,maxTime,minTime);
                pads.Add(oneSize);
                ind += 1;
                continue;
            }
            PadScript twoSize = GetRandomTwoSizeKey(speed,maxTime,minTime);
            pads.Add(twoSize);
            ind += 2;
        }
    }

    private PadScript GetRandomOneSizeKey(float speed,float endTime,float startTime)
    {
        int rand;
        do{
            rand = Random.Range(0, oneSizeSprites.Length);
        } while (_oneSizeTaken.Contains(rand));
        
        _oneSizeTaken.Add(rand);
        GameObject obj = Instantiate(oneSizePadPrefab, transform);
        PadScript script = obj.GetComponent<PadScript>();
        float randomTime = Random.Range(startTime, endTime);
        int maxSpawn = GetAmountToSpawn(randomTime, endTime);
        _allTaps += maxSpawn;
        script.SetPad(oneSizeKeys[rand],oneSizeSprites[rand],speed,randomTime,maxSpawn,_finalCountDownTime);
        return script;
    }

    private static int GetAmountToSpawn(float time, float endTime)
    {
        int maxNum = (int)Mathf.Ceil(endTime/time);
        return Random.Range(1, maxNum);
    }

    private PadScript GetRandomTwoSizeKey(float speed,float endTime,float startTime)
    {
        int rand;
        do{
            rand = Random.Range(0, twoSizeSprites.Length);
        } while (_twoSizeTaken.Contains(rand));
        _twoSizeTaken.Add(rand);
        GameObject obj = Instantiate(twoSizePadPrefab, transform);
        PadScript script = obj.GetComponent<PadScript>();
        float randomTime = Random.Range(startTime,endTime);
        int maxSpawn = GetAmountToSpawn(randomTime, endTime);
        _allTaps += maxSpawn;
        script.SetPad(twoSizeKeys[rand],twoSizeSprites[rand],speed,randomTime,maxSpawn,_finalCountDownTime);
        return script;
    }

    private IEnumerator CountDownToSequence()
    {
        textDisplay.gameObject.SetActive(true);
        textDisplay.SetText("Sequence starts in");
        yield return new WaitForSeconds(0.7f);
        while (_finalCountDownTime > 0)
        {
            textDisplay.SetText(_finalCountDownTime.ToString());
            yield return new WaitForSeconds(1f);
            _finalCountDownTime--;
        }
        textDisplay.SetText("Go");
        yield return new WaitForSeconds(0.3f);
        textDisplay.gameObject.SetActive(false);
    }

    public void GoodTapIncrease()
    {
        _goodTap++;
    }
}
