using System.Collections;
using Cinemachine;
using UnityEngine;

public class RedQueenRoom : MonoBehaviour
{
    public GameObject[] activateOnTrigger;

    [SerializeField] private Vector3 posToLook;
    [SerializeField] private float objectsDelayTime;
    [SerializeField] private float cameraSpeedModifier;
    private Vector3 _cameraPos;

    private CinemachineVirtualCamera _cmCamera;
    private bool _isTransition;

    private void Start()
    {
        foreach (var obj in activateOnTrigger) obj.SetActive(false);
    }

    private void Update()
    {
        if (_isTransition)
        {
            if (_cameraPos.y >= posToLook.y)
            {
                _cmCamera.transform.position = _cameraPos;
                return;
            }

            _cameraPos.y = Mathf.MoveTowards(_cameraPos.y, posToLook.y, Time.deltaTime * cameraSpeedModifier);
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
            _cameraPos = _cmCamera.transform.position;
            _cmCamera.Follow = null;
            _isTransition = true;
            StartCoroutine(ActivateObjectsWithDelay(objectsDelayTime));
        }
    }

    /**
     * activates the scene event objects at a given time delay.
     */
    private IEnumerator ActivateObjectsWithDelay(float time)
    {
        yield return new WaitForSeconds(time);
        foreach (var obj in activateOnTrigger) obj.SetActive(true);
    }
}