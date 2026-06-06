using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

public class Checkpoint : MonoBehaviour
{
    [Header("Визуал")]
    [Tooltip("Спрайт активного чекпоинта")]
    public Sprite activeSprite;

    [Tooltip("Спрайт неактивного чекпоинта")]
    public Sprite inactiveSprite;

    [Tooltip("Цвет активного чекпоинта")]
    public Color activeColor = Color.green;

    [Tooltip("Цвет неактивного чекпоинта")]
    public Color inactiveColor = Color.gray;

    [Header("Идентификация")]
    [Tooltip("Уникальный ID чекпоинта (автоматически генерируется)")]
    public string checkpointID;

    [Header("Слои")]
    public LayerMask playerLayer;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    private static Checkpoint currentCheckpoint;
    private Tilemap parentTilemap;
    private Vector3Int cellPosition;

    void Awake()
    {
        if (string.IsNullOrEmpty(checkpointID))
        {
            checkpointID = System.Guid.NewGuid().ToString();
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
    }

    void Start()
    {
        parentTilemap = GetComponentInParent<Tilemap>();
        if (parentTilemap != null)
        {
            cellPosition = parentTilemap.WorldToCell(transform.position);

            parentTilemap.SetTileFlags(cellPosition, TileFlags.None);
            parentTilemap.SetColor(cellPosition, new Color(0f, 0f, 0f, 0f));
        }

        SetVisualState(false);
        Debug.Log($"[Checkpoint] Инициализирован в позиции {transform.position}, ID: {checkpointID}");

        string savedCheckpointID = GetSavedCheckpointID();
        string currentScene = SceneManager.GetActiveScene().name;
        string savedScene = GetSavedCheckpointScene();

        if (savedCheckpointID == checkpointID && savedScene == currentScene)
        {
            ActivateCheckpoint(false); // false = не сохранять снова
            Debug.Log($"[Checkpoint] Восстановлен последний чекпоинт: {checkpointID}");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            ActivateCheckpoint(true);
        }
    }

    void ActivateCheckpoint(bool shouldSave = true)
    {
        if (isActivated) return;

        isActivated = true;
        Debug.Log($"[Checkpoint] Активирован в позиции {transform.position}");

        if (currentCheckpoint != null && currentCheckpoint != this)
        {
            currentCheckpoint.DeactivateCheckpoint();
        }

        currentCheckpoint = this;
        SetVisualState(true);

        if (shouldSave)
        {
            SaveCheckpoint();
        }

        SetPlayerRespawnPoint();
    }

    public void DeactivateCheckpoint()
    {
        isActivated = false;
        SetVisualState(false);
    }

    void SetVisualState(bool active)
    {
        if (spriteRenderer == null) return;

        if (active)
        {
            if (activeSprite != null) spriteRenderer.sprite = activeSprite;
            spriteRenderer.color = activeColor;
        }
        else
        {
            if (inactiveSprite != null) spriteRenderer.sprite = inactiveSprite;
            spriteRenderer.color = inactiveColor;
        }
        // УДАЛЕНА СТРОКА С Shader.Find, которая крашила сборку и вызывала утечку памяти
    }

    void SaveCheckpoint()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        PlayerPrefs.SetString($"LastCheckpointID_{currentScene}", checkpointID);
        PlayerPrefs.SetString($"LastCheckpointScene", currentScene);
        PlayerPrefs.SetFloat($"CheckpointX_{checkpointID}", transform.position.x);
        PlayerPrefs.SetFloat($"CheckpointY_{checkpointID}", transform.position.y);
        PlayerPrefs.Save();

        Debug.Log($"[Checkpoint] Сохранён: ID={checkpointID}, Scene={currentScene}, Pos={transform.position}");
    }

    void SetPlayerRespawnPoint()
    {
        // ИСПОЛЬЗУЕТСЯ НОВАЯ КОМАНДА ДЛЯ UNITY 6
        PlayerRespawn playerRespawn = FindAnyObjectByType<PlayerRespawn>();
        if (playerRespawn != null)
        {
            playerRespawn.SetRespawnPoint(transform.position);
        }
        else
        {
            Debug.LogWarning("[Checkpoint] PlayerRespawn не найден на сцене!");
        }
    }

    public static string GetSavedCheckpointID()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        return PlayerPrefs.GetString($"LastCheckpointID_{currentScene}", "");
    }

    public static string GetSavedCheckpointScene()
    {
        return PlayerPrefs.GetString($"LastCheckpointScene", "");
    }

    public static Vector2 GetSavedCheckpointPosition(string checkpointID)
    {
        float x = PlayerPrefs.GetFloat($"CheckpointX_{checkpointID}", 0f);
        float y = PlayerPrefs.GetFloat($"CheckpointY_{checkpointID}", 0f);
        return new Vector2(x, y);
    }

    public static void ClearSavedCheckpoint()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerPrefs.DeleteKey($"LastCheckpointID_{currentScene}");
        PlayerPrefs.DeleteKey($"LastCheckpointScene");
        PlayerPrefs.Save();
        Debug.Log("[Checkpoint] Сохранённый чекпоинт очищен");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = isActivated ? Color.green : new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>() ?
            GetComponent<BoxCollider2D>().bounds.size : Vector3.one);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isActivated ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, GetComponent<BoxCollider2D>() ?
            GetComponent<BoxCollider2D>().bounds.size : Vector3.one);

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.7f,
            isActivated ? $"ACTIVE\n{checkpointID.Substring(0, 8)}" : $"CHECKPOINT\n{checkpointID.Substring(0, 8)}"
        );
#endif
    }
}