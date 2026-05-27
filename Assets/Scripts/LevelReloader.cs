using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelReloader : MonoBehaviour
{
    private static LevelReloader instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public static void ReloadLevel()
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ReloadLevelCoroutine());
        }
        else
        {
            Debug.LogError("[LevelReloader] Instance не найден!");
        }
    }

    IEnumerator ReloadLevelCoroutine()
    {
        Debug.Log("[LevelReloader] Начинается перезагрузка уровня...");

        // 1. Возвращаем игрока на чекпоинт
        TeleportPlayerToCheckpoint();

        yield return new WaitForFixedUpdate();

        // 2. Удаляем все сущности
        DestroyAllEntities();

        yield return new WaitForFixedUpdate();

        // 3. Перезапускаем спавнеры
        RespawnAllEntities();

        yield return new WaitForFixedUpdate();

        // 4. Обновляем камеру
        CameraFollow camera = FindFirstObjectByType<CameraFollow>();
        if (camera != null)
        {
            camera.ForceSetToCurrentRoom();
        }

        Debug.Log("[LevelReloader] Перезагрузка завершена!");
    }

    void TeleportPlayerToCheckpoint()
    {
        PlayerRespawn playerRespawn = FindFirstObjectByType<PlayerRespawn>();
        if (playerRespawn != null)
        {
            Vector2 respawnPoint = GetCurrentRespawnPoint();
            playerRespawn.transform.position = respawnPoint;

            Rigidbody2D playerRb = playerRespawn.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }

            Debug.Log($"[LevelReloader] Игрок телепортирован на {respawnPoint}");
        }
    }

    Vector2 GetCurrentRespawnPoint()
    {
        string savedCheckpointID = Checkpoint.GetSavedCheckpointID();
        string savedScene = Checkpoint.GetSavedCheckpointScene();
        string currentScene = SceneManager.GetActiveScene().name;

        if (!string.IsNullOrEmpty(savedCheckpointID) && savedScene == currentScene)
        {
            Vector2 savedPosition = Checkpoint.GetSavedCheckpointPosition(savedCheckpointID);
            if (savedPosition != Vector2.zero)
            {
                return savedPosition;
            }
        }

        PlayerRespawn respawn = FindFirstObjectByType<PlayerRespawn>();
        if (respawn != null)
        {
            return respawn.defaultSpawnPoint;
        }

        return Vector2.zero;
    }

    void DestroyAllEntities()
    {
        // Удаляем врагов
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
        Debug.Log($"[LevelReloader] Удалено врагов: {enemies.Length}");

        // Удаляем подвижные блоки
        MovableBlock[] movableBlocks = FindObjectsByType<MovableBlock>(FindObjectsSortMode.None);
        foreach (MovableBlock block in movableBlocks)
        {
            Destroy(block.gameObject);
        }
        Debug.Log($"[LevelReloader] Удалено подвижных блоков: {movableBlocks.Length}");

        // Удаляем хрупкие блоки
        FragileBlock[] fragileBlocks = FindObjectsByType<FragileBlock>(FindObjectsSortMode.None);
        foreach (FragileBlock block in fragileBlocks)
        {
            Destroy(block.gameObject);
        }
        Debug.Log($"[LevelReloader] Удалено хрупких блоков: {fragileBlocks.Length}");
    }

    void RespawnAllEntities()
    {
        // Перезапускаем спавнер врагов
        EnemySpawner enemySpawner = FindFirstObjectByType<EnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner.RespawnEnemies();
        }

        // Перезапускаем спавнер подвижных блоков
        MovableBlockSpawner movableSpawner = FindFirstObjectByType<MovableBlockSpawner>();
        if (movableSpawner != null)
        {
            movableSpawner.RespawnBlocks();
        }

        // Перезапускаем спавнер хрупких блоков
        FragileBlockSpawner fragileSpawner = FindFirstObjectByType<FragileBlockSpawner>();
        if (fragileSpawner != null)
        {
            fragileSpawner.RespawnBlocks();
        }
    }
}