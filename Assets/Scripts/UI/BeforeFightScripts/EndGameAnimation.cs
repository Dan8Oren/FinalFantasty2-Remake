using System.Collections;
using Cinemachine;
using UnityEngine;

public class EndGameAnimation : MonoBehaviour
{
    public SpecialMessageBox specialMessageBox;
    public GameObject endGame;

    [SerializeField] private GameObject cameraPos;
    [SerializeField] private float cameraSpeedModifier;
    [SerializeField] private float cakesTime;
    [SerializeField] private float beforeCakesTime;
    [SerializeField] private string[] colors;
    private Vector3 _cameraPos;

    private CinemachineVirtualCamera _cmCamera;
    private bool _isTransition;
    private bool _isTransitionEnd;
    private Vector3 _Pos;

    private void Update()
    {
        if (_isTransition)
        {
            if (_cameraPos.y <= _Pos.y)
            {
                if (!_isTransitionEnd) StartCoroutine(EndMessages());
                _cmCamera.transform.position = _cameraPos;
                _isTransitionEnd = true;
                return;
            }

            _cameraPos.y = Mathf.MoveTowards(_cameraPos.y, _Pos.y, Time.deltaTime * cameraSpeedModifier);
            _cmCamera.transform.position = _cameraPos;
        }
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            SoundManager.Instance.StopPlaying();
            SoundManager.Instance.PlayEndGame();
            endGame.SetActive(true);
            _cmCamera = MySceneManager.Instance.cmCamera;
            _Pos = cameraPos.transform.position;
            _cameraPos = _cmCamera.transform.position;
            _cmCamera.Follow = null;
            _isTransition = true;
            MySceneManager.Instance.HeroRigid.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private IEnumerator EndMessages()
    {
        specialMessageBox.gameObject.SetActive(true);
        specialMessageBox.ShowDialog("Congratulation you won the Game!!\nHere is a Cake!.", false);
        yield return new WaitForSeconds(beforeCakesTime);
        foreach (var color in colors) specialMessageBox.ShowDialog($"<color={color}>Cake</color>", true);

        StartCoroutine(specialMessageBox.StartLoop(cakesTime));
    }
}