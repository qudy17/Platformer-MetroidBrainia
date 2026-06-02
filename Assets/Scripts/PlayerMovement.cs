using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 7f;

    [Header("Ускорение и торможение")]
    public float acceleration = 50f;
    public float deceleration = 40f;

    [Header("Ограничение скорости падения")]
    public float maxFallSpeed = 20f;

    [Header("Проверка земли")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Анимация")]
    private Animator animator;

    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float verticalInput;
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;

    public bool IsGrounded { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Важно для работы с платформами
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Update()
    {
        ReadInput();
        UpdateFacingDirection();
        CheckGround();
        FlipCharacter();
        CheckPlatformParent();
    }

    void FixedUpdate()
    {
        Move();
        ClampFallSpeed();
        UpdateAnimation();
        // Обновляем позицию платформы
        if (currentPlatform != null)
        {
            lastPlatformPosition = currentPlatform.position;
        }
    }

    void ReadInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            verticalInput = 1f;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            verticalInput = -1f;
    }

    void UpdateFacingDirection()
    {
        if (horizontalInput != 0f || verticalInput != 0f)
        {
            FacingDirection = new Vector2(horizontalInput, verticalInput).normalized;
        }
    }

    void Move()
    {
        float currentSpeedX = rb.linearVelocity.x;

        // Если стоим на движущейся платформе, учитываем её скорость
        float platformVelocityX = 0f;
        if (currentPlatform != null && IsGrounded)
        {
            Rigidbody2D platformRb = currentPlatform.GetComponent<Rigidbody2D>();
            if (platformRb != null)
            {
                platformVelocityX = platformRb.linearVelocity.x;
            }
        }

        float targetSpeedX = horizontalInput * moveSpeed + platformVelocityX;
        float newSpeedX;

        if (horizontalInput != 0f)
        {
            newSpeedX = Mathf.MoveTowards(
                currentSpeedX,
                targetSpeedX,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // Если клавиши не нажаты, тормозим до скорости платформы
            newSpeedX = Mathf.MoveTowards(
                currentSpeedX,
                platformVelocityX,
                deceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = new Vector2(newSpeedX, rb.linearVelocity.y);
    }

    void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    void CheckGround()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Определяем, на какой платформе стоим
        if (IsGrounded)
        {
            Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (groundCollider != null) //&& groundCollider.CompareTag("MovingPlatform")
            {
                currentPlatform = groundCollider.transform;
            }
        }
        else
        {
            currentPlatform = null;
        }
    }

    void CheckPlatformParent()
    {
        // Этот метод больше не нужен, так как не используем parenting
    }

    void FlipCharacter()
    {
        if (horizontalInput > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontalInput < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    void UpdateAnimation()
    {
        // Вариант 1: Если хотите плавно по скорости
        if (IsGrounded)
        {
            float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
            float threshold = 0.5f; // Порог для переключения

            if (horizontalSpeed > threshold)
            {
                animator.SetFloat("Speed", 1f);
                
            }
            else if (horizontalInput != 0 && horizontalSpeed > 0.1f)
            {
                animator.SetFloat("Speed", 1f);
            }
            else
            {
                animator.SetFloat("Speed", 0f);
            }
        }
        else
        {
            animator.SetFloat("Speed", 0f);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)FacingDirection);
    }
}