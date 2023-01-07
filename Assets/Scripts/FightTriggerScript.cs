using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightTriggerScript : MonoBehaviour
{
    public Animator fightTransition;
    [SerializeField] private int roomDoorNum;
    [SerializeField] private float transitionWaitTime;
    [SerializeField] private float waitTimeBeforeTransition;
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            StartCoroutine(LoadFight());
        }
    }
    
    private IEnumerator LoadFight()
    {
        print("Triggered!!!");
        Rigidbody2D rigidbody2D = MySceneManager.Instance.hero.GetComponent<Rigidbody2D>();
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
        yield return new WaitForSeconds(waitTimeBeforeTransition);
        fightTransition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionWaitTime);
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        MySceneManager.Instance.LoadScene(roomDoorNum+1,MySceneManager.k_FIGHT);
    }

}
