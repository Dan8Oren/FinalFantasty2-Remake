using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : MonoBehaviour
{
    private const string BAD_LEVEL_MSG = "\t \t The door won't budge, look for a key!";
    private const string DOOR_UNLOCKED_MSG = "\t \t You used the key to unlock the door!";
    public int entranceNumber;
    public Sprite openDoorSprite;
    [SerializeField] private bool isAllLevels = false;
    [SerializeField] private String[] dialogs;
    [SerializeField] private string nextScene;
    [SerializeField] private float waitTimeBeforeTransition;
    [SerializeField] private MessageBoxScript messageBoxScript = null;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        messageBoxScript = MySceneManager.Instance.messageBoxScript;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!isAllLevels && entranceNumber > GameManager.Instance.FightLevel)
        {
            dialogs = new string[] { BAD_LEVEL_MSG };
            messageBoxScript.ShowDialogs(dialogs,true);
            return;
        }

        if (!isAllLevels)
        {
            dialogs = new string[] { DOOR_UNLOCKED_MSG };
            messageBoxScript.ShowDialogs(dialogs,true);
        }
        // If the collider that entered the trigger has the "Player" tag, animate the door and load the new scene
        if (col.CompareTag(MySceneManager.k_PLAYER_TAG))
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            AnimateDoor(openDoorSprite);
            StartCoroutine(LoadNextScene());
        }
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
        MySceneManager.Instance.LoadScene(entranceNumber,nextScene);
    }
}