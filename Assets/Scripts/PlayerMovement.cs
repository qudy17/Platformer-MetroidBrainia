using UnityEngine;
using UnityEngine.InputSystem;
// тест русского языка
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 7f;

    [Header("Ускорение и торможение")]
    [Tooltip("Как быстро игрок разгоняется (выше = резче старт)")]
    public float acceleration = 50f;

    [Tooltip("Как быстро игрок тормозит когда не нажата клавиша")]
    public float deceleration = 40f;

    [Header("Ограничение скорости падения")]
    public float maxFallSpeed = 20f;

    [Header("Проверка земли")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    // Направление взгляда — читается из ScreamAbility
    public Vector2 FacingDirection { get; private set; } = Vector2.right;

    private Rigidbody2D rb;
    private float horizontalInput;
    private float verticalInput;

    public bool IsGrounded { get; private set; }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ReadInput();
        UpdateFacingDirection();
        CheckGround();
        FlipCharacter();
    }

    void FixedUpdate()
    {
        Move();
        ClampFallSpeed();
    }

    void ReadInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Keyboard.current.dKey.isPressed ||
            Keyboard.current.rightArrowKey.isPressed)
            horizontalInput = 1f;
        else if (Keyboard.current.aKey.isPressed ||
                 Keyboard.current.leftArrowKey.isPressed)
            horizontalInput = -1f;

        if (Keyboard.current.wKey.isPressed ||
            Keyboard.current.upArrowKey.isPressed)
            verticalInput = 1f;
        else if (Keyboard.current.sKey.isPressed ||
                 Keyboard.current.downArrowKey.isPressed)
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
        float targetSpeedX = horizontalInput * moveSpeed;

        float newSpeedX;

        if (horizontalInput != 0f)
        {
            // Игрок нажал клавишу — разгоняемся к целевой скорости
            newSpeedX = Mathf.MoveTowards(
                currentSpeedX,
                targetSpeedX,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // Клавиша не нажата — тормозим
            // Но НЕ гасим импульс от отдачи резко — торможение постепенное
            newSpeedX = Mathf.MoveTowards(
                currentSpeedX,
                0f,
                deceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = new Vector2(newSpeedX, rb.linearVelocity.y);
    }

    void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -maxFallSpeed
            );
        }
    }

    void CheckGround()
    {
        IsGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void FlipCharacter()
    {
        if (horizontalInput > 0f)
            transform.localScale = new Vector3(1f, 1f, 1f);
        else if (horizontalInput < 0f)
            transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3)FacingDirection
        );
    }
}