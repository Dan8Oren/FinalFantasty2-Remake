using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class RedBedRoomAnimation : MonoBehaviour
{
    [SerializeField] private GameObject magician;
    private CinemachineVirtualCamera _cmCamera;
    private Vector3 _magicianPos;
    private Vector3 _cameraPos;
    private bool _isTransition = false;
    [SerializeField ]private float cameraSpeedModifier;

    private void Update()
    {
        if (_isTransition)
        {
            if (_cameraPos.y >=_magicianPos.y)
            {
                _cmCamera.transform.position = _cameraPos;
                return;
            }
            _cameraPos.y = Mathf.MoveTowards(_cameraPos.y, _magicianPos.y,Time.deltaTime*cameraSpeedModifier);
            _cmCamera.transform.position = _cameraPos;
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            SoundManager.Instance.StopPlaying();
            SoundManager.Instance.PlayBeforeFight();
            _cmCamera = MySceneManager.Instance.cmCamera;
            _magicianPos = magician.transform.position;
            _cameraPos = _cmCamera.transform.position;
            _cmCamera.Follow = null;
            _isTransition = true;
        }
    }
}
