using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Настройки респавна")]
    [Tooltip("Начальная точка появления")]
    public Vector2 defaultSpawnPoint;

    [Tooltip("Задержка перед респавном")]
    public float respawnDelay = 0.5f;

    private Vector2 currentRespawnPoint;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

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
        StartCoroutine(RespawnCoroutine());
    }

    System.Collections.IEnumerator RespawnCoroutine()
    {
        Debug.Log($"[PlayerRespawn] Возрождение через {respawnDelay} сек...");
        yield return new WaitForSeconds(respawnDelay);

        transform.position = currentRespawnPoint;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

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