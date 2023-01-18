using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public const string k_YELLOW_FLOOR = "YellowFloor";
    public const string k_FIGHT = "FightScene";
    public const string k_PLAYER_TAG = "Player";
    private const string DOORS_TAG = "Doors";
    private const string DISABLE_AFTER_FIGHT_TAG = "DisableAfterFight";
    private const string FIGHT_TRIGGER_TAG = "FightTrigger";
    public static MySceneManager Instance = null;
    public GameObject hero;
    public GameObject mainCamera;
    public GameObject cmCamera;
    public GameObject[] doors;
    public string LastSceneName;  //{ get; private set; }
    [SerializeField] private float afterMovmentDelay;
    public bool IsInFight; //{ get; private set; }
    public int CurrentEntrance; //{ get; private set; }
    private CinemachineVirtualCamera _cmCamera;
    
    // private Scene _fightScene; 
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
        LastSceneName = SceneManager.GetActiveScene().name;
        _cmCamera = cmCamera.GetComponent<CinemachineVirtualCamera>();
        IsInFight = false;
        CurrentEntrance = -1;
        //By convention all-ways the last one. (my convention :P)
        // _fightScene = SceneManager.GetSceneByName(k_FIGHT);
    }

    private void OnRegularLevelLoad()
    {
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
    
    public void LoadScene(int entrance,String sceneToLoad)
    {
        CurrentEntrance = entrance;
        if (!sceneToLoad.Equals(k_FIGHT))
        {
            LastSceneName = SceneManager.GetActiveScene().name;
        }
        SceneManager.LoadScene(sceneToLoad);
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
                //fightScene
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
}
