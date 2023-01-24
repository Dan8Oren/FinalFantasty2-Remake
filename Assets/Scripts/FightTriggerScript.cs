using System.Collections;
using UnityEngine;

public class FightTriggerScript : MonoBehaviour
{
    public Animator fightTransition;
    [SerializeField] private int roomDoorNum;
    [SerializeField] private float transitionWaitTime;
    [SerializeField] private float waitTimeBeforeTransition;

    /**
     * Loads a fight scene by the roomDoorNum field.
     */
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (GameManager.Instance.CompletedFightLevels.Contains(roomDoorNum)) return;
        if (col.CompareTag("Player")) StartCoroutine(LoadFight());
    }

    /**
     * Animates the transition from the world to the fight scene.
     */
    private IEnumerator LoadFight()
    {
        var rigidbody2D = MySceneManager.Instance.hero.GetComponent<Rigidbody2D>();
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(waitTimeBeforeTransition);
        fightTransition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionWaitTime);
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        MySceneManager.Instance.LoadFightScene(roomDoorNum);
    }
}