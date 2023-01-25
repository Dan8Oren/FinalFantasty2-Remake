using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class MySceneManager : MonoBehaviour
{
    public static MySceneManager Instance;

    private bool _isRun;

    public Rigidbody2D HeroRigid { get; private set; }
    public bool IsInFight { get; private set; }
    public int CurrentEntrance { get; private set; }
    public string LastSceneName { get; private set; }

    private void Awake()
    {
        //singleton pattern the prevent two scene managers
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _isRun = false;
        HeroRigid = hero.GetComponent<Rigidbody2D>();
        regularSceneAnimator.SetTrigger("Start");
        LastSceneName = SceneManager.GetActiveScene().name;
        IsInFight = false;
        CurrentEntrance = -1;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    private void OnRegularLevelLoad()
    {
        HeroRigid.constraints = RigidbodyConstraints2D.FreezeRotation;
        regularSceneAnimator.SetTrigger("Start");
        if (cmCamera.Follow == null) cmCamera.Follow = hero.transform;
        doors = GameObject.FindGameObjectsWithTag(DOORS_TAG);
        if (CurrentEntrance == -1) return;
        UpdateSceneObjectsByProgress();
        SpawnHeroAtLocation();
    }

    /**
     * disables game-object's after the fight at the scene has all-ready happend.
     */
    private void UpdateSceneObjectsByProgress()
    {
        if (GameManager.Instance.CompletedFightLevels.Contains(CurrentEntrance)
            || LastSceneName.Equals(k_FIGHT))
            foreach (var obj in GameObject.FindGameObjectsWithTag(DISABLE_AFTER_FIGHT_TAG))
                obj.SetActive(false);
    }

    /**
     * Spawns the hero at the specific door entrance location at the scene
     */
    private void SpawnHeroAtLocation()
    {
        for (var i = 0; i < doors.Length; i++)
        {
            var doorScript = doors[i].GetComponent<DoorScript>();
            if (doorScript.entranceNumber == CurrentEntrance)
            {
                var pos = doors[i].transform.position;
                pos.y += 0.5f;
                if (SceneManager.GetActiveScene().name == k_YELLOW_FLOOR) pos.y -= 1f;

                hero.transform.position = pos;
                mainCamera.transform.position = pos;
                if (!LastSceneName.Equals(k_FIGHT) || _isRun) StartCoroutine(DeactivateUntilMovement(doors[i]));

                return;
            }
        }
    }

    /**
     * Deactivates an object for (afterMovementDelay field's) time after the player made any input.
     * Used to deactivate doors trigger when spawning.
     */
    private IEnumerator DeactivateUntilMovement(GameObject obj)
    {
        if (obj != null)
        {
            obj.SetActive(false);
            yield return new WaitUntil(() =>
                (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.UpArrow) ||
                 Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow)) &&
                    !InventoryManager.Instance.IsOpen);
            yield return new WaitForSeconds(afterMovementDelay);
            obj.SetActive(true);
        }
    }

    /**
     * Delay's the load of a given scene name by a given duration.
     */
    private IEnumerator LoadAfterTransition(float duration, string sceneToLoad)
    {
        yield return new WaitForSeconds(duration);
        SceneManager.LoadScene(sceneToLoad);
    }


    public void LoadNormalScene(int entrance, string sceneToLoad, bool isRun = false)
    {
        _isRun = isRun;
        LastSceneName = SceneManager.GetActiveScene().name;
        CurrentEntrance = entrance;
        regularSceneAnimator.SetTrigger("End");
        StartCoroutine(LoadAfterTransition(regTransitionWaitTime, sceneToLoad));
    }

    public void LoadFightScene(int fightLevel)
    {
        LastSceneName = SceneManager.GetActiveScene().name;
        GameManager.Instance.SetFightLevel(fightLevel);
        StartCoroutine(LoadAfterTransition(fightTransitionWaitTime, k_FIGHT));
    }

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        SoundManager.Instance.PlayThemeByScene();
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

    private void OnFightSceneLoad()
    {
        var obj = GameObject.FindGameObjectWithTag(FIGHT_CAMERA_TAG);
        Assert.IsFalse(obj == null);
        fightCamera = obj;
    }

    /**
     * Animates a screen shake of the current game's camera.
     */
    public IEnumerator Shake(float duration, float force)
    {
        var trans = IsInFight ? fightCamera.transform : cmCameraObject.transform;
        var originalPos = trans.position;
        var activeTime = 0f;
        while (activeTime <= duration)
        {
            var xDistortion = Random.Range(-0.5f, 0.5f) * force;
            var yDistortion = Random.Range(-0.5f, 0.5f) * force;
            trans.localPosition = new Vector3(xDistortion, yDistortion, originalPos.z);
            activeTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
    }

    #region Constants

    public const string k_YELLOW_FLOOR = "YellowFloor";
    public const string k_FIGHT = "FightScene";
    public const string k_PLAYER_TAG = "Player";
    private const string DOORS_TAG = "Doors";
    private const string DISABLE_AFTER_FIGHT_TAG = "DisableAfterFight";
    private const string FIGHT_CAMERA_TAG = "Fight Camera";

    #endregion

    #region Inspector

    public GameObject hero;
    public GameObject mainCamera;
    public GameObject cmCameraObject;
    public CinemachineVirtualCamera cmCamera;
    public GameObject fightCamera;
    public GameObject[] doors;
    public MessageBoxScript messageBoxScript;

    [SerializeField] private float afterMovementDelay;
    [SerializeField] private Animator regularSceneAnimator;
    [SerializeField] private float regTransitionWaitTime;
    [SerializeField] private float fightTransitionWaitTime;

    #endregion
}