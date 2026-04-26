using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class SoundWave : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float maxDistance;
    private Vector2 startPosition;
    private Rigidbody2D rb;
    private bool initialized = false;

    private Rigidbody2D playerRb;
    private float recoilForce;

    [Header("Слои")]
    public LayerMask solidLayer;
    public LayerMask playerLayer;
    public LayerMask movableBlockLayer;
    public LayerMask fragileBlockLayer;
    public LayerMask enemyLayer;

    [Header("Параметры удара по блоку")]
    public float blockImpactForceMultiplier = 1.2f;
    public bool destroyOnBlockHit = true;

    [Header("Отдача при ударе по блоку")]
    [Tooltip("Применять ли отдачу к игроку при ударе волны по блоку")]
    public bool applyRecoilOnBlockHit = true;

    [Header("Хрупкие блоки")] // ДОБАВЛЕНО
    public float fragileBlockRecoilRadius = 3f; // Радиус отдачи при разрушении
    public float fragileBlockRecoilForce = 10f; // Сила отдачи при разрушении

    // ─── Флаг, предотвращающий двойное срабатывание ───
    private bool hasCollided = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        hasCollided = false;
    }

    public void Initialize(
        Vector2 dir,
        float spd,
        float maxDist,
        Rigidbody2D playerRigidbody,
        float recoil)
    {
        direction = dir.normalized;
        speed = spd;
        maxDistance = maxDist;
        playerRb = playerRigidbody;
        recoilForce = recoil;
        startPosition = transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        rb.linearVelocity = direction * speed;
        initialized = true;

        // Сбрасываем флаг при инициализации
        hasCollided = false;
    }

    void Update()
    {
        if (!initialized) return;

        float distanceTravelled = Vector2.Distance(startPosition, transform.position);
        if (distanceTravelled >= maxDistance)
        {
            DestroyWave();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // ─── Проверяем флаг ПЕРВЫМ ДЕЛОМ ───
        if (hasCollided) return;

        int otherLayer = 1 << other.gameObject.layer;

        // Игнорируем игрока
        if ((otherLayer & playerLayer) != 0)
            return;

        // ── Подвижный блок ──────────────────────────────────────────────
        if ((otherLayer & movableBlockLayer) != 0)
        {
            hasCollided = true;
            HandleBlockHit(other);
            return;
        }

        // ── Хрупкий блок ──────────────────────────────────────────────
        if ((otherLayer & fragileBlockLayer) != 0)
        {
            hasCollided = true;
            HandleFragileBlockHit(other);
            return;
        }

        // ── Твёрдая поверхность ─────────────────────────────────────────
        if ((otherLayer & solidLayer) != 0)
        {
            hasCollided = true;
            Debug.Log($"[SoundWave] Попал в поверхность: {other.gameObject.name}");
            ApplyRecoilToPlayer();
            DestroyWave();
            return;
        }

        if ((otherLayer & enemyLayer) != 0)
        {
            hasCollided = true;
            HandleEnemyHit(other);
            return;
        }
    }

    void HandleBlockHit(Collider2D blockCollider)
    {
        Debug.Log($"[SoundWave] Столкновение с: {blockCollider.gameObject.name}, слой: {LayerMask.LayerToName(blockCollider.gameObject.layer)}");

        MovableBlock block = blockCollider.GetComponent<MovableBlock>();

        if (block == null)
        {
            block = blockCollider.GetComponentInParent<MovableBlock>();
            Debug.Log($"[SoundWave] Поиск в родителе: {(block != null ? "НАЙДЕН" : "НЕ НАЙДЕН")}");
        }
        else
        {
            Debug.Log("[SoundWave] MovableBlock найден на самом объекте");
        }

        if (block != null)
        {
            float impactForce = recoilForce * blockImpactForceMultiplier;
            Debug.Log($"[SoundWave] Вызываю ReceiveWaveImpact с силой: {impactForce}");
            bool hitSuccess = block.ReceiveWaveImpact(direction, impactForce);

            if (applyRecoilOnBlockHit && hitSuccess)
            {
                ApplyRecoilToPlayer();
            }
        }
        else
        {
            Debug.LogWarning($"[SoundWave] MovableBlock не найден на {blockCollider.gameObject.name} и его родителях!");
        }

        if (destroyOnBlockHit)
        {
            DestroyWave();
        }
    }

    void HandleFragileBlockHit(Collider2D blockCollider)
    {
        Debug.Log($"[SoundWave] Столкновение с хрупким блоком: {blockCollider.gameObject.name}");

        FragileBlock fragileBlock = blockCollider.GetComponent<FragileBlock>();

        if (fragileBlock == null)
        {
            fragileBlock = blockCollider.GetComponentInParent<FragileBlock>();
        }

        if (fragileBlock != null)
        {
            // Разрушаем блок
            fragileBlock.DestroyBlock();

            // Проверяем, близко ли игрок — если да, откидываем
            if (playerRb != null)
            {
                float distanceToPlayer = Vector2.Distance(
                    transform.position,
                    playerRb.transform.position
                );

                if (distanceToPlayer <= fragileBlockRecoilRadius)
                {
                    ApplyFragileBlockRecoil();
                }
            }
        }

        // Уничтожаем волну
        DestroyWave();
    }

    void ApplyFragileBlockRecoil()
    {
        if (playerRb == null) return;

        Vector2 recoilDir = -direction;

        if (recoilDir.y > 0.3f)
        {
            playerRb.linearVelocity = new Vector2(
                playerRb.linearVelocity.x,
                0f
            );
        }

        playerRb.AddForce(recoilDir * fragileBlockRecoilForce, ForceMode2D.Impulse);

        Debug.Log($"[SoundWave] Отдача от хрупкого блока: направление {recoilDir}, сила {fragileBlockRecoilForce}");
    }

    public void HitFragileBlock()
    {
        // Уже обработано в HandleFragileBlockHit
        DestroyWave();
    }

    void ApplyRecoilToPlayer()
    {
        if (playerRb == null)
        {
            Debug.LogError("[SoundWave] playerRb не назначен!");
            return;
        }

        Vector2 recoilDir = -direction;

        if (recoilDir.y > 0.3f)
        {
            playerRb.linearVelocity = new Vector2(
                playerRb.linearVelocity.x,
                0f
            );
        }

        playerRb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);

        Debug.Log($"[SoundWave] Отдача: направление {recoilDir}, сила {recoilForce}");
    }

    void HandleEnemyHit(Collider2D enemyCollider)
    {
        Enemy enemy = enemyCollider.GetComponent<Enemy>();
        if (enemy != null)
        {
            float impactForce = recoilForce * blockImpactForceMultiplier;
            enemy.ReceiveWaveImpact(direction, impactForce);
        }

        if (destroyOnBlockHit)
        {
            DestroyWave();
        }
    }

    void DestroyWave()
    {
        Destroy(gameObject);
    }
}