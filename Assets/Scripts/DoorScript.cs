using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : MonoBehaviour
{
    private const string BAD_LEVEL_MSG = "The door won't budge, look for a key!";
    private const string DOOR_UNLOCKED_MSG = "You used the key to unlock the door!";
    public int entranceNumber;
    public Sprite openDoorSprite;
    [SerializeField] private bool isAllLevels = false;
    [SerializeField] private String[] dialogs;
    [SerializeField] private string nextScene;
    [SerializeField] private float waitTimeBeforeTransition;
    [SerializeField] private bool isTriggerOnce = false;
    private MessageBoxScript _messageBoxScript;
    private SpriteRenderer _spriteRenderer;
    private int _counter;

    private void Start()
    {
        _messageBoxScript = MySceneManager.Instance.messageBoxScript;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (isTriggerOnce) //after Fight Message..
        {
            _counter++;
            if (_counter == 1)
            {
                StartCoroutine(HandleAfterFightSounds());
                _messageBoxScript.ShowDialogs(dialogs,true);
            }
            return;
        }
        if (!isAllLevels &&  !GameManager.Instance.CompletedFightLevels.Contains(entranceNumber-1))
        {
            dialogs = new string[] { BAD_LEVEL_MSG };
            _messageBoxScript.ShowDialogs(dialogs,true);
            return;
        }
        
        // If the collider that entered the trigger has the "Player" tag, animate the door and load the new scene
        if (col.CompareTag(MySceneManager.k_PLAYER_TAG))
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            AnimateDoor(openDoorSprite);
            StartCoroutine(LoadNextScene());
        }
    }

    private IEnumerator HandleAfterFightSounds()
    {
        SoundManager.Instance.StopPlaying();
        SoundManager.Instance.PlayJoinedTheTeam();
        yield return new WaitForSeconds(5f);
        SoundManager.Instance.StopPlaying();
        SoundManager.Instance.PlayThemeByScene();
    }

    // Plays the Door open animation
    private void AnimateDoor(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }

    // Loads the specified scene after a short delay
    private IEnumerator LoadNextScene()
    {
        Rigidbody2D rigidbody2D = MySceneManager.Instance.hero.GetComponent<Rigidbody2D>();
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(waitTimeBeforeTransition);
        MySceneManager.Instance.LoadNormalScene(entranceNumber,nextScene);
    }
}