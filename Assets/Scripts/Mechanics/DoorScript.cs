using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DoorScript : MonoBehaviour
{
    private const string BAD_LEVEL_MSG = "The door won't budge, look for a key!";
    private int _counter;

    private MessageBoxScript _messageBoxScript;
    private SpriteRenderer _spriteRenderer;

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
                _messageBoxScript.ShowDialogs(dialogs, true);
            }

            return;
        }

        if (!isAllLevels && !GameManager.Instance.CompletedFightLevels.Contains(entranceNumber - 1))
        {
            dialogs = new[] { BAD_LEVEL_MSG };
            _messageBoxScript.ShowDialogs(dialogs, true);
            return;
        }

        // If the collider that entered the trigger has the "Player" tag, animate the door and load the new scene
        if (col.CompareTag(MySceneManager.k_PLAYER_TAG))
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            DisplayOpenDoor(openDoorSprite);
            StartCoroutine(LoadNextScene());
        }
    }

    /**
     * Triggers by after fight gameObject locations.
     */
    private IEnumerator HandleAfterFightSounds()
    {
        SoundManager.Instance.StopPlaying();
        SoundManager.Instance.PlayJoinedTheTeam();
        yield return new WaitForSeconds(5f);
        SoundManager.Instance.StopPlaying();
        SoundManager.Instance.PlayThemeByScene();
    }

    /**
     * Sets the Door sprite to open
     */
    private void DisplayOpenDoor(Sprite sprite)
    {
        _spriteRenderer.sprite = sprite;
    }

    /**
     * Loads the specified scene after a short delay
     */
    private IEnumerator LoadNextScene()
    {
        var rigidbody2D = MySceneManager.Instance.hero.GetComponent<Rigidbody2D>();
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(waitTimeBeforeTransition);
        MySceneManager.Instance.LoadNormalScene(entranceNumber, nextScene);
    }

    #region Inspector

    [Tooltip("Used to distinguish each door at the game.")]
    public int entranceNumber;

    public Sprite openDoorSprite;

    [Tooltip("flag to doors who gets opened at any time")] [SerializeField]
    private bool isAllLevels;

    [Tooltip("Dialogs to show at trigger")] [SerializeField]
    private string[] dialogs;

    [Tooltip("Scene name to load at trigger")] [SerializeField]
    private string nextScene;

    [SerializeField] private float waitTimeBeforeTransition;
    [SerializeField] private bool isTriggerOnce;

    #endregion
}