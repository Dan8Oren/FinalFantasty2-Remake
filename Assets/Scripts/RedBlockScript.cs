using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBlockScript : MonoBehaviour
{
    [SerializeField] private int levelToUnlock;
    private void Start()
    {
        if (GameManager.Instance.FightLevel >= levelToUnlock)
        {
            Destroy(gameObject);
        }
    }
}
