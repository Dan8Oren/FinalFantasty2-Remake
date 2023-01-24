using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class PadScript : MonoBehaviour
{
    private void Start()
    {
        _lastTap = null;
        _spawned = 0;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _defaultColor = _spriteRenderer.color;
        disabledText.gameObject.SetActive(false);
        _isFirst = true;
    }

    // Update is called once per frame
    private void Update()
    {
        FinishInitialization();
        _curSeconds += Time.deltaTime;
        _disableTime += Time.deltaTime;
        if (_disableTime > timeForDisabled)
        {
            disabledText.gameObject.SetActive(false);
            _spriteRenderer.color = _defaultColor;
            _disableTime = 0;
            _disable = false;
        }

        if (_timeToSpawn < _curSeconds && _spawned < maxSpawn && enabled) SpawnNewTap();

        if (Input.GetKeyDown(_keyCode) && !_disable) HandleKeyClicked();
    }


    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(k_PAD_TAG))
        {
            _activeKeyTap = col.gameObject;
            _onTarget = true;
        }
    }


    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag(k_PAD_TAG)) StartCoroutine(DelayAction(col));
    }

    //Horizontal layout changes the position after start was called..
    private void FinishInitialization()
    {
        if (_isFirst)
        {
            _curSeconds -= _firstSpawnTime; //to let countdown finish
            var pos = transform.position;
            pos.y = yPosToSpawn;
            _spawnLocation = pos;
            _isFirst = false;
        }
    }

    private void SpawnNewTap()
    {
        var obj = Instantiate(keyTapPrefab, _spawnLocation, Quaternion.identity);
        obj.GetComponent<SpriteRenderer>().sprite = _sprite;
        obj.GetComponent<Rigidbody2D>().gravityScale *= _speed;
        _curSeconds = 0;
        _spawned++;
        if (_spawned >= maxSpawn) _lastTap = obj;
    }

    /**
     * Handles the changes when the pad's key is clicked.
     */
    private void HandleKeyClicked()
    {
        if (_onTarget &&
            _activeKeyTap != null && _activeKeyTap.gameObject.activeSelf)
        {
            SoundManager.Instance.PlayGoodSequenceClick(FightManager.Instance.fightAudio);
            maxSpawn--;
            _spawned--;
            goodActive.SetActive(true);
            _activeKeyTap.gameObject.SetActive(false);
            SequenceManager.Instance.GoodTapIncrease();
            if ((maxSpawn == _spawned && maxSpawn == 0) ||
                (_lastTap != null && !_lastTap.gameObject.activeSelf))
            {
                _spriteRenderer.color = new Color(0, 0.7f, 0, 0.8f);
                enabled = false;
            }
        }
        else
        {
            badActive.SetActive(true);
            _spriteRenderer.color = new Color(0.7f, 0, 0, 0.8f);
            disabledText.gameObject.SetActive(true);
            _disable = true;
            _disableTime = 0;
        }
    }

    /**
     * gives the player a little more time to react.
     */
    private IEnumerator DelayAction(Collider2D col)
    {
        yield return new WaitForSeconds(bonusTimeToAct);
        if (col.gameObject.activeSelf)
        {
            _onTarget = false;
            if (_lastTap == col.gameObject)
            {
                _spriteRenderer.color = new Color(0.7f, 0, 0, 0.8f);
                enabled = false;
            }
        }

        Destroy(col.gameObject);
        _activeKeyTap = null;
    }

    /**
     * sets the pad behavior.
     * <param name="key"> KeyCode to be activated by</param>
     * <param name="sprite"> the sprite the pad should display</param>
     * <param name="speed"> the gravity modifier for the taps to fall at.</param>
     * <param name="timeToSpawn"> each tap spawn time</param>
     * <param name="amountToSpawn"> number of taps to spawn</param>
     * <param name="firstSpawnTime"> the countdown event time at total.</param>
     */
    public void SetPad(KeyCode key, Sprite sprite,
        float speed, float timeToSpawn, int amountToSpawn, float firstSpawnTime)
    {
        if (_spriteRenderer == null) Start();
        maxSpawn = amountToSpawn;
        _firstSpawnTime = firstSpawnTime;
        _sprite = sprite;
        _spriteRenderer.sprite = sprite;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        _keyCode = key;
        _speed = speed;
        _timeToSpawn = timeToSpawn;
    }

    #region Inspector

    public const string k_PAD_TAG = "Pad";
    public GameObject keyTapPrefab;
    public GameObject goodActive;
    public GameObject badActive;
    public TextMeshPro disabledText;
    [SerializeField] private float yPosToSpawn;
    [SerializeField] private float timeForDisabled;
    [SerializeField] private int maxSpawn = 3;
    [SerializeField] private float bonusTimeToAct = 0.01f;

    #endregion


    #region Private Fields

    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private Sprite _sprite;
    private KeyCode _keyCode;
    private Vector3 _spawnLocation;
    private GameObject _activeKeyTap;
    private GameObject _lastTap;
    private bool _onTarget;
    private bool _isFirst;
    private bool _disable;
    private float _curSeconds;
    private float _speed;
    private float _timeToSpawn;
    private float _disableTime;
    private float _firstSpawnTime;
    private int _spawned;
    private Color _defaultColor;

    #endregion
}