using UnityEngine;
using UnityEngine.Tilemaps;
// тест русского языка
//1
public class EnemySpawner : MonoBehaviour
{
    [Header("������")]
    public Tilemap markerTilemap;
    public GameObject enemyPrefab;
    public TileBase markerTile;

    [Header("������������")]
    public Vector2 spawnOffset = Vector2.zero;

    void Awake()
    {
        if (markerTilemap == null)
        {
            Debug.LogError("[EnemySpawner] markerTilemap �� ��������!");
            return;
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("[EnemySpawner] enemyPrefab �� ��������!");
            return;
        }

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        BoundsInt bounds = markerTilemap.cellBounds;
        int spawnCount = 0;

        // �������� ��������� �� �������
        Enemy prefabEnemy = enemyPrefab.GetComponent<Enemy>();

        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            TileBase tile = markerTilemap.GetTile(cellPos);
            bool isMarkerTile = (markerTile == null) ? tile != null : tile == markerTile;

            if (!isMarkerTile) continue;

            // �������� ������� �������
            Vector3 worldPos = markerTilemap.GetCellCenterWorld(cellPos) + (Vector3)spawnOffset;

            // ������� �����
            GameObject enemy = Instantiate(enemyPrefab, worldPos, Quaternion.identity, transform);
            enemy.name = $"Enemy_{cellPos.x}_{cellPos.y}";

            // �������� ��������� �� �������
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

            // ������� ������
            markerTilemap.SetTile(cellPos, null);
            spawnCount++;
        }

        Debug.Log($"[EnemySpawner] ������� ������: {spawnCount}");
    }
}