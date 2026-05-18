using UnityEngine;
using UnityEngine.Tilemaps;
// тест русского языка
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

    [Header("Слои")]
    public LayerMask playerLayer;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;
    private static Checkpoint currentCheckpoint;
    private Tilemap parentTilemap;
    private Vector3Int cellPosition;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Проверяем, является ли объект частью Tilemap
        parentTilemap = GetComponentInParent<Tilemap>();
        if (parentTilemap != null)
        {
            cellPosition = parentTilemap.WorldToCell(transform.position);
            parentTilemap.SetTile(cellPosition, null);
        }

        SetVisualState(false);
        Debug.Log($"[Checkpoint] Инициализирован в позиции {transform.position}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isActivated) return;

        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            ActivateCheckpoint();
        }
    }

    void ActivateCheckpoint()
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
        SaveRespawnPoint();
    }

    public void DeactivateCheckpoint()
    {
        isActivated = false;
        SetVisualState(false);
    }

    void SetVisualState(bool active)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
        }

        if (active)
        {
            if (activeSprite != null)
                spriteRenderer.sprite = activeSprite;

            spriteRenderer.color = activeColor;
        }
        else
        {
            if (inactiveSprite != null)
                spriteRenderer.sprite = inactiveSprite;

            spriteRenderer.color = inactiveColor;
        }

        spriteRenderer.material = new Material(Shader.Find("Sprites/Default"));
    }

    void SaveRespawnPoint()
    {
        PlayerRespawn playerRespawn = FindFirstObjectByType<PlayerRespawn>();
        if (playerRespawn != null)
        {
            playerRespawn.SetRespawnPoint(transform.position);
        }
        else
        {
            Debug.LogWarning("[Checkpoint] PlayerRespawn не найден на сцене!");
        }
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

        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.7f,
            isActivated ? "ACTIVE CHECKPOINT" : "CHECKPOINT"
        );
    }
}