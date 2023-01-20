using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
    [SerializeField] private float posYToDestroy;

    private void FixedUpdate()
    {
        if (transform.position.y < posYToDestroy)
        {
            Destroy(gameObject);
        }
    }
}
