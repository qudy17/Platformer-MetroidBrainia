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

    [Header("Слои")]
    [Tooltip("Слои твёрдых поверхностей — волна останавливается")]
    public LayerMask solidLayer;

    [Tooltip("Слой игрока — волна игнорирует его коллайдер")]
    public LayerMask playerLayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
    }

    public void Initialize(Vector2 dir, float spd, float maxDist)
    {
        direction = dir.normalized;
        speed = spd;
        maxDistance = maxDist;
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
            DestroyWave();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        int otherLayer = 1 << other.gameObject.layer;

        // Игнорируем игрока полностью
        if ((otherLayer & playerLayer) != 0)
        {
            return;
        }

        // Твёрдая поверхность — останавливаемся
        if ((otherLayer & solidLayer) != 0)
        {
            Debug.Log($"[SoundWave] Попал в поверхность: {other.gameObject.name}");
            DestroyWave();
            return;
        }

        // ЗАГЛУШКА
        Debug.Log($"[SoundWave] Попал в объект: {other.gameObject.name} " +
                  $"(слой: {LayerMask.LayerToName(other.gameObject.layer)})");
    }

    void DestroyWave()
    {
        Destroy(gameObject);
    }
}