using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5;
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _movement;

    #region Animator Tags

    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Speed = Animator.StringToHash("Speed");
    #endregion
    
    private static bool _isInventoryOpen;
    private void Start()
    {
        _isInventoryOpen = false;
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (_isInventoryOpen)
            {
                InventoryManager.Instance.CloseInventory();
                _isInventoryOpen = false;
                return;
            }
            _isInventoryOpen = true;
            InventoryManager.Instance.OpenInventory();
        }

        if (!_isInventoryOpen)
        {
            _movement.x = Input.GetAxisRaw("Horizontal");
            _movement.y = Input.GetAxisRaw("Vertical");
        
            _animator.SetFloat(Horizontal, _movement.x);
            _animator.SetFloat(Vertical, _movement.y);
            _animator.SetFloat(Speed, _movement.sqrMagnitude);
        }
        
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(_rb.position + _movement * speed * Time.fixedDeltaTime);
    }
}
