using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Ссылки")]
    public Tilemap markerTilemap;
    public GameObject enemyPrefab;
    public TileBase markerTile;

    [Header("Выравнивание")]
    public Vector2 spawnOffset = Vector2.zero;

    // Сохраняем исходные позиции маркеров
    private List<Vector3Int> originalMarkerPositions = new List<Vector3Int>();
    private bool hasSpawned = false;

    void Awake()
    {
        if (markerTilemap == null)
        {
            Debug.LogError("[EnemySpawner] markerTilemap не назначен!");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab не назначен!");
            return;
        }

        // Сохраняем исходные позиции маркеров
        SaveOriginalMarkers();

        // Первый спавн
        SpawnEnemies();
        hasSpawned = true;
    }

    void SaveOriginalMarkers()
    {
        originalMarkerPositions.Clear();
        BoundsInt bounds = markerTilemap.cellBounds;

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = markerTilemap.GetTile(cellPos);
            bool isMarkerTile = (markerTile == null) ? tile != null : tile == markerTile;

            if (isMarkerTile)
            {
                originalMarkerPositions.Add(cellPos);
            }
        }

        Debug.Log($"[EnemySpawner] Сохранено маркеров: {originalMarkerPositions.Count}");
    }

    public void RespawnEnemies()
    {
        Debug.Log("[EnemySpawner] Начинается респавн врагов...");

        // Восстанавливаем маркеры в Tilemap
        RestoreMarkers();

        // Спавним врагов
        SpawnEnemies();

        Debug.Log("[EnemySpawner] Респавн врагов завершен");
    }

    void RestoreMarkers()
    {
        if (markerTile == null)
        {
            Debug.LogWarning("[EnemySpawner] markerTile не задан, пропускаю восстановление");
            return;
        }

        foreach (Vector3Int pos in originalMarkerPositions)
        {
            markerTilemap.SetTile(pos, markerTile);
        }

        Debug.Log($"[EnemySpawner] Восстановлено маркеров: {originalMarkerPositions.Count}");
    }

    void SpawnEnemies()
    {
        BoundsInt bounds = markerTilemap.cellBounds;
        int spawnCount = 0;

        Enemy prefabEnemy = enemyPrefab.GetComponent<Enemy>();

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = markerTilemap.GetTile(cellPos);
            bool isMarkerTile = (markerTile == null) ? tile != null : tile == markerTile;

            if (!isMarkerTile) continue;

            Vector3 worldPos = markerTilemap.GetCellCenterWorld(cellPos) + (Vector3)spawnOffset;

            GameObject enemy = Instantiate(enemyPrefab, worldPos, Quaternion.identity, transform);
            enemy.name = $"Enemy_{cellPos.x}_{cellPos.y}";

            if (prefabEnemy != null)
            {
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null)
                {
                    enemyComponent.friction = prefabEnemy.friction;
                    enemyComponent.maxSpeed = prefabEnemy.maxSpeed;
                    enemyComponent.enemyMass = prefabEnemy.enemyMass;
                    enemyComponent.waveForceMultiplier = prefabEnemy.waveForceMultiplier;
                    enemyComponent.groundLayer = prefabEnemy.groundLayer;
                    enemyComponent.playerLayer = prefabEnemy.playerLayer;
                    enemyComponent.movableBlockLayer = prefabEnemy.movableBlockLayer;
                    enemyComponent.spikesLayer = prefabEnemy.spikesLayer;
                    enemyComponent.groundCheckDistance = prefabEnemy.groundCheckDistance;
                    enemyComponent.deathEffectPrefab = prefabEnemy.deathEffectPrefab;
                }
            }

            markerTilemap.SetTile(cellPos, null);
            spawnCount++;
        }

        Debug.Log($"[EnemySpawner] Создано врагов: {spawnCount}");
    }
}