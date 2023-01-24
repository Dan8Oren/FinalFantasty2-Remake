using System.Collections;
using UnityEngine;

/**
 * On active,Sets an "Active" Animator bool parameter to true and then to false.
 * waits each time named animationTime,and deactivates the game object at the end.
 */
[RequireComponent(typeof(Animator))]
public class ActiveScript : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private float animationTime = 0.2f;

    private void OnEnable()
    {
        StartCoroutine(Activate());
    }

    private IEnumerator Activate()
    {
        animator.SetBool("Active", true);
        yield return new WaitForSeconds(animationTime);
        animator.SetBool("Active", false);
        yield return new WaitForSeconds(animationTime);
        gameObject.SetActive(false);
    }
}