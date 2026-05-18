using UnityEngine;
using UnityEngine.Tilemaps;
// тест русского языка
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
    private bool isDestroyed = false;
    private bool isGroup = false;
    private CompositeCollider2D compositeCollider;

    void Awake()
    {
        solidCollider = GetComponent<BoxCollider2D>();
        compositeCollider = GetComponent<CompositeCollider2D>();

        // Проверяем, является ли это группой
        isGroup = compositeCollider != null;

        if (!isGroup)
        {
            // Одиночный блок - работаем как раньше
            solidCollider.isTrigger = false;

            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = solidCollider.size * 1.1f;
            triggerCollider.offset = solidCollider.offset;
        }
        else
        {
            // Группа - используем CompositeCollider
            solidCollider = null; // У родителя нет BoxCollider2D

            // Добавляем триггер-коллайдер для обнаружения волны
            triggerCollider = gameObject.AddComponent<BoxCollider2D>();
            triggerCollider.isTrigger = true;

            // Размер триггера равен размеру композита + немного больше
            Bounds bounds = compositeCollider.bounds;
            triggerCollider.size = bounds.size * 1.1f;
            triggerCollider.offset = transform.InverseTransformPoint(bounds.center);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDestroyed) return;

        SoundWave wave = other.GetComponent<SoundWave>();
        if (wave != null)
        {
            Debug.Log($"[FragileBlock] {gameObject.name}: Волна попала! isGroup={isGroup}");

            isDestroyed = true;
            wave.HitFragileBlock();

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
        Debug.Log($"[FragileBlock] {gameObject.name}: Разрушаюсь! isGroup={isGroup}");

        // МГНОВЕННО отключаем ВСЁ
        if (solidCollider != null) solidCollider.enabled = false;
        if (triggerCollider != null) triggerCollider.enabled = false;
        if (compositeCollider != null) compositeCollider.enabled = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // Если это группа - создаём эффекты для каждого дочернего блока
        if (isGroup)
        {
            // Получаем все дочерние спрайты
            SpriteRenderer[] childSprites = GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sprite in childSprites)
            {
                if (destroyEffectPrefab != null)
                {
                    Instantiate(destroyEffectPrefab, sprite.transform.position, Quaternion.identity);
                }

                // Отключаем спрайты
                sprite.enabled = false;
            }

            // Отключаем все коллайдеры детей
            BoxCollider2D[] childColliders = GetComponentsInChildren<BoxCollider2D>();
            foreach (BoxCollider2D col in childColliders)
            {
                col.enabled = false;
            }
        }
        else
        {
            // Одиночный блок - один эффект
            if (destroyEffectPrefab != null)
            {
                Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        // Удаляем объект (весь родительский, если группа)
        Destroy(gameObject, 0.05f);
    }

    void OnDrawGizmosSelected()
    {
        if (isDestroyed) return;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
        Gizmos.DrawSphere(transform.position, recoilRadius);

        if (isGroup && compositeCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(compositeCollider.bounds.center, compositeCollider.bounds.size);
        }
        else if (spriteRenderer != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, spriteRenderer.bounds.size);
        }

#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            isGroup ? $"FRAGILE GROUP\n{gameObject.name}" : $"FRAGILE\n{gameObject.name}"
        );
#endif
    }
}