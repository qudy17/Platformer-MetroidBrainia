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
    public LayerMask acousticMirrorLayer;
    public LayerMask resonancePipeLayer;

    [Header("Параметры удара по блоку")]
    public float blockImpactForceMultiplier = 1.2f;
    public bool destroyOnBlockHit = true;

    [Header("Отдача при ударе по блоку")]
    [Tooltip("Применять ли отдачу к игроку при ударе волны по блоку")]
    public bool applyRecoilOnBlockHit = true;

    [Tooltip("Максимальное расстояние до игрока для применения отдачи")]
    public float recoilMaxDistance = 5f;

    [Header("Хрупкие блоки")]
    public float fragileBlockRecoilRadius = 3f;
    public float fragileBlockRecoilForce = 10f;

    [Header("Отражение")]
    [Tooltip("Максимальное количество отражений")]
    public int maxReflections = 10;
    private int currentReflections = 0;
    private bool hasReflected = false;

    private bool hasCollided = false;

    public Vector2 GetDirection()
    {
        return direction;
    }

    public float GetSpeed()
    {
        return speed;
    }

    public float GetMaxDistance()
    {
        return maxDistance;
    }

    public Rigidbody2D GetPlayerRb()
    {
        return playerRb;
    }

    public float GetRecoilForce()
    {
        return recoilForce;
    }

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
        hasCollided = false;
        currentReflections = 0;
        hasReflected = false;
    }

    void Update()
    {
        if (!initialized) return;

        if (hasReflected) return;

        float distanceTravelled = Vector2.Distance(startPosition, transform.position);

        // ДЕТАЛЬНЫЙ ЛОГ
        Debug.Log($"[SoundWave] Update: startPos={startPosition}, currentPos={transform.position}, " +
                  $"travelled={distanceTravelled:F3}, maxDist={maxDistance:F3}, " +
                  $"travelled >= maxDist = {distanceTravelled >= maxDistance}");

        if (distanceTravelled >= maxDistance)
        {
            Debug.Log($"[SoundWave] Превышена максимальная дистанция ({maxDistance})");
            DestroyWave();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasCollided) return;

        int otherLayer = 1 << other.gameObject.layer;

        if ((otherLayer & playerLayer) != 0) return;

        // Резонансные трубы — НЕ УНИЧТОЖАЕМ волну здесь!
        // Обработка идёт в ResonancePipe.OnTriggerEnter2D()
        if ((otherLayer & resonancePipeLayer) != 0)
        {
            return;
        }

        // Акустическое зеркало — ОТРАЖАЕМСЯ
        if ((otherLayer & acousticMirrorLayer) != 0)
        {
            HandleMirrorReflection(other);
            return;
        }

        // Подвижный блок
        if ((otherLayer & movableBlockLayer) != 0)
        {
            hasCollided = true;
            HandleBlockHit(other);
            return;
        }

        // Хрупкий блок
        if ((otherLayer & fragileBlockLayer) != 0)
        {
            hasCollided = true;
            HandleFragileBlockHit(other);
            return;
        }

        // Твёрдая поверхность
        if ((otherLayer & solidLayer) != 0)
        {
            hasCollided = true;
            Debug.Log($"[SoundWave] Попал в поверхность: {other.gameObject.name}");
            TryApplyRecoilToPlayer();
            DestroyWave();
            return;
        }

        // Враг
        if ((otherLayer & enemyLayer) != 0)
        {
            hasCollided = true;
            HandleEnemyHit(other);
            return;
        }
    }

    void HandleMirrorReflection(Collider2D mirrorCollider)
    {
        if (currentReflections >= maxReflections)
        {
            Debug.Log($"[SoundWave] Достигнут лимит отражений ({maxReflections})");
            TryApplyRecoilToPlayer(); // ИСПРАВЛЕНО
            DestroyWave();
            return;
        }

        AcousticMirror mirror = mirrorCollider.GetComponent<AcousticMirror>();
        if (mirror == null)
            mirror = mirrorCollider.GetComponentInParent<AcousticMirror>();

        if (mirror != null)
        {
            Vector2 surfaceNormal = mirror.GetSurfaceNormal(transform.position);
            Vector2 reflectDirection = Vector2.Reflect(direction, surfaceNormal).normalized;

            Debug.Log($"[SoundWave] Отражение! Вход: {direction}, Нормаль: {surfaceNormal}, Выход: {reflectDirection}");

            direction = reflectDirection;
            hasReflected = true;
            rb.linearVelocity = direction * speed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            currentReflections++;
        }
        else
        {
            hasCollided = true;
            TryApplyRecoilToPlayer(); // ИСПРАВЛЕНО
            DestroyWave();
        }
    }

    void HandleBlockHit(Collider2D blockCollider)
    {
        MovableBlock block = blockCollider.GetComponent<MovableBlock>();
        if (block == null)
            block = blockCollider.GetComponentInParent<MovableBlock>();

        if (block != null)
        {
            float impactForce = recoilForce * blockImpactForceMultiplier;
            bool hitSuccess = block.ReceiveWaveImpact(direction, impactForce);

            if (applyRecoilOnBlockHit && hitSuccess)
            {
                TryApplyRecoilToPlayer(); // ИСПРАВЛЕНО
            }
        }

        if (destroyOnBlockHit)
        {
            DestroyWave();
        }
    }

    public void HitResonancePipe()
    {
        // Просто заглушка, чтобы избежать ошибок
        // Волна уничтожается в ResonancePipe.ReceiveWave()
    }

    void HandleFragileBlockHit(Collider2D blockCollider)
    {
        FragileBlock fragileBlock = blockCollider.GetComponent<FragileBlock>();
        if (fragileBlock == null)
            fragileBlock = blockCollider.GetComponentInParent<FragileBlock>();

        if (fragileBlock != null)
        {
            fragileBlock.DestroyBlock();

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
    }

    public void HitFragileBlock()
    {
        DestroyWave();
    }

    // НОВЫЙ МЕТОД: проверяет расстояние до игрока перед отдачей
    void TryApplyRecoilToPlayer()
    {
        if (playerRb == null) return;

        // Проверяем расстояние до игрока
        float distanceToPlayer = Vector2.Distance(
            transform.position,
            playerRb.transform.position
        );

        // Если игрок слишком далеко — не применяем отдачу
        if (distanceToPlayer > recoilMaxDistance)
        {
            Debug.Log($"[SoundWave] Игрок слишком далеко для отдачи ({distanceToPlayer:F1} > {recoilMaxDistance})");
            return;
        }

        // Применяем отдачу (уменьшаем силу в зависимости от расстояния)
        float distanceFactor = 1f - (distanceToPlayer / recoilMaxDistance);
        float adjustedRecoil = recoilForce * distanceFactor;

        Vector2 recoilDir = -direction;

        if (recoilDir.y > 0.3f)
        {
            playerRb.linearVelocity = new Vector2(
                playerRb.linearVelocity.x,
                0f
            );
        }

        playerRb.AddForce(recoilDir * adjustedRecoil, ForceMode2D.Impulse);

        Debug.Log($"[SoundWave] Отдача: направление {recoilDir}, сила {adjustedRecoil:F1} (дистанция: {distanceToPlayer:F1})");
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
        Debug.Log($"[SoundWave] DestroyWave вызван! Позиция: {transform.position}, Имя: {gameObject.name}");
        Destroy(gameObject);
    }
}