using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkPlayerController : NetworkBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private Animator _animator;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    
    // 네트워크 변수들
    private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>();
    private NetworkVariable<bool> networkFacingRight = new NetworkVariable<bool>(true);
    private NetworkVariable<float> networkVelocityX = new NetworkVariable<float>();
    private NetworkVariable<bool> networkGrounded = new NetworkVariable<bool>(true);

    private bool _isGrounded = true;
    private bool _isFacingRight = true;
    private float horizontalMovement;

    // 입력 액션들
    private InputAction moveAction;
    private InputAction jumpAction;
    
    void Awake()
    {
        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();
        if (_animator == null)
            _animator = GetComponent<Animator>();
            
        // Input Actions 설정
        SetupInputActions();
    }
    
    void SetupInputActions()
    {
        // 이동 액션 설정
        moveAction = new InputAction("Move", InputActionType.Value, "<Keyboard>/a,<Keyboard>/d,<Keyboard>/leftArrow,<Keyboard>/rightArrow");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/d")
            .With("Positive", "<Keyboard>/rightArrow");
            
        // 점프 액션 설정
        jumpAction = new InputAction("Jump", InputActionType.Button, "<Keyboard>/space");
        
        // 이벤트 연결
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
        jumpAction.performed += OnJumpPerformed;
    }

    public override void OnNetworkSpawn()
    {
        // 플레이어가 스폰될 때 초기 위치 설정
        if (IsOwner)
        {
            // 소유자인 경우 PlayerInput 활성화 (커스텀 InputAction 대신)
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = true;
                Debug.Log("PlayerInput enabled for owner");
            }
            
            // 커스텀 InputAction 비활성화
            moveAction.Disable();
            jumpAction.Disable();
        }
        else
        {
            // 다른 플레이어인 경우 PlayerInput 비활성화
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.enabled = false;
                Debug.Log("PlayerInput disabled for non-owner");
            }
            
            // 커스텀 InputAction도 비활성화
            moveAction.Disable();
            jumpAction.Disable();
        }

        // 네트워크 변수 변화 감지
        networkPosition.OnValueChanged += OnPositionChanged;
        networkFacingRight.OnValueChanged += OnFacingChanged;
        networkVelocityX.OnValueChanged += OnVelocityXChanged;
        networkGrounded.OnValueChanged += OnGroundedChanged;

        // 플레이어 색상을 다르게 설정 (구분용)
        SetPlayerColor();
    }

    void OnDestroy()
    {
        // Input Actions 정리
        if (moveAction != null)
        {
            moveAction.performed -= OnMovePerformed;
            moveAction.canceled -= OnMoveCanceled;
            moveAction.Dispose();
        }
        
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
            jumpAction.Dispose();
        }
    }

    void SetPlayerColor()
    {
        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (IsOwner)
            {
                spriteRenderer.color = Color.white; // 자신은 흰색
            }
            else
            {
                // 다른 플레이어는 다른 색상
                spriteRenderer.color = new Color(
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    Random.Range(0.5f, 1f),
                    1f
                );
            }
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            // 위치 동기화
            UpdateNetworkPositionServerRpc(transform.position);
        }
    }
    
    // Input System 이벤트 핸들러들
    void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        horizontalMovement = context.ReadValue<float>();
        Debug.Log($"Move input: {horizontalMovement}, IsOwner: {IsOwner}");
        
        // 애니메이션 동기화
        UpdateAnimationServerRpc(Mathf.Abs(horizontalMovement));
        
        // 방향 동기화
        if (horizontalMovement > 0 && !_isFacingRight)
        {
            _isFacingRight = true;
            UpdateFacingDirectionServerRpc(true);
        }
        else if (horizontalMovement < 0 && _isFacingRight)
        {
            _isFacingRight = false;
            UpdateFacingDirectionServerRpc(false);
        }
    }
    
    void OnMoveCanceled(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        horizontalMovement = 0;
        Debug.Log("Move input canceled");
        
        // 애니메이션 동기화
        UpdateAnimationServerRpc(0);
    }
    
    void OnJumpPerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        if (_isGrounded)
        {
            Debug.Log($"Jump input, IsGrounded: {_isGrounded}, IsOwner: {IsOwner}");
            _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
            _isGrounded = false;
            UpdateGroundedStateServerRpc(false);
        }
    }

    void FixedUpdate()
    {
        if (IsOwner)
        {
            // 소유자만 물리 움직임 처리
            _rb.linearVelocity = new Vector2(horizontalMovement * _speed, _rb.linearVelocity.y);
        }
    }

    // 기존 PlayerInput 메서드들은 더 이상 사용하지 않음 (Input System Actions로 대체)
    
    // PlayerInput 이벤트와 연결될 메서드들
    public void Move(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        horizontalMovement = context.ReadValue<Vector2>().x;
        Debug.Log($"PlayerInput Move: {horizontalMovement}, IsOwner: {IsOwner}");
        
        // 로컬 애니메이터 업데이트
        if (_animator != null)
        {
            _animator.SetFloat("velocityX", Mathf.Abs(horizontalMovement));
        }
        
        // 네트워크로 애니메이션 동기화
        UpdateAnimationServerRpc(Mathf.Abs(horizontalMovement));
        
        // 방향 전환 처리 (기존 PlayerController와 동일)
        if (horizontalMovement > 0 && !_isFacingRight)
        {
            _isFacingRight = true;
            transform.localScale = new Vector3(1, 1, 1);
            UpdateFacingDirectionServerRpc(true);
        }
        else if (horizontalMovement < 0 && _isFacingRight)
        {
            _isFacingRight = false;
            transform.localScale = new Vector3(-1, 1, 1);
            UpdateFacingDirectionServerRpc(false);
        }
    }
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        
        if (context.performed && _isGrounded)
        {
            Debug.Log($"PlayerInput Jump, IsGrounded: {_isGrounded}, IsOwner: {IsOwner}");
            
            // 로컬 물리 및 애니메이터 (기존 PlayerController와 동일)
            _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
            _isGrounded = false;
            if (_animator != null)
            {
                _animator.SetBool("grounded", false);
            }
            
            // 네트워크 동기화
            UpdateGroundedStateServerRpc(false);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (!IsOwner) return;

        if (other.collider.CompareTag("Ground"))
        {
            _isGrounded = true;
            if (_animator != null)
            {
                _animator.SetBool("grounded", true);
            }
            UpdateGroundedStateServerRpc(true);
            Debug.Log("Player landed on ground");
        }

        // 다른 플레이어와 충돌 처리
        if (other.collider.CompareTag("Player"))
        {
            Debug.Log("Player collision detected!");
            OnPlayerCollisionServerRpc();
        }
    }

    // 서버 RPC들 (클라이언트에서 서버로 데이터 전송)
    [ServerRpc]
    void UpdateNetworkPositionServerRpc(Vector2 position)
    {
        networkPosition.Value = position;
    }

    [ServerRpc]
    void UpdateFacingDirectionServerRpc(bool facingRight)
    {
        networkFacingRight.Value = facingRight;
    }

    [ServerRpc]
    void UpdateAnimationServerRpc(float velocityX)
    {
        networkVelocityX.Value = velocityX;
    }

    [ServerRpc]
    void UpdateGroundedStateServerRpc(bool grounded)
    {
        networkGrounded.Value = grounded;
    }

    [ServerRpc]
    void OnPlayerCollisionServerRpc()
    {
        // 모든 클라이언트에게 충돌 이벤트 전달
        OnPlayerCollisionClientRpc();
    }

    // 클라이언트 RPC (서버에서 모든 클라이언트로 데이터 전송)
    [ClientRpc]
    void OnPlayerCollisionClientRpc()
    {
        // 충돌 효과 (예: 색상 변화, 이펙트 등)
        StartCoroutine(FlashColor());
    }

    System.Collections.IEnumerator FlashColor()
    {
        var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new UnityEngine.WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }

    // 네트워크 변수 변화 이벤트 핸들러들
    void OnPositionChanged(Vector2 oldValue, Vector2 newValue)
    {
        if (!IsOwner)
        {
            // 다른 플레이어의 위치 동기화
            transform.position = newValue;
        }
    }

    void OnFacingChanged(bool oldValue, bool newValue)
    {
        if (!IsOwner)
        {
            transform.localScale = new Vector3(newValue ? 1 : -1, 1, 1);
        }
    }

    void OnVelocityXChanged(float oldValue, float newValue)
    {
        if (!IsOwner && _animator != null)
        {
            _animator.SetFloat("velocityX", newValue);
        }
    }

    void OnGroundedChanged(bool oldValue, bool newValue)
    {
        if (!IsOwner && _animator != null)
        {
            _animator.SetBool("grounded", newValue);
        }
    }
}