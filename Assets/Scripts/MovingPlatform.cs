using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Точки маршрута")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Настройки движения")]
    public float speed = 3f;

    [Header("Привязка к кнопке")]
    public PressurePlate redButton;
    public bool requiresConstantWeight = true;

    [Header("Настройки платформы")]
    public bool canCarryPlayer = true;

    private Rigidbody2D rb;
    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 previousPosition;
    private bool buttonIsPressed = false;

    // Словарь для хранения информации об объектах на платформе
    private Dictionary<Rigidbody2D, Vector2> objectsOnPlatform = new Dictionary<Rigidbody2D, Vector2>();

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
            // Важно: создаём физический материал с трением
            if (col.sharedMaterial == null)
            {
                PhysicsMaterial2D material = new PhysicsMaterial2D("PlatformMaterial");
                material.friction = 1f;
                material.bounciness = 0f;
                col.sharedMaterial = material;
            }
        }
    }

    void Start()
    {
        startPos = startPoint != null ? (Vector2)startPoint.position : (Vector2)transform.position;
        endPos = endPoint != null ? (Vector2)endPoint.position : startPos;

        transform.position = startPos;
        previousPosition = startPos;

        if (redButton != null)
        {
            redButton.OnPlateStateChanged += OnButtonStateChanged;
            buttonIsPressed = redButton.IsPressed();
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

        // Перемещаем объекты вместе с платформой
        if (canCarryPlayer)
        {
            MoveObjectsOnPlatform();
        }
    }

    void OnButtonStateChanged(bool pressed)
    {
        buttonIsPressed = pressed;
    }

    void UpdateMovement()
    {
        Vector2 targetPos = buttonIsPressed ? endPos : startPos;
        Vector2 currentPos = rb.position;

        Vector2 direction = targetPos - currentPos;
        float distance = direction.magnitude;

        if (distance < 0.01f)
        {
            rb.position = targetPos;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 velocity = direction.normalized * speed;
        rb.linearVelocity = velocity;
    }

    void MoveObjectsOnPlatform()
    {
        Vector2 displacement = rb.position - previousPosition;
        if (displacement.magnitude < 0.001f) return;

        // Создаём копию ключей для безопасной итерации
        List<Rigidbody2D> keys = new List<Rigidbody2D>(objectsOnPlatform.Keys);

        foreach (Rigidbody2D objRb in keys)
        {
            if (objRb != null)
            {
                // Перемещаем объект
                objRb.position += displacement;

                // Корректируем скорость объекта, чтобы он не "скользил"
                Vector2 currentVelocity = objRb.linearVelocity;
                objRb.linearVelocity = new Vector2(
                    currentVelocity.x + displacement.x / Time.fixedDeltaTime,
                    currentVelocity.y
                );
            }
            else
            {
                objectsOnPlatform.Remove(objRb);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canCarryPlayer) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            // Проверяем, что объект стоит сверху на платформе
            if (contact.normal.y < -0.3f)
            {
                Rigidbody2D otherRb = collision.rigidbody;
                if (otherRb != null && !objectsOnPlatform.ContainsKey(otherRb))
                {
                    objectsOnPlatform.Add(otherRb, otherRb.position);
                }
                break;
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!canCarryPlayer) return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.3f)
            {
                Rigidbody2D otherRb = collision.rigidbody;
                if (otherRb != null && !objectsOnPlatform.ContainsKey(otherRb))
                {
                    objectsOnPlatform.Add(otherRb, otherRb.position);
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