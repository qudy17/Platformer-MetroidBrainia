using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class MovableBlock : MonoBehaviour
{
    [Header("Физика блока")]
    public float friction = 8f;
    public float maxSpeed = 12f;
    public float blockMass = 2f;
    public float waveForceMultiplier = 1f;

    [Header("Слои")]
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public LayerMask movableBlockLayer;

    [Header("Проверка земли")]
    public float groundCheckDistance = 0.1f;

    [Tooltip("С какой силой игрок может толкать активированный блок. 0 = не может толкать")]
    public float playerPushForce = 0f; // 0 = игрок не толкает блок

    private Rigidbody2D rb;
    private CompositeCollider2D compositeCollider;
    private BoxCollider2D boxCollider;
    private bool isGroup = false;
    private bool hasBeenHit = false;

    public bool IsGrounded { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        compositeCollider = GetComponent<CompositeCollider2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        isGroup = compositeCollider != null;

        rb.mass = blockMass;
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Dynamic, но с замороженной позицией
        rb.bodyType = RigidbodyType2D.Dynamic;
        FreezePosition();
    }

    void Start()
    {
        if (isGroup)
        {
            StartCoroutine(DelayedGroundCheck());
        }
        else
        {
            CheckIfShouldFall();
        }
    }

    IEnumerator DelayedGroundCheck()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        CheckIfShouldFall();
    }

    void CheckIfShouldFall()
    {
        CheckGround();

        if (!IsGrounded)
        {
            Debug.Log($"[MovableBlock] {gameObject.name}: в воздухе! Начинаю падение.");
            UnfreezeForPhysics();
        }
        else
        {
            Debug.Log($"[MovableBlock] {gameObject.name}: на земле. Заморожен.");
        }
    }

    void FixedUpdate()
    {
        if (!hasBeenHit) return;

        CheckGround();
        ApplyFriction();
        ClampSpeed();
    }

    void FreezePosition()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX |
                         RigidbodyConstraints2D.FreezePositionY |
                         RigidbodyConstraints2D.FreezeRotation;
    }

    void UnfreezeForPhysics()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        hasBeenHit = true;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        int otherLayer = 1 << collision.gameObject.layer;

        // Если игрок толкает замороженный блок — полностью игнорируем
        if ((otherLayer & playerLayer) != 0 && !hasBeenHit)
        {
            // Обнуляем силу, которую игрок передаёт блоку
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Если игрок толкает активированный блок и playerPushForce = 0 — игнорируем
        if ((otherLayer & playerLayer) != 0 && hasBeenHit && playerPushForce <= 0f)
        {
            // Не даём игроку менять скорость блока
            // Но коллизия остаётся (игрок не проходит сквозь)
            return;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        int otherLayer = 1 << collision.gameObject.layer;

        // При столкновении с другим активированным блоком — передаём импульс
        if ((otherLayer & movableBlockLayer) != 0)
        {
            MovableBlock otherBlock = collision.gameObject.GetComponent<MovableBlock>();
            if (otherBlock == null)
                otherBlock = collision.gameObject.GetComponentInParent<MovableBlock>();

            if (otherBlock != null && otherBlock.hasBeenHit && !hasBeenHit)
            {
                // Соседний блок активирован и толкает нас — активируемся
                Debug.Log($"[MovableBlock] {gameObject.name}: активирован соседним блоком!");
                UnfreezeForPhysics();
            }
        }
    }

    void CheckGround()
    {
        LayerMask combinedMask = groundLayer | movableBlockLayer;

        Vector2 size;
        Vector2 center;

        if (compositeCollider != null)
        {
            Bounds bounds = compositeCollider.bounds;
            if (bounds.size == Vector3.zero)
            {
                IsGrounded = CheckGroundByChildren(combinedMask);
                return;
            }
            size = bounds.size;
            center = bounds.center;
        }
        else if (boxCollider != null)
        {
            size = boxCollider.size;
            center = (Vector2)transform.position + boxCollider.offset;
        }
        else
        {
            IsGrounded = false;
            return;
        }

        IsGrounded = CheckGroundAtPoints(center, size, combinedMask);
    }

    bool CheckGroundByChildren(LayerMask mask)
    {
        BoxCollider2D[] childColliders = GetComponentsInChildren<BoxCollider2D>();

        foreach (var col in childColliders)
        {
            if (col.usedByComposite)
            {
                if (CheckGroundAtPoints(col.bounds.center, col.bounds.size, mask))
                    return true;
            }
        }

        return false;
    }

    bool CheckGroundAtPoints(Vector2 center, Vector2 size, LayerMask mask)
    {
        Vector2[] points = new Vector2[]
        {
            center + new Vector2(-size.x * 0.4f, -size.y * 0.5f),
            center + new Vector2( 0f,             -size.y * 0.5f),
            center + new Vector2( size.x * 0.4f, -size.y * 0.5f)
        };

        foreach (var point in points)
        {
            if (Physics2D.Raycast(point, Vector2.down, groundCheckDistance, mask).collider != null)
                return true;
        }

        return false;
    }

    void ApplyFriction()
    {
        if (!IsGrounded) return;

        float newVelX = Mathf.MoveTowards(
            rb.linearVelocity.x, 0f,
            friction * Time.fixedDeltaTime
        );
        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
    }

    void ClampSpeed()
    {
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed);
        float clampedY = Mathf.Clamp(rb.linearVelocity.y, -maxSpeed, maxSpeed);
        rb.linearVelocity = new Vector2(clampedX, clampedY);
    }

    public bool ReceiveWaveImpact(Vector2 waveDirection, float waveForce)
    {
        Debug.Log($"[MovableBlock] {gameObject.name}: удар волной! Сила: {waveForce}");

        UnfreezeForPhysics();

        float speed = waveForce / blockMass * waveForceMultiplier;
        rb.linearVelocity = new Vector2(waveDirection.x * speed, rb.linearVelocity.y);

        Debug.Log($"[MovableBlock] {gameObject.name}: скорость: {rb.linearVelocity}");

        return true;
    }

    void OnDrawGizmosSelected()
    {
        Vector2 size;
        Vector2 center;

        if (compositeCollider != null && compositeCollider.bounds.size != Vector3.zero)
        {
            Bounds bounds = compositeCollider.bounds;
            size = bounds.size;
            center = bounds.center;
        }
        else if (boxCollider != null)
        {
            size = boxCollider.size;
            center = (Vector2)transform.position + boxCollider.offset;
        }
        else return;

        Gizmos.color = IsGrounded ? Color.green : Color.red;

        foreach (var offset in new[] {
            new Vector2(-size.x * 0.4f, -size.y * 0.5f),
            new Vector2( 0f,            -size.y * 0.5f),
            new Vector2( size.x * 0.4f, -size.y * 0.5f)
        })
        {
            Vector2 point = center + offset;
            Gizmos.DrawLine(point, point + Vector2.down * groundCheckDistance);
            Gizmos.DrawWireSphere(point, 0.05f);
        }

        if (compositeCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(compositeCollider.bounds.center, compositeCollider.bounds.size);
        }

        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
            hasBeenHit ? $"ACTIVE\n{gameObject.name}" : $"FROZEN\n{gameObject.name}");
    }
}