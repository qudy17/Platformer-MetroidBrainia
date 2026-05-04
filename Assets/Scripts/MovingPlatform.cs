using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Точки маршрута")]
    [Tooltip("Начальная точка (позиция А)")]
    public Transform startPoint;

    [Tooltip("Конечная точка (позиция Б)")]
    public Transform endPoint;

    [Header("Настройки движения")]
    [Tooltip("Скорость движения платформы")]
    public float speed = 3f;

    [Header("Привязка к кнопке")]
    [Tooltip("Красная кнопка, которая активирует платформу")]
    public PressurePlate redButton;

    [Tooltip("Требуется ли постоянный вес на кнопке для удержания платформы")]
    public bool requiresConstantWeight = true;

    [Header("Настройки платформы")]
    [Tooltip("Можно ли стоять на платформе")]
    public bool canCarryPlayer = true;

    // Приватные переменные
    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 endPos;
    private HashSet<Rigidbody2D> objectsOnPlatform = new HashSet<Rigidbody2D>();
    private Vector2 previousPosition;
    private bool buttonIsPressed = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = false;
        }
    }

    void Start()
    {
        // Сохраняем позиции
        startPos = startPoint != null ? (Vector2)startPoint.position : (Vector2)transform.position;
        endPos = endPoint != null ? (Vector2)endPoint.position : startPos;

        // Ставим платформу на начальную позицию
        transform.position = startPos;
        previousPosition = startPos;

        // Подписываемся на события кнопки
        if (redButton != null)
        {
            redButton.OnPlateStateChanged += OnButtonStateChanged;
            // Сразу проверяем текущее состояние кнопки
            buttonIsPressed = redButton.IsPressed();
            Debug.Log($"[MovingPlatform] {gameObject.name}: Подписана на кнопку {redButton.name}. Начальное состояние: pressed={buttonIsPressed}");
        }
        else
        {
            Debug.LogWarning($"[MovingPlatform] {gameObject.name}: Красная кнопка не назначена!");
        }
    }

    void OnDestroy()
    {
        if (redButton != null)
        {
            redButton.OnPlateStateChanged -= OnButtonStateChanged;
        }
    }

    void FixedUpdate()
    {
        previousPosition = rb.position;
        UpdateMovement();
    }

    void LateUpdate()
    {
        if (canCarryPlayer)
        {
            MoveObjectsOnPlatform();
        }
    }

    void OnButtonStateChanged(bool pressed)
    {
        Debug.Log($"[MovingPlatform] {gameObject.name}: Кнопка изменила состояние: {buttonIsPressed} -> {pressed}");
        buttonIsPressed = pressed;
    }

    void UpdateMovement()
    {
        // Определяем целевую позицию в зависимости от состояния кнопки
        Vector2 targetPos = buttonIsPressed ? endPos : startPos;
        Vector2 currentPos = rb.position;

        Vector2 direction = targetPos - currentPos;
        float distance = direction.magnitude;

        // Если уже на месте - останавливаемся
        if (distance < 0.01f)
        {
            rb.position = targetPos;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Двигаемся к цели
        Vector2 velocity = direction.normalized * speed;
        rb.linearVelocity = velocity;
    }

    void MoveObjectsOnPlatform()
    {
        Vector2 displacement = rb.position - previousPosition;
        if (displacement.magnitude < 0.001f) return;

        foreach (Rigidbody2D objRb in objectsOnPlatform)
        {
            if (objRb != null)
            {
                objRb.position += displacement;
            }
        }

        objectsOnPlatform.RemoveWhere(rb => rb == null);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canCarryPlayer) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.5f)
            {
                Rigidbody2D otherRb = collision.rigidbody;
                if (otherRb != null)
                {
                    objectsOnPlatform.Add(otherRb);
                }
                break;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D otherRb = collision.rigidbody;
        if (otherRb != null)
        {
            objectsOnPlatform.Remove(otherRb);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector2 start = startPoint != null ? (Vector2)startPoint.position : (Vector2)transform.position;
        Vector2 end = endPoint != null ? (Vector2)endPoint.position : start;

        Gizmos.DrawWireSphere(start, 0.3f);
        Gizmos.DrawWireSphere(end, 0.3f);
        Gizmos.DrawLine(start, end);

        if (redButton != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, redButton.transform.position);
            Gizmos.DrawWireSphere(redButton.transform.position, 0.25f);
        }
    }
}