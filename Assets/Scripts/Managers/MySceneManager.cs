using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Instance = null;
    public GameObject hero;
    public GameObject[] doors;
    [SerializeField] private float afterMovmentDelay;
    public int CurrentEntrance { get; private set; }
    
    private Scene _fightSceneNum; 
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
        CurrentEntrance = -1;
        //By convention all-ways the last one. (my convention :P)
        _fightSceneNum = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
    }

    private void OnRegularLevelLoad()
    {
        hero = GameObject.FindGameObjectWithTag("Player");
        doors = GameObject.FindGameObjectsWithTag("Doors");
        if (CurrentEntrance == -1)
        {
            return;
        }
        for (int i = 0; i < doors.Length; i++)
        {
            if (doors[i].GetComponent<DoorScript>().entranceNumber == CurrentEntrance)
            {
                Vector3 pos = doors[i].transform.position;
                pos.y -= 0.5f;
                hero.transform.position = pos;
                StartCoroutine(DeactivateRespawnedDoor(doors[i]));
                return;
            }
        }
    }

    private IEnumerator DeactivateRespawnedDoor(GameObject door)
    {
        door.SetActive(false);
        yield return new WaitUntil(() => Input.anyKey);
        yield return new WaitForSeconds(afterMovmentDelay);
        door.SetActive(true);
    }
    
    public void LoadScene(int entrance,String sceneToLoad)
    {
        CurrentEntrance = entrance;
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
        if (scene != _fightSceneNum)
        {
            OnRegularLevelLoad();
        }
    }
}
