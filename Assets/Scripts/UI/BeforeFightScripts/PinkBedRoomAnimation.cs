using System.Collections;
using Cinemachine;
using UnityEngine;

public class PinkBedRoomAnimation : MonoBehaviour
{
    [SerializeField] private GameObject brainWashed;
    [SerializeField] private Animator brainWashedAnimator;
    [SerializeField] private float cameraTransitionTime;
    [SerializeField] private float cameraSpeedModifier;
    private Vector3 _brainWashedPos;
    private Vector3 _cameraPos;

    private CinemachineVirtualCamera _cmCamera;
    private bool _isTransition;


    private void Update()
    {
        if (_isTransition)
        {
            if (_cameraPos.y >= _brainWashedPos.y)
            {
                _cmCamera.transform.position = _cameraPos;
                return;
            }

            _cameraPos.y = Mathf.MoveTowards(_cameraPos.y, _brainWashedPos.y, Time.deltaTime * cameraSpeedModifier);
            _cmCamera.transform.position = _cameraPos;
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            SoundManager.Instance.StopPlaying();
            SoundManager.Instance.PlayBeforeFight();
            _cmCamera = MySceneManager.Instance.cmCameraObject.GetComponent<CinemachineVirtualCamera>();
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