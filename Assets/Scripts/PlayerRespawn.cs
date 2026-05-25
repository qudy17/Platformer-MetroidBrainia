using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Настройки респавна")]
    [Tooltip("Начальная точка появления (если нет сохранённого чекпоинта)")]
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

    void Awake()
    {
        // ВАЖНО: Устанавливаем позицию в Awake, ДО инициализации камеры
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        LoadLastCheckpoint();
    }

    void Start()
    {
        Debug.Log($"[PlayerRespawn] Точка возрождения: {currentRespawnPoint}");

        // Принудительно обновляем камеру после загрузки
        CameraFollow camera = FindFirstObjectByType<CameraFollow>();
        if (camera != null)
        {
            camera.ForceSetToCurrentRoom();
        }
    }

    void LoadLastCheckpoint()
    {
        string savedCheckpointID = Checkpoint.GetSavedCheckpointID();
        string savedScene = Checkpoint.GetSavedCheckpointScene();
        string currentScene = SceneManager.GetActiveScene().name;

        // Проверяем, есть ли сохранённый чекпоинт для текущей сцены
        if (!string.IsNullOrEmpty(savedCheckpointID) && savedScene == currentScene)
        {
            Vector2 savedPosition = Checkpoint.GetSavedCheckpointPosition(savedCheckpointID);

            // Проверяем, что позиция валидная
            if (savedPosition != Vector2.zero)
            {
                currentRespawnPoint = savedPosition;
                transform.position = currentRespawnPoint;
                Debug.Log($"[PlayerRespawn] Загружен сохранённый чекпоинт: {savedCheckpointID.Substring(0, 8)} в {currentRespawnPoint}");
                return;
            }
        }

        // Если нет сохранённого чекпоинта, используем дефолтную позицию
        currentRespawnPoint = defaultSpawnPoint;
        transform.position = currentRespawnPoint;
        Debug.Log($"[PlayerRespawn] Использована начальная точка: {currentRespawnPoint}");
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

        // Регистрируем смерть в статистике
        GameStatsTracker.RegisterDeath();

        // Отключаем управление и визуал игрока
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
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
            rb.bodyType = RigidbodyType2D.Dynamic;
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

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            UnityEditor.Handles.Label(
                currentRespawnPoint + Vector2.up * 0.5f,
                "RESPAWN"
            );
        }
#endif
    }
}