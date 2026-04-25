using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
public class FragileBlock : MonoBehaviour
{
    [Header("Эффекты разрушения")]
    [Tooltip("Префаб эффекта разрушения (опционально)")]
    public GameObject destroyEffectPrefab;

    [Header("Отдача игроку")]
    [Tooltip("Радиус, в котором игрок получает отдачу при разрушении")]
    public float recoilRadius = 3f;

    [Tooltip("Сила отдачи игроку")]
    public float recoilForce = 10f;

    [Header("Слои")]
    public LayerMask playerLayer;

    private BoxCollider2D solidCollider;
    private BoxCollider2D triggerCollider;
    private SpriteRenderer spriteRenderer;
    private Tilemap parentTilemap;
    private Vector3Int cellPosition;
    private bool isDestroyed = false;

    void Awake()
    {
        solidCollider = GetComponent<BoxCollider2D>();
        solidCollider.isTrigger = false;

        triggerCollider = gameObject.AddComponent<BoxCollider2D>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = solidCollider.size * 1.1f;
        triggerCollider.offset = solidCollider.offset;

        spriteRenderer = GetComponent<SpriteRenderer>();

        parentTilemap = GetComponentInParent<Tilemap>();
        if (parentTilemap != null)
        {
            cellPosition = parentTilemap.WorldToCell(transform.position);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        SoundWave wave = other.GetComponent<SoundWave>();
        if (wave != null)
        {
            Debug.Log($"[FragileBlock] {gameObject.name}: Волна попала!");

            // ВАЖНО: Ставим флаг ДО вызова методов волны
            isDestroyed = true;

            // ВАЖНО: СНАЧАЛА сообщаем волне (ставит её флаг hasCollided)
            wave.HitFragileBlock();

            // ПОТОМ отдача и разрушение
            ApplyRecoilToNearbyPlayer();
            DestroyBlock();
        }
    }

    void ApplyRecoilToNearbyPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(
            transform.position,
            recoilRadius,
            playerLayer
        );

        if (playerCollider != null)
        {
            Rigidbody2D playerRb = playerCollider.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 recoilDirection = (playerRb.transform.position - transform.position).normalized;

                if (recoilDirection.y > 0.3f)
                {
                    playerRb.linearVelocity = new Vector2(
                        playerRb.linearVelocity.x,
                        0f
                    );
                }

                playerRb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);
            }
        }
    }

    public void DestroyBlock()
    {
        Debug.Log($"[FragileBlock] {gameObject.name}: Разрушаюсь!");

        // МГНОВЕННО отключаем ВСЁ
        if (solidCollider != null) solidCollider.enabled = false;
        if (triggerCollider != null) triggerCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // Эффект
        if (destroyEffectPrefab != null)
        {
            Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
        }

        // Убираем тайл из Tilemap
        if (parentTilemap != null)
        {
            parentTilemap.SetTile(cellPosition, null);
        }

        // Удаляем объект
        Destroy(gameObject, 0.05f);
    }

    void OnDrawGizmosSelected()
    {
        if (isDestroyed) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, recoilRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, spriteRenderer ?
            spriteRenderer.bounds.size : Vector3.one);

        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            "FRAGILE"
        );
    }
}