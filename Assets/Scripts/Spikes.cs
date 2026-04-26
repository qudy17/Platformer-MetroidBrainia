using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("Настройки урона")]
    [Tooltip("Задержка перед респавном после смерти")]
    public float deathDelay = 0.1f;

    [Header("Слои")]
    public LayerMask playerLayer;
    public LayerMask movableBlockLayer; // Чтобы шипы не убивали блоки

    private BoxCollider2D boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        // ВАЖНО: Коллайдер НЕ триггер
        boxCollider.isTrigger = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверяем, что столкнулись с игроком
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            KillPlayer(collision.gameObject);
        }
        // Подвижные блоки просто касаются и остаются на шипах
    }

    void KillPlayer(GameObject player)
    {
        Debug.Log($"[Spikes] Игрок коснулся шипов в позиции {transform.position}");

        PlayerRespawn playerRespawn = player.GetComponent<PlayerRespawn>();
        if (playerRespawn != null)
        {
            StartCoroutine(DeathSequence(playerRespawn));
        }
        else
        {
            Debug.LogWarning("[Spikes] PlayerRespawn не найден на игроке!");
        }
    }

    System.Collections.IEnumerator DeathSequence(PlayerRespawn playerRespawn)
    {
        yield return new WaitForSeconds(deathDelay);
        playerRespawn.Respawn();
    }

    void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawCube(
                transform.position + (Vector3)boxCollider.offset,
                boxCollider.size
            );
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, boxCollider ?
            boxCollider.bounds.size : Vector3.one);

        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.7f,
            "SPIKES"
        );
    }
}