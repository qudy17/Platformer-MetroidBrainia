using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 7f;

    [Header("Ограничение скорости падения")]
    public float maxFallSpeed = 20f;

    [Header("Проверка земли")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;
    public LayerMask groundLayer;

    // Направление взгляда игрока — читается из ScreamAbility
    // По умолчанию смотрим вправо
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

    // ─────────────────────────────────────────
    //  Ввод
    // ─────────────────────────────────────────

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

    // ─────────────────────────────────────────
    //  Направление взгляда
    // ─────────────────────────────────────────

    void UpdateFacingDirection()
    {
        // Приоритет: вертикаль важнее горизонтали
        // Если зажаты оба — смотрим по диагонали
        // Если ничего не зажато — сохраняем последнее направление

        if (horizontalInput != 0f || verticalInput != 0f)
        {
            FacingDirection = new Vector2(horizontalInput, verticalInput).normalized;
        }
    }

    // ─────────────────────────────────────────
    //  Движение
    // ─────────────────────────────────────────

    void Move()
    {
        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
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

    // ─────────────────────────────────────────
    //  Земля
    // ─────────────────────────────────────────

    void CheckGround()
    {
        IsGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // ─────────────────────────────────────────
    //  Флип спрайта (только по горизонтали)
    // ─────────────────────────────────────────

    void FlipCharacter()
    {
        // Флипаем только если есть горизонтальное движение
        if (horizontalInput > 0f)
            transform.localScale = new Vector3(1f, 1.5f, 1f);
        else if (horizontalInput < 0f)
            transform.localScale = new Vector3(-1f, 1.5f, 1f);
    }

    // ─────────────────────────────────────────
    //  Gizmos
    // ─────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Проверка земли
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Направление взгляда
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(
            transform.position,
            transform.position + (Vector3)FacingDirection
        );
    }
}