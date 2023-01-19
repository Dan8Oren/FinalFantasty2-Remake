using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MySceneManager : MonoBehaviour
{
    public const string k_YELLOW_FLOOR = "YellowFloor";
    public const string k_FIGHT = "FightScene";
    public const string k_PLAYER_TAG = "Player";
    private const string DOORS_TAG = "Doors";
    private const string DISABLE_AFTER_FIGHT_TAG = "DisableAfterFight";
    private const string FIGHT_TRIGGER_TAG = "FightTrigger";
    private const string FIGHT_CAMERA_TAG = "Fight Camera";
    public static MySceneManager Instance = null;
    public GameObject hero;
    public GameObject mainCamera;
    public GameObject cmCamera;
    public GameObject fightCamera;
    public GameObject[] doors;
    public MessageBoxScript messageBoxScript { get; private set; }
    public Rigidbody2D HeroRigid { get; private set; }
    public string LastSceneName { get; private set; }
    [SerializeField] private float afterMovmentDelay;

    [SerializeField] private Animator regularSceneAnimator;
    [SerializeField] private float regTransitionWaitTime;
    [SerializeField] private float fightTransitionWaitTime;
    public bool IsInFight { get; private set; }
    public int CurrentEntrance { get; private set; }
    private CinemachineVirtualCamera _cmCamera;
    
    private void Awake()
    {
        //singleton pattern the prevent two scene managers
        if ( Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
           Destroy(gameObject); 
        }
        HeroRigid = hero.GetComponent<Rigidbody2D>();
        regularSceneAnimator.SetTrigger("Start");
        LastSceneName = SceneManager.GetActiveScene().name;
        _cmCamera = cmCamera.GetComponent<CinemachineVirtualCamera>();
        IsInFight = false;
        CurrentEntrance = -1;
    }

    private void OnRegularLevelLoad()
    {
        HeroRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        regularSceneAnimator.SetTrigger("Start");
        if (_cmCamera.Follow == null)
        {
            _cmCamera.Follow = hero.transform;
        }
        doors = GameObject.FindGameObjectsWithTag(DOORS_TAG);
        var fightTrigger = GameObject.FindWithTag(FIGHT_TRIGGER_TAG);
        if (CurrentEntrance == -1)
        {
            return;
        }

        if (GameManager.Instance.FightLevel > CurrentEntrance)
        {
            foreach (var obj in GameObject.FindGameObjectsWithTag(DISABLE_AFTER_FIGHT_TAG))
            {
                obj.SetActive(false);
            }
        
            if (fightTrigger != null)
            {
                fightTrigger.SetActive(false);
                fightTrigger = null;
            }
        }
        
        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].GetComponent<DoorScript>().entranceNumber == CurrentEntrance)
            {
                Vector3 pos = doors[i].transform.position;
                pos.y += 0.5f;
                if ( SceneManager.GetActiveScene().name == k_YELLOW_FLOOR)
                {
                    pos.y -= 1f;
                }
                print(SceneManager.GetActiveScene().name);
                Assert.IsFalse(hero == null);
                hero.transform.position = pos;
                mainCamera.transform.position = pos;
                StartCoroutine(DeactivateUntilMovement(doors[i]));
                if (fightTrigger != null)
                {
                    StartCoroutine(DeactivateUntilMovement(fightTrigger));
                }
                return;
            }
        }
    }

    private IEnumerator DeactivateUntilMovement(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            yield return new WaitUntil(() => Input.anyKey);
            yield return new WaitForSeconds(afterMovmentDelay);
            obj.SetActive(true);
        }
    }

    private IEnumerator LoadAfterTransition(float duration,String sceneToLoad)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(sceneToLoad);
    }
    
    
    public void LoadScene(int entrance,String sceneToLoad)
    {
        if (!sceneToLoad.Equals(k_FIGHT))
        {
            LastSceneName = SceneManager.GetActiveScene().name;
            CurrentEntrance = entrance;
            regularSceneAnimator.SetTrigger("End");
            StartCoroutine(LoadAfterTransition(regTransitionWaitTime,sceneToLoad));
            return;
        }
        //TODO: fight transition
        StartCoroutine(LoadAfterTransition(fightTransitionWaitTime,sceneToLoad));
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }
    
    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("============Level Loaded==============");
        Debug.Log(scene.name); //TODO: REMOVE ME!
        hero.SetActive(true);
        mainCamera.SetActive(true);
        if (!scene.name.Equals(k_FIGHT) && !IsInFight)
        {
            OnRegularLevelLoad();
        }
        else
        {
            IsInFight = !IsInFight;
            if (IsInFight)
            {
                hero.SetActive(false);
                mainCamera.SetActive(false);
                OnFightSceneLoad();
                return;
            }
            OnRegularLevelLoad();
        }
    }
    
    public void FightWon()
    {
     print("Won");   
        //adds loot to player.
        GameManager.Instance.FightWon();
        foreach (var obj in GameObject.FindGameObjectsWithTag(DISABLE_AFTER_FIGHT_TAG))
        {
            obj.SetActive(false);
        }
    }

    private void OnFightSceneLoad()
    {
        GameObject obj = GameObject.FindGameObjectWithTag(FIGHT_CAMERA_TAG);
        Assert.IsFalse(obj == null);
        fightCamera = obj;
        // obj.GetComponent<Camera>();
    }
    

    public IEnumerator Shake( float duration, float force)
    {
        Transform trans = (IsInFight)?fightCamera.transform:cmCamera.transform;
        Vector3 originalPos = trans.position;
        float activeTime = 0f;
        while (activeTime <= duration)
        {
            float xDistortion = Random.Range(-0.5f, 0.5f) * force;
            float yDistortion = Random.Range(-0.5f, 0.5f) * force;
            trans.localPosition = new Vector3(xDistortion, yDistortion, originalPos.z);
            activeTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
    }
}
