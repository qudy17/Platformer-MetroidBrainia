using UnityEngine;
using System.Collections;
// тест русского языка
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Физика")]
    public float friction = 8f;
    public float maxSpeed = 20f;
    public float enemyMass = 80f;
    public float waveForceMultiplier = 40f;

    [Header("Слои")]
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public LayerMask movableBlockLayer;
    public LayerMask spikesLayer;

    [Header("Проверка земли")]
    public float groundCheckDistance = 0.1f;

    [Header("Смерть")]
    [Tooltip("Эффект смерти врага (опционально)")]
    public GameObject deathEffectPrefab;

    private Rigidbody2D rb;
    private Collider2D mainCollider;
    private SpriteRenderer spriteRenderer;
    private bool hasBeenHit = false;
    private bool isDead = false;
    private bool isKillingPlayer = false;

    public bool IsGrounded { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        enemyMass = rb.mass;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        rb.bodyType = RigidbodyType2D.Dynamic;

        // ВАЖНО: Замораживаем позицию по X (не даём игроку толкать)
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        StartCoroutine(DelayedGroundCheck());
    }

    IEnumerator DelayedGroundCheck()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        CheckIfShouldFall();
    }

    void CheckIfShouldFall()
    {
        CheckGround();

        if (!IsGrounded)
        {
            Debug.Log($"[Enemy] {gameObject.name}: в воздухе! Включаю гравитацию.");
            rb.gravityScale = 3f;
            hasBeenHit = true;
            // РАЗМОРАЖИВАЕМ позицию X для физики
            UnfreezePosition();
        }
        else
        {
            Debug.Log($"[Enemy] {gameObject.name}: на земле. Остаётся с нулевой гравитацией.");
        }
    }

    void FixedUpdate()
    {
        if (!hasBeenHit || isDead) return;

        CheckGround();
        ApplyFriction();
        ClampSpeed();
    }

    void UnfreezePosition()
    {
        // Размораживаем X, чтобы враг мог двигаться под действием физики
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void ActivateEnemy()
    {
        if (!hasBeenHit && !isDead)
        {
            hasBeenHit = true;
            rb.gravityScale = 3f;
            UnfreezePosition();
            Debug.Log($"[Enemy] {gameObject.name}: АКТИВИРОВАН!");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        int otherLayer = 1 << collision.gameObject.layer;

        // Игрок касается врага — игрок умирает
        if ((otherLayer & playerLayer) != 0)
        {
            KillPlayer(collision.gameObject);
            GameStatsTracker.RegisterDeath();
            return;
        }

        // Подвижный блок падает на врага — враг умирает
        if ((otherLayer & movableBlockLayer) != 0)
        {
            HandleMovableBlockHit(collision);
            return;
        }

        // Враг касается шипов — враг умирает
        if ((otherLayer & spikesLayer) != 0)
        {
            Die();
        }
    }

    void HandleMovableBlockHit(Collision2D collision)
    {
        MovableBlock block = collision.gameObject.GetComponent<MovableBlock>();
        if (block == null)
            block = collision.gameObject.GetComponentInParent<MovableBlock>();

        if (block != null)
        {
            // Проверяем скорость блока по Y — если падает, убиваем врага
            Rigidbody2D blockRb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (blockRb == null)
                blockRb = collision.gameObject.GetComponentInParent<Rigidbody2D>();

            if (blockRb != null && blockRb.linearVelocity.y < -0.5f)
            {
                // Блок падает сверху — убиваем врага
                Debug.Log($"[Enemy] {gameObject.name}: Подвижный блок упал на врага!");
                Die();
            }
            else
            {
                // Блок просто касается — активируем врага
                if (!hasBeenHit)
                {
                    ActivateEnemy();
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;

        int otherLayer = 1 << collision.gameObject.layer;

        // Игрок продолжает касаться врага
        if ((otherLayer & playerLayer) != 0)
        {

        }
    }

    void KillPlayer(GameObject player)
    {
        // Защита от повторного вызова
        if (isDead || isKillingPlayer) return;

        isKillingPlayer = true;

        Debug.Log($"[Enemy] Игрок коснулся врага!");

        PlayerRespawn playerRespawn = player.GetComponent<PlayerRespawn>();
        if (playerRespawn != null)
        {
            StartCoroutine(KillPlayerSequence(playerRespawn));
        }
        else
        {
            isKillingPlayer = false;
        }
    }

    System.Collections.IEnumerator KillPlayerSequence(PlayerRespawn playerRespawn)
    {
        yield return new WaitForSeconds(0.1f);
        playerRespawn.Respawn();

        isKillingPlayer = false;
    }

    void CheckGround()
    {
        LayerMask combinedMask = groundLayer | movableBlockLayer | spikesLayer;

        Vector2[] checkPoints = GetGroundCheckPoints();

        foreach (var point in checkPoints)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(point, Vector2.down, groundCheckDistance, combinedMask);

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (hit.collider.transform.IsChildOf(transform)) continue;
                if (hit.collider.transform == transform) continue;

                // Игрок не является опорой
                if (((1 << hit.collider.gameObject.layer) & playerLayer) != 0) continue;

                IsGrounded = true;
                return;
            }
        }

        IsGrounded = false;
    }

    Vector2[] GetGroundCheckPoints()
    {
        Bounds bounds = mainCollider.bounds;
        Vector2 center = bounds.center;
        Vector2 size = bounds.size;

        return new Vector2[]
        {
            center + new Vector2(-size.x * 0.4f, -size.y * 0.5f),
            center + new Vector2(0f, -size.y * 0.5f),
            center + new Vector2(size.x * 0.4f, -size.y * 0.5f)
        };
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
        if (isDead) return false;

        Debug.Log($"[Enemy] {gameObject.name}: удар волной! Сила: {waveForce}");

        ActivateEnemy();

        float speed = waveForce / enemyMass * waveForceMultiplier;
        rb.linearVelocity = new Vector2(waveDirection.x * speed, rb.linearVelocity.y);

        return true;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"[Enemy] {gameObject.name}: Враг умирает!");

        // Отключаем коллайдер и спрайт
        if (mainCollider != null) mainCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // Эффект смерти
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Удаляем объект
        Destroy(gameObject, 0.5f);
    }

    void OnDrawGizmosSelected()
    {
        if (mainCollider == null) return;

        Bounds bounds = mainCollider.bounds;
        Vector2 center = bounds.center;
        Vector2 size = bounds.size;

        Gizmos.color = IsGrounded ? Color.green : Color.red;

        Vector2[] points = new Vector2[]
        {
            center + new Vector2(-size.x * 0.4f, -size.y * 0.5f),
            center + new Vector2(0f, -size.y * 0.5f),
            center + new Vector2(size.x * 0.4f, -size.y * 0.5f)
        };

        foreach (var point in points)
        {
            Gizmos.DrawLine(point, point + Vector2.down * groundCheckDistance);
            Gizmos.DrawWireSphere(point, 0.05f);
        }

        Gizmos.color = hasBeenHit ? Color.yellow : Color.gray;
        Gizmos.DrawWireCube(center, size);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f,
            hasBeenHit ? $"ACTIVE\n{gameObject.name}" : $"FROZEN\n{gameObject.name}");
    }
}