using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FragileBlockSpawner : MonoBehaviour
{
    [Header("Ссылки")]
    public Tilemap markerTilemap;
    public GameObject fragileBlockPrefab;
    public TileBase markerTile;

    [Header("Выравнивание")]
    public Vector2 spawnOffset = Vector2.zero;

    private List<Vector3Int> originalMarkerPositions = new List<Vector3Int>();

    void Awake()
    {
        if (markerTilemap == null)
        {
            Debug.LogError("[FragileBlockSpawner] markerTilemap не назначен!");
            return;
        }

        if (fragileBlockPrefab == null)
        {
            Debug.LogError("[FragileBlockSpawner] fragileBlockPrefab не назначен!");
            return;
        }

        SaveOriginalMarkers();
        SpawnBlocksAsGroups();
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

        Debug.Log($"[FragileBlockSpawner] Сохранено маркеров: {originalMarkerPositions.Count}");
    }

    public void RespawnBlocks()
    {
        Debug.Log("[FragileBlockSpawner] Начинается респавн блоков...");
        RestoreMarkers();
        SpawnBlocksAsGroups();
        Debug.Log("[FragileBlockSpawner] Респавн блоков завершен");
    }

    void RestoreMarkers()
    {
        if (markerTile == null)
        {
            Debug.LogWarning("[FragileBlockSpawner] markerTile не задан, пропускаю восстановление");
            return;
        }

        foreach (Vector3Int pos in originalMarkerPositions)
        {
            markerTilemap.SetTile(pos, markerTile);
        }

        Debug.Log($"[FragileBlockSpawner] Восстановлено маркеров: {originalMarkerPositions.Count}");
    }

    void SpawnBlocksAsGroups()
    {
        BoundsInt bounds = markerTilemap.cellBounds;
        HashSet<Vector3Int> processedCells = new HashSet<Vector3Int>();
        List<List<Vector3Int>> groups = new List<List<Vector3Int>>();

        // Находим все связанные группы тайлов
        foreach (Vector3Int cellPos in bounds.allPositionsWithin)
        {
            if (processedCells.Contains(cellPos)) continue;

            TileBase tile = markerTilemap.GetTile(cellPos);
            bool isMarkerTile = (markerTile == null) ? tile != null : tile == markerTile;
            if (!isMarkerTile) continue;

            List<Vector3Int> group = new List<Vector3Int>();
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(cellPos);
            processedCells.Add(cellPos);

            // BFS для поиска всех соседних тайлов
            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();
                group.Add(current);

                Vector3Int[] neighbors = {
                    current + Vector3Int.right,
                    current + Vector3Int.left,
                    current + Vector3Int.up,
                    current + Vector3Int.down
                };

                foreach (Vector3Int neighbor in neighbors)
                {
                    if (processedCells.Contains(neighbor)) continue;

                    TileBase neighborTile = markerTilemap.GetTile(neighbor);
                    bool isNeighborMarker = (markerTile == null) ? neighborTile != null : neighborTile == markerTile;

                    if (isNeighborMarker)
                    {
                        processedCells.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            groups.Add(group);
        }

        // Создаём группы блоков
        foreach (var group in groups)
        {
            SpawnBlockGroup(group);
        }

        // Очищаем маркеры
        foreach (var pos in processedCells)
        {
            markerTilemap.SetTile(pos, null);
        }

        Debug.Log($"[FragileBlockSpawner] Создано групп хрупких блоков: {groups.Count}");
    }

    void SpawnBlockGroup(List<Vector3Int> cells)
    {
        if (cells.Count == 0) return;

        // Если только один тайл - создаём одиночный блок
        if (cells.Count == 1)
        {
            Vector3 cellWorldPos = markerTilemap.GetCellCenterWorld(cells[0]) + (Vector3)spawnOffset;
            GameObject single = Instantiate(fragileBlockPrefab, cellWorldPos, Quaternion.identity);
            single.name = $"FragileBlock_{cells[0].x}_{cells[0].y}";
            return;
        }

        // Создаём родительский объект для группы
        GameObject parent = new GameObject($"FragileGroup_{cells[0].x}_{cells[0].y}");
        parent.layer = fragileBlockPrefab.layer;

        // Вычисляем центр группы
        Vector3 centerWorld = Vector3.zero;
        foreach (var cell in cells)
        {
            centerWorld += markerTilemap.GetCellCenterWorld(cell);
        }
        centerWorld /= cells.Count;
        centerWorld += (Vector3)spawnOffset;
        parent.transform.position = centerWorld;

        // Получаем настройки из префаба
        FragileBlock prefabBlock = fragileBlockPrefab.GetComponent<FragileBlock>();

        // Создаём дочерние объекты
        foreach (var cell in cells)
        {
            Vector3 cellWorldPos = markerTilemap.GetCellCenterWorld(cell) + (Vector3)spawnOffset;
            Vector3 localPos = cellWorldPos - parent.transform.position;

            GameObject child = Instantiate(fragileBlockPrefab, parent.transform);
            child.transform.localPosition = localPos;
            child.transform.localRotation = Quaternion.identity;
            child.name = $"Block_{cell.x}_{cell.y}";

            // ВАЖНО: сначала удаляем FragileBlock
            FragileBlock childFragile = child.GetComponent<FragileBlock>();
            if (childFragile != null)
            {
                DestroyImmediate(childFragile);
            }

            // Настраиваем коллайдер для композита
            BoxCollider2D childCollider = child.GetComponent<BoxCollider2D>();
            if (childCollider != null)
            {
                childCollider.usedByComposite = true;
                childCollider.isTrigger = false;
            }
        }

        // Добавляем Rigidbody2D на родителя (статический, для Composite)
        Rigidbody2D rb = parent.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // Добавляем CompositeCollider2D
        CompositeCollider2D composite = parent.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.offsetDistance = 0.0001f;

        // Добавляем FragileBlock на родителя
        FragileBlock fragileBlock = parent.AddComponent<FragileBlock>();

        // Копируем настройки из префаба
        if (prefabBlock != null)
        {
            fragileBlock.destroyEffectPrefab = prefabBlock.destroyEffectPrefab;
            fragileBlock.recoilRadius = prefabBlock.recoilRadius;
            fragileBlock.recoilForce = prefabBlock.recoilForce;
            fragileBlock.playerLayer = prefabBlock.playerLayer;
        }

        Debug.Log($"[FragileBlockSpawner] Создана группа из {cells.Count} блоков: {parent.name}");
    }
}