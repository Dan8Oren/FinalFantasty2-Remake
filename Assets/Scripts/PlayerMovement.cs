using UnityEngine;

[RequireComponent(typeof(Animator), typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
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
        if (!MySceneManager.Instance.IsInFight &&
            Input.GetKeyDown(KeyCode.X) && !InventoryManager.Instance.disableX)
        {
            if (InventoryManager.Instance.IsOpen)
            {
                InventoryManager.Instance.CloseInventory();
                return;
            }

            InventoryManager.Instance.OpenInventory();
        }

        if (!InventoryManager.Instance.IsOpen)
        {
            _movement.x = Input.GetAxisRaw("Horizontal");
            _movement.y = Input.GetAxisRaw("Vertical");
            CollisionFix();
            _animator.SetFloat(Horizontal, _movement.x);
            _animator.SetFloat(Vertical, _movement.y);
            _animator.SetFloat(Speed, _movement.sqrMagnitude);
        }
    }

    /**
     * Updates the hero position on the screen by the player's input.
     */
    private void FixedUpdate()
    {
        if (_wallColide &&
            (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)))
            if (Mathf.Abs(_colNormal.x) > 0.5f)
                _movement += _collisionFix;

        if (_wallColide &&
            (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow)))
            if (Mathf.Abs(_colNormal.y) > 0.5f)
                _movement += _collisionFix;
        _rb.MovePosition(_rb.position + _movement * speed * Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Wall"))
        {
            var p = col.contacts[0];
            _colNormal = p.normal;
            _wallColide = true;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Wall"))
        {
            _wallColide = false;
            _collisionFix = Vector2.zero;
        }
    }

    /**
     * Used to add additional help when colliding walls, to help the player slide to the desired location.
     */
    private void CollisionFix()
    {
        if (_movement.x != 0)
        {
            if (_movement.x < 0)
                _collisionFix.x = -fixAmount;
            else
                _collisionFix.x = fixAmount;
        }

        if (_movement.y != 0)
        {
            if (_movement.y < 0)
                _collisionFix.y = -fixAmount;
            else
                _collisionFix.y = fixAmount;
        }
    }

    #region Fields

    public static PlayerMovement Instance { get; private set; }
    [SerializeField] private float speed = 5;
    [SerializeField] private float fixAmount = 1f;
    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _movement;
    private Vector2 _collisionFix;
    private bool _wallColide;
    private Vector2 _colNormal;

    #endregion

    #region Animator Tags

    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int Speed = Animator.StringToHash("Speed");

    #endregion
}