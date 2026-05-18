using UnityEngine;
//окококок пока
// тест русского языка
// привет
public class Spikes : MonoBehaviour
{
    [Header("Настройки урона")]
    [Tooltip("Задержка перед респавном после смерти")]
    public float deathDelay = 0.1f;

    [Header("Слои")]
    public LayerMask playerLayer;
    public LayerMask enemyLayer; // ДОБАВЛЕНО

    private BoxCollider2D boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        int otherLayer = 1 << collision.gameObject.layer;

        // Игрок умирает от шипов
        if ((otherLayer & playerLayer) != 0)
        {
            KillPlayer(collision.gameObject);
            GameStatsTracker.RegisterDeath();
        }

        // Враг умирает от шипов
        if ((otherLayer & enemyLayer) != 0)
        {
            KillEnemy(collision.gameObject);
        }
    }

    void KillPlayer(GameObject player)
    {
        Debug.Log($"[Spikes] Игрок коснулся шипов!");

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

    void KillEnemy(GameObject enemyObject)
    {
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.Die();
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