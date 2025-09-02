using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;
    
    [Header("Ground Detection")]
    public LayerMask groundLayers = 1;
    public float groundCheckDistance = 0.1f;
    
    [Header("Attack Settings")]
    public GameObject whipPrefab;
    public Transform attackPoint;
    public float attackCooldown = 0.5f;
    
    [Header("Debug")]
    public bool enableDebugLogs = false;

    private Rigidbody rb;
    private CapsuleCollider col;
    private Animator anim;
    private Vector2 moveInput;
    private bool jumpPressed;
    private bool isGrounded;
    private float lastAttackTime;
    private bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        anim = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                        RigidbodyConstraints.FreezeRotationZ | 
                        RigidbodyConstraints.FreezePositionZ;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        col.height = 2f;
        col.radius = 0.5f;
        col.center = Vector3.up;
        
        // Set reasonable Rigidbody defaults
        rb.mass = 1f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;
    }

    // Called by the Player Input component
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        if (enableDebugLogs) Debug.Log($"Input received: {moveInput}");
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) jumpPressed = true;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    void Update()
    {
        CheckGrounded();

        if (jumpPressed && isGrounded)
        {
            Jump();
            jumpPressed = false;
        }

        HandleSpriteFlip();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        MovePlayer();
    }

    void CheckGrounded()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        float rayDistance = col.bounds.extents.y + groundCheckDistance;
        isGrounded = Physics.Raycast(rayStart, Vector3.down, rayDistance, groundLayers);
        Debug.DrawRay(rayStart, Vector3.down * rayDistance, isGrounded ? Color.green : Color.red);
    }

    void MovePlayer()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.x = moveInput.x * moveSpeed;
        velocity.z = 0f;
        rb.linearVelocity = velocity;
        
        // Debug log
        if (enableDebugLogs) Debug.Log($"MoveInput: {moveInput.x}, Velocity: {rb.linearVelocity}");
    }

    void Jump()
    {
        Vector3 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
        isGrounded = false;
    }

    void Attack()
    {
        if (anim != null && HasParameter("Attack"))
        {
            anim.SetTrigger("Attack");
        }
        
        if (whipPrefab != null)
        {
            Vector3 spawnPos = attackPoint != null ? attackPoint.position : transform.position;
            Quaternion spawnRot = attackPoint != null ? attackPoint.rotation : transform.rotation;
            Instantiate(whipPrefab, spawnPos, spawnRot);
        }
    }

    void HandleSpriteFlip()
    {
        if (moveInput.x > 0 && !facingRight) Flip();
        else if (moveInput.x < 0 && facingRight) Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void UpdateAnimator()
    {
        if (anim == null) return;
        
        try
        {
            if (HasParameter("Speed"))
                anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
            if (HasParameter("IsGrounded"))
                anim.SetBool("IsGrounded", isGrounded);
            if (HasParameter("VelocityY"))
                anim.SetFloat("VelocityY", rb.linearVelocity.y);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Animator parameter error: {e.Message}");
        }
    }

    bool HasParameter(string paramName)
    {
        if (anim == null) return false;
        
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName) return true;
        }
        return false;
    }

    // Helper method to check if everything is set up correctly
    void OnValidate()
    {
        if (Application.isPlaying) return;
        
        if (GetComponent<Rigidbody>() == null)
            Debug.LogWarning("PlayerController requires a Rigidbody component!");
        if (GetComponent<CapsuleCollider>() == null)
            Debug.LogWarning("PlayerController requires a CapsuleCollider component!");
        if (GetComponent<PlayerInput>() == null)
            Debug.LogWarning("PlayerController requires a PlayerInput component!");
    }
}