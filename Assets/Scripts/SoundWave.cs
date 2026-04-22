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

    // Rigidbody игрока — применяем отдачу когда волна врезается
    private Rigidbody2D playerRb;
    private float recoilForce;

    [Header("Слои")]
    [Tooltip("Слои твёрдых поверхностей — волна останавливается и толкает игрока")]
    public LayerMask solidLayer;

    [Tooltip("Слой игрока — волна игнорирует")]
    public LayerMask playerLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// Инициализация волны после спавна.
    /// </summary>
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
    }

    void Update()
    {
        if (!initialized) return;

        float distanceTravelled = Vector2.Distance(startPosition, transform.position);
        if (distanceTravelled >= maxDistance)
        {
            // Волна улетела слишком далеко — отдачи нет
            DestroyWave();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = 1 << other.gameObject.layer;

        // Игнорируем игрока
        if ((otherLayer & playerLayer) != 0)
        {
            return;
        }

        // Твёрдая поверхность — отталкиваем игрока и уничтожаем волну
        if ((otherLayer & solidLayer) != 0)
        {
            Debug.Log($"[SoundWave] Попал в поверхность: {other.gameObject.name} " +
                      $"→ применяем отдачу");

            ApplyRecoilToPlayer();
            DestroyWave();
            return;
        }

        // ЗАГЛУШКА — другие объекты добавим позже
        Debug.Log($"[SoundWave] Попал в объект: {other.gameObject.name} " +
                  $"(слой: {LayerMask.LayerToName(other.gameObject.layer)})");
    }

    void ApplyRecoilToPlayer()
    {
        if (playerRb == null)
        {
            Debug.LogError("[SoundWave] playerRb не назначен!");
            return;
        }

        // Отдача — противоположно направлению волны
        Vector2 recoilDir = -direction;

        // Крик вниз (волна летит вниз, врезается в пол)
        // → отдача вверх → сбрасываем вертикальную скорость для чистого прыжка
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