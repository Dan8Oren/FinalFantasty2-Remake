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
    public bool ActiveSequence { get; private set;}
    public bool IsGood { get; private set;}
    public Sprite[] oneSizeSprites;
    [Tooltip("Indexes should match the sprites above.")]
    public KeyCode[] oneSizeKeys;
    public Sprite[] twoSizeSprites;
    public KeyCode[] twoSizeKeys;
    public int countDownTime;
    public TextMeshPro countDownDisplay;
    [SerializeField] private GameObject oneSizePadPrefab;
    [SerializeField] private GameObject twoSizePadPrefab;

    [Header("===== Levels Fields =======")] 
    [SerializeField] private int level1Pads;
    [SerializeField] private int level1Speed;
    [SerializeField] private float level1MaxTime;
    [SerializeField] private float level1MinTime;
    [Range(0,100)]
    [SerializeField] private float level1ChanceForTwo;
    [Header("===== Level 2 =======")]
    [SerializeField] private int level2Pads;
    [SerializeField] private int level2Speed;
    [SerializeField] private float level2MaxTime;
    [SerializeField] private float level2MinTime;
    [Range(0,100)]
    [SerializeField] private float level2ChanceForTwo;
    [Header("===== Level 3 =======")]
    [SerializeField] private int level3Pads;
    [SerializeField] private int level3Speed;
    [SerializeField] private float level3MaxTime;
    [SerializeField] private float level3MinTime;
    [Range(0,100)]
    [SerializeField] private float level3ChanceForTwo;
    [Header("===== Level 4 =======")]
    [SerializeField] private int level4Pads;
    [SerializeField] private int level4Speed;
    [SerializeField] private float level4MaxTime;
    [SerializeField] private float level4MinTime;
    [Range(0,100)]
    [SerializeField] private float level4ChanceForTwo;
    [Header("===== Level 5 =======")]
    [SerializeField] private int level5Pads;
    [SerializeField] private int level5Speed;
    [SerializeField] private float level5MaxTime;
    [SerializeField] private float level5MinTime;
    [Range(0,100)]
    [SerializeField] private float level5ChanceForTwo;
    [Header("===== End of levels fields =======")]
    
    
    [SerializeField] private List<PadScript> pads;
    private HashSet<int> _oneSizeTaken;
    private HashSet<int> _twoSizeTaken;
    private int _goodTap;
    private Color _activeColor;
    private Image _imageRenderer;


    public int level = 0;//TODO: Remove me!
    private void Start()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(this);
        }
        Instance = this;
        ActiveSequence = false;
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
    }


    private void FixedUpdate()
    {
        if (ActiveSequence)
        {
            foreach (var pad in pads)
            {
                if (pad.isActiveAndEnabled)
                {
                    return;
                }
            }
            ActiveSequence = false;
            if(_goodTap == 0)
            {
                IsGood = true;
                StartCoroutine(FinishAndResult(SUCCESS_MSG));
                return;
            }
            IsGood = false;
            StartCoroutine(FinishAndResult(FAILED_MSG));
        }
    }

    private IEnumerator FinishAndResult(String result)
    {
        countDownDisplay.gameObject.SetActive(true);
        countDownDisplay.SetText(result);
        yield return new WaitForSeconds(0.5f);
        _imageRenderer.color = Color.clear;
        countDownDisplay.gameObject.SetActive(false);
        DestroyPads();
        //TODO: active back all other inputs.
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
        //TODO: disable here all other inputs.
        _imageRenderer.color = _activeColor;
        _goodTap = 0;
        ActiveSequence = true;
        pads = new List<PadScript>();
        _oneSizeTaken = new HashSet<int>();
        _twoSizeTaken = new HashSet<int>();
        switch (levelNum)
        {
            case 1:
                CreateLevel(level1Pads,level1Speed,level1MaxTime,level1MinTime,level1ChanceForTwo);
                break;
            case 2:
                CreateLevel(level2Pads,level2Speed,level2MaxTime,level2MinTime,level2ChanceForTwo);
                break;
            case 3:
                CreateLevel(level3Pads,level3Speed,level3MaxTime,level3MinTime,level3ChanceForTwo);
                break;
            case 4:
                CreateLevel(level4Pads,level4Speed,level4MaxTime,level4MinTime,level4ChanceForTwo);
                break;
            case 5:
                CreateLevel(level5Pads,level5Speed,level5MaxTime,level5MinTime,level5ChanceForTwo);
                break;
        }

        StartCoroutine(CountDownToSequence());
    }

    private void CreateLevel(int numOfPads,float speed, float maxTime,float minTime, float chanceForTwo)
    {
        int ind = 0;
        while(ind < numOfPads)
        {
            _goodTap--;
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
        float randomTime = Random.Range(startTime,endTime) + countDownTime;
        int maxSpawn = getAmountToSpawn(randomTime, endTime);
        script.SetPad(oneSizeKeys[rand],oneSizeSprites[rand],speed,randomTime,maxSpawn);
        return script;
    }

    private int getAmountToSpawn(float time, float endTime)
    {
        int maxNum = (int)Mathf.Ceil(endTime - time);
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
        float randomTime = Random.Range(startTime,endTime) + (countDownTime+1);
        int maxSpawn = getAmountToSpawn(randomTime, endTime);
        script.SetPad(twoSizeKeys[rand],twoSizeSprites[rand],speed,randomTime,maxSpawn);
        return script;
    }

    private IEnumerator CountDownToSequence()
    {
        countDownDisplay.gameObject.SetActive(true);
        countDownDisplay.SetText("Sequence starts in");
        yield return new WaitForSeconds(0.7f);
        while (countDownTime > 0)
        {
            countDownDisplay.SetText(countDownTime.ToString());
            yield return new WaitForSeconds(1f);
            countDownTime--;
        }
        countDownDisplay.SetText("Go");
        yield return new WaitForSeconds(0.3f);
        countDownDisplay.gameObject.SetActive(false);
    }

    public void GoodTapIncrease()
    {
        _goodTap++;
    }
}
