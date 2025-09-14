using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    
    private bool _isGrounded;
    private bool _isFacingRight = true;
    private float horizontalMovement;
    
    public static event Action<ulong> OnPlayerDied;
    
    [Header("Player Identification")]
    public Material[] playerMaterials;
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        
        // Rigidbody2D 제약 설정 확인
        if (_rb != null)
        {
            // Z축 회전만 고정, 이동은 자유롭게
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log($"Player {OwnerClientId} Rigidbody2D constraints: {_rb.constraints}");
        }
    }
    
    public override void OnNetworkSpawn()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (IsOwner)
        {
            // 소유자인 경우에만 입력 처리와 카메라 설정
            SetPlayerAppearance();
            
            var cameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(transform);
            }
            
            Debug.Log($"Player {OwnerClientId} is owner - enabling controls");
        }
        else
        {
            // 다른 플레이어의 경우 입력 비활성화
            var playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
            }
            Debug.Log($"Player {OwnerClientId} is not owner - disabling input");
        }
    }
    
    private void SetPlayerAppearance()
    {
        if (spriteRenderer != null && playerMaterials.Length > 0)
        {
            int materialIndex = (int)OwnerClientId % playerMaterials.Length;
            spriteRenderer.material = playerMaterials[materialIndex];
        }
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        horizontalMovement = context.ReadValue<Vector2>().x;
        Debug.Log($"Player {OwnerClientId} Move input: {horizontalMovement}");
        
        UpdateAnimationServerRpc(Mathf.Abs(horizontalMovement));
        
        if (horizontalMovement > 0 && !_isFacingRight)
        {
            _isFacingRight = true;
            FlipPlayerServerRpc(1);
        }
        else if (horizontalMovement < 0 && _isFacingRight)
        {
            _isFacingRight = false;
            FlipPlayerServerRpc(-1);
        }
    }
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        if (context.performed && _isGrounded)
        {
            JumpServerRpc();
        }
    }
    
    [ServerRpc]
    private void JumpServerRpc()
    {
        _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
        SetGroundedClientRpc(false);
        _isGrounded = false;
    }
    
    [ServerRpc]
    private void UpdateAnimationServerRpc(float velocityX)
    {
        UpdateAnimationClientRpc(velocityX);
    }
    
    [ClientRpc]
    private void UpdateAnimationClientRpc(float velocityX)
    {
        _animator.SetFloat("velocityX", velocityX);
    }
    
    [ServerRpc]
    private void FlipPlayerServerRpc(float scaleX)
    {
        FlipPlayerClientRpc(scaleX);
    }
    
    [ClientRpc]
    private void FlipPlayerClientRpc(float scaleX)
    {
        transform.localScale = new Vector3(scaleX, 1, 1);
    }
    
    [ClientRpc]
    private void SetGroundedClientRpc(bool grounded)
    {
        _animator.SetBool("grounded", grounded);
    }
    
    private void FixedUpdate()
    {
        if (!IsOwner) return;
        
        if (_rb != null)
        {
            Vector2 newVelocity = new Vector2(horizontalMovement * _speed, _rb.linearVelocity.y);
            _rb.linearVelocity = newVelocity;
            
            // 디버그 로그 (움직임 확인용)
            if (Mathf.Abs(horizontalMovement) > 0.1f)
            {
                Debug.Log($"Player {OwnerClientId} FixedUpdate - input: {horizontalMovement}, velocity: {newVelocity}, position: {transform.position}");
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsOwner) return;
        
        if (other.collider.CompareTag("FallingObject"))
        {
            PlayerDeathServerRpc();
        }
        
        if (other.collider.CompareTag("Ground"))
        {
            _isGrounded = true;
            SetGroundedClientRpc(true);
        }
    }
    
    [ServerRpc]
    private void PlayerDeathServerRpc()
    {
        OnPlayerDied?.Invoke(OwnerClientId);
        PlayerDeathClientRpc();
    }
    
    [ClientRpc]
    private void PlayerDeathClientRpc()
    {
        gameObject.SetActive(false);
    }
}
