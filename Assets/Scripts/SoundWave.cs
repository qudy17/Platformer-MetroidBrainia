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

    [Header("Параметры удара по блоку")]
    public float blockImpactForceMultiplier = 1.2f;
    public bool destroyOnBlockHit = true;

    [Header("Отдача при ударе по блоку")]
    [Tooltip("Применять ли отдачу к игроку при ударе волны по блоку")]
    public bool applyRecoilOnBlockHit = true;

    // ─── НОВОЕ: флаг, предотвращающий двойное срабатывание ───
    private bool hasCollided = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
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
        // ─── КЛЮЧЕВОЕ ИЗМЕНЕНИЕ: проверяем флаг ПЕРВЫМ ДЕЛОМ ───
        if (hasCollided) return;

        int otherLayer = 1 << other.gameObject.layer;

        // Игнорируем игрока
        if ((otherLayer & playerLayer) != 0)
            return;

        // ── Подвижный блок ──────────────────────────────────────────────
        if ((otherLayer & movableBlockLayer) != 0)
        {
            hasCollided = true;  // <-- ставим флаг ДО обработки
            HandleBlockHit(other);
            return;
        }

        // ── Твёрдая поверхность ─────────────────────────────────────────
        if ((otherLayer & solidLayer) != 0)
        {
            hasCollided = true;  // <-- ставим флаг ДО обработки
            Debug.Log($"[SoundWave] Попал в поверхность: {other.gameObject.name}");
            ApplyRecoilToPlayer();
            DestroyWave();
            return;
        }
    }

    void HandleBlockHit(Collider2D blockCollider)
    {
        Debug.Log($"[SoundWave] Столкновение с: {blockCollider.gameObject.name}, слой: {LayerMask.LayerToName(blockCollider.gameObject.layer)}");

        // Ищем MovableBlock на самом объекте
        MovableBlock block = blockCollider.GetComponent<MovableBlock>();

        // Если не нашли — ищем в родителе (для групп блоков)
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

    void DestroyWave()
    {
        Destroy(gameObject);
    }
}