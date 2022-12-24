using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorScript : MonoBehaviour
{
    public int entranceNumber;
    public Sprite openDoorSprite;
    public Animator transition;
    [SerializeField] private string nextScene; 
    [SerializeField] private float transitionWaitTime;
    [SerializeField] private float waitTimeBeforeTransition;
    private SpriteRenderer _spriteRenderer;
    private void OnTriggerEnter2D(Collider2D col)
    {
        // If the collider that entered the trigger has the "Player" tag, animate the door and load the new scene
        if (col.CompareTag("Player"))
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
        yield return new WaitForSeconds(waitTimeBeforeTransition);
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionWaitTime);
        MySceneManager.Instance.LoadScene(entranceNumber,nextScene);
    }
}