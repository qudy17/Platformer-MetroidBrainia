using UnityEngine;
// тест русского языка
public class PlayerRespawn : MonoBehaviour
{
    [Header("Настройки респавна")]
    [Tooltip("Начальная точка появления")]
    public Vector2 defaultSpawnPoint;

    [Tooltip("Задержка перед респавном")]
    public float respawnDelay = 0.5f;

    [Header("Визуал смерти")]
    [Tooltip("Эффект смерти (опционально)")]
    public GameObject deathEffectPrefab;

    private Vector2 currentRespawnPoint;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        currentRespawnPoint = defaultSpawnPoint;
        transform.position = currentRespawnPoint;
        Debug.Log($"[PlayerRespawn] Начальная точка возрождения: {currentRespawnPoint}");
    }

    public void SetRespawnPoint(Vector2 newPoint)
    {
        currentRespawnPoint = newPoint;
        Debug.Log($"[PlayerRespawn] Новая точка возрождения: {currentRespawnPoint}");
    }

    public void Respawn()
    {
        // Защита от повторного вызова
        if (isDead) return;

        StartCoroutine(RespawnCoroutine());
    }

    System.Collections.IEnumerator RespawnCoroutine()
    {
        isDead = true;
        Debug.Log($"[PlayerRespawn] Игрок умер. Возрождение через {respawnDelay} сек...");

        // Отключаем управление и визуал игрока
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic; // Отключаем физику
        }

        // Эффект смерти
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }

        // Ждём
        yield return new WaitForSeconds(respawnDelay);

        // Возрождаем
        transform.position = currentRespawnPoint;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Dynamic; // Возвращаем физику
        }

        if (spriteRenderer != null)
            spriteRenderer.enabled = true;

        if (playerMovement != null)
            playerMovement.enabled = true;

        isDead = false;
        Debug.Log($"[PlayerRespawn] Игрок возрождён в {currentRespawnPoint}");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(currentRespawnPoint, 0.3f);

        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(
                currentRespawnPoint + Vector2.up * 0.5f,
                "RESPAWN"
            );
        }
    }
}