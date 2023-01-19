using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }
    [SerializeField] private float speed = 5;
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _movement;

    #region Animator Tags

    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Speed = Animator.StringToHash("Speed");
    #endregion
    
    private void Start()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.X) && !InventoryManager.Instance.disableX)
        // {
        //     if (InventoryManager.Instance.IsOpen)
        //     {
        //         InventoryManager.Instance.CloseInventory();
        //         return;
        //     }
        //     InventoryManager.Instance.OpenInventory();
        // }

        if (!InventoryManager.Instance.IsOpen)
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
