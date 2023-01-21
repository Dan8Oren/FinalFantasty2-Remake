using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class RedQueenRoom : MonoBehaviour
{
    public GameObject[] activateOnTrigger;
    [SerializeField] private Vector3 posToLook;
    [SerializeField] private float objectsDelayTime;
    private CinemachineVirtualCamera _cmCamera;
    private Vector3 _cameraPos;
    private bool _isTransition = false;
    [SerializeField ]private float cameraSpeedModifier;

    private void Start()
    {
        foreach (var obj in activateOnTrigger)
        {
            obj.SetActive(false);
        }
    }

    private void Update()
    {
        if (_isTransition)
        {
            if (_cameraPos.y >=posToLook.y)
            {
                _cmCamera.transform.position = _cameraPos;
                return;
            }
            _cameraPos.y = Mathf.MoveTowards(_cameraPos.y, posToLook.y,Time.deltaTime*cameraSpeedModifier);
            _cmCamera.transform.position = _cameraPos;
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            _cmCamera = MySceneManager.Instance.cmCamera;
            _cameraPos = _cmCamera.transform.position;
            _cmCamera.Follow = null;
            _isTransition = true;
            StartCoroutine(ActivateObjectsWithDelay(objectsDelayTime));
        }
    }

    private IEnumerator ActivateObjectsWithDelay(float time)
    {
        yield return new WaitForSeconds(time);
        foreach (var obj in activateOnTrigger)
        {
            obj.SetActive(true);
        }
    }
}
