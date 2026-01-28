//using UnityEngine;
//using UnityEngine.InputSystem;

//[RequireComponent(typeof(Rigidbody2D))]
//[RequireComponent(typeof(Animator))]
//public class PlayerMovement : MonoBehaviour
//{
//    [Header("Movement Settings")]
//    [SerializeField] private float moveSpeed = 4f;

//    private Rigidbody2D _rb;
//    private Animator _animator;
//    private Vector2 _movementInput;

//    private void Awake()
//    {
//        _rb = GetComponent<Rigidbody2D>();
//        _animator = GetComponent<Animator>();
//    }

//    void Update()
//    {
//        if (PauseController.IsGamePaused)
//        {
//            _rb.linearVelocity = Vector2.zero;
//            _animator.SetBool("IsMoving", false);
//            return;
//        }
//    }

//    private void FixedUpdate()
//    {
//        Move();
//    }

//    // Gọi từ PlayerInput (Invoke Unity Events)
//    public void OnMove(InputAction.CallbackContext context)
//    {
//        _movementInput = context.ReadValue<Vector2>();
//        UpdateAnimation();
//    }

//    private void Move()
//    {
//           if (_movementInput == Vector2.zero)
//    {
//        _rb.linearVelocity = Vector2.zero;
//        return;
//    }
//        _rb.linearVelocity = _movementInput * moveSpeed;
//    }

//    private void UpdateAnimation()
//    {
//        bool isMoving = _movementInput != Vector2.zero;

//        _animator.SetBool("IsMoving", isMoving);

//        // Chỉ cập nhật hướng khi đang di chuyển
//        if (isMoving)
//        {
//            _animator.SetFloat("MoveX", _movementInput.x);
//            _animator.SetFloat("MoveY", _movementInput.y);
//        }
//    }
//}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float moveSpeed = 4f;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }
        rb.linearVelocity = moveInput * moveSpeed;
    }



    public void Move(InputAction.CallbackContext context)
    {
        animator.SetBool("isWalking", true);
        if (context.canceled)
        {
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);
        }
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
    }
}
    
