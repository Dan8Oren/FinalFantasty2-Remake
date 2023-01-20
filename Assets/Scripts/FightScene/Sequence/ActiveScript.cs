using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        animator.SetBool("Active",true);
        yield return new WaitForSeconds(animationTime);
        animator.SetBool("Active",false);
        yield return new WaitForSeconds(animationTime);
        gameObject.SetActive(false);
    }
    
    
}
