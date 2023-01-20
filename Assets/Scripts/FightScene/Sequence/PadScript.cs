using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PadScript : MonoBehaviour
{
    public const String k_PAD_TAG = "Pad";
    public GameObject keyTapPrefab;
    public GameObject goodActive;
    public GameObject badActive;
    public TextMeshPro disabledText;
    [SerializeField] private float yPosToSpawn;
    [SerializeField] private float timeForDisabled;
    [SerializeField] private int maxSpawn = 3;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rigidbody;
    private float _speed;
    private KeyCode _keyCode;
    private Sprite _sprite;
    private float _curSeconds;
    private bool _onTarget;
    private GameObject _activeKeyTap;
    private float _timeToSpawn;
    private Vector3 _spawnLocation;
    private bool _isFirst;
    private bool _disable;
    private float _disableTime;
    private int _spawned;
    private Color _defaultColor;
    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _defaultColor = _spriteRenderer.color;
        disabledText.gameObject.SetActive(false);
        _isFirst = true;
    }

    // Update is called once per frame
    private void Update()
    {
        _curSeconds += Time.deltaTime;
        _disableTime += Time.deltaTime;
        if (_disableTime > timeForDisabled)
        {
            disabledText.gameObject.SetActive(false);
            _spriteRenderer.color = _defaultColor;
            _disableTime = 0;
            _disable = false;
        }
        //Horizontal layout changes the position after start was called..
        if (_curSeconds>_timeToSpawn && _isFirst) 
        {
            Vector3 pos = transform.position;
            pos.y = yPosToSpawn;
            _spawnLocation = pos;
            _isFirst = false;
        }
        if ((_timeToSpawn <= _curSeconds) && _spawned != maxSpawn)
        {
            GameObject obj = Instantiate(keyTapPrefab, _spawnLocation, Quaternion.identity);
            obj.GetComponent<SpriteRenderer>().sprite = _sprite;
            obj.GetComponent<Rigidbody2D>().gravityScale*=_speed;
            _curSeconds = 0;
            _spawned++;
        }
        
        if (Input.GetKeyDown(_keyCode) && !_disable)
        {
            if (_onTarget)
            {
                maxSpawn--;
                _spawned--;
                goodActive.SetActive(true);
                Destroy(_activeKeyTap);
                if (maxSpawn == _spawned && maxSpawn == 0)
                {
                    _spriteRenderer.color = new Color(0, 0.7f, 0, 0.8f);
                    SequenceManager.Instance.GoodTapIncrease();
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
        if (col.CompareTag(k_PAD_TAG))
        {
            _onTarget = false;
            Destroy(col);
            _spriteRenderer.color = new Color(0.7f, 0, 0, 0.8f);
            enabled = false;
        }
    }

    public void SetPad(KeyCode key,Sprite sprite,float speed,float timeToSpawn,int amountToSpawn)
    {
        if (_spriteRenderer == null)
        {
            Start();
        }

        maxSpawn = amountToSpawn;
        _sprite = sprite;
        _spriteRenderer.sprite = sprite;
        _rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
        _keyCode = key;
        _speed = speed;
        _timeToSpawn = timeToSpawn;
    }
}
