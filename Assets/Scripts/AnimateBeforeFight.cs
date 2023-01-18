using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Cinemachine.Editor;
using UnityEngine;

public class AnimateBeforeFight : MonoBehaviour
{
    [SerializeField] private GameObject brainWashed;
    [SerializeField] private Animator brainWashedAnimator;
    [SerializeField] private float cameraTransitionTime;
    private CinemachineVirtualCamera _cmCamera;
    private GameObject _camera;
    private Vector3 _brainWashedPos;
    private Vector3 _cameraPos;
    private bool _isTransition = false;
    private void Update()
    {
        if (_isTransition)
        {
            if (_cameraPos.y >=_brainWashedPos.y)
            {
                _cmCamera.transform.position = _cameraPos;
                return;
            }
            _cameraPos.y = Mathf.MoveTowards(_cameraPos.y, _brainWashedPos.y,Time.deltaTime);
            _cmCamera.transform.position = _cameraPos;
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            _camera = MySceneManager.Instance.mainCamera;
            _cmCamera = MySceneManager.Instance.cmCamera.GetComponent<CinemachineVirtualCamera>();
            _brainWashedPos = brainWashed.transform.position;
            _cameraPos = _cmCamera.transform.position;
            _cmCamera.Follow = null;
            _isTransition = true;
            StartCoroutine(Animate());
        }
    }

    private IEnumerator Animate()
    {
        yield return new WaitForSeconds(cameraTransitionTime);
        brainWashedAnimator.SetTrigger("BrainWashed");
    }
}
