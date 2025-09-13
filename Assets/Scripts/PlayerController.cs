using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 10f;


    private bool _isGrounded;
    private bool _isFacingRight = true;
    private float horizontalMovement;
    public event Action OnPlayerDied;

    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
        _animator.SetFloat("velocityX", Mathf.Abs(horizontalMovement));
        if (horizontalMovement > 0 && !_isFacingRight)
        {
            _isFacingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (horizontalMovement < 0 && _isFacingRight)
        {
            _isFacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && _isGrounded)
        {
            _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
            _animator.SetBool("grounded", false);
            _isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = new Vector2(horizontalMovement * _speed, _rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {

        if (other.collider.CompareTag("FallingObject"))
        {
            OnPlayerDied?.Invoke();
            Destroy(gameObject);
        }

        if (other.collider.CompareTag("Ground"))
        {
            _isGrounded = true;
            _animator.SetBool("grounded", true);
        }
    }

    // private void OnColliderExit2D(Collision2D other)
    // {
    //     if (other.collider.CompareTag("Ground"))
    //     {
    //         _isGrounded = true;
    //         _animator.SetBool("grounded", true);
    //
    //     }
    // }

}
