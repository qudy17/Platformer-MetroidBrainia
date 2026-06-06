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

    // ВАЖНО: ИЗМЕНЕНО НА Start
    void Start()
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
    }

    public void RespawnBlocks()
    {
        RestoreMarkers();
        SpawnBlocksAsGroups();
    }

    void RestoreMarkers()
    {
        if (markerTile == null) return;

        foreach (Vector3Int pos in originalMarkerPositions)
        {
            markerTilemap.SetTile(pos, markerTile);
        }
    }

    void SpawnBlocksAsGroups()
    {
        BoundsInt bounds = markerTilemap.cellBounds;
        HashSet<Vector3Int> processedCells = new HashSet<Vector3Int>();
        List<List<Vector3Int>> groups = new List<List<Vector3Int>>();

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

        foreach (var group in groups)
        {
            SpawnBlockGroup(group);
        }

        foreach (var pos in processedCells)
        {
            markerTilemap.SetTile(pos, null);
        }
    }

    void SpawnBlockGroup(List<Vector3Int> cells)
    {
        if (cells.Count == 0) return;

        if (cells.Count == 1)
        {
            Vector3 cellWorldPos = markerTilemap.GetCellCenterWorld(cells[0]) + (Vector3)spawnOffset;
            GameObject single = Instantiate(fragileBlockPrefab, cellWorldPos, Quaternion.identity);
            single.name = $"FragileBlock_{cells[0].x}_{cells[0].y}";
            return;
        }

        GameObject parent = new GameObject($"FragileGroup_{cells[0].x}_{cells[0].y}");
        parent.layer = fragileBlockPrefab.layer;

        Vector3 centerWorld = Vector3.zero;
        foreach (var cell in cells)
        {
            centerWorld += markerTilemap.GetCellCenterWorld(cell);
        }
        centerWorld /= cells.Count;
        centerWorld += (Vector3)spawnOffset;
        parent.transform.position = centerWorld;

        FragileBlock prefabBlock = fragileBlockPrefab.GetComponent<FragileBlock>();

        foreach (var cell in cells)
        {
            Vector3 cellWorldPos = markerTilemap.GetCellCenterWorld(cell) + (Vector3)spawnOffset;
            Vector3 localPos = cellWorldPos - parent.transform.position;

            GameObject child = Instantiate(fragileBlockPrefab, parent.transform);
            child.transform.localPosition = localPos;
            child.transform.localRotation = Quaternion.identity;
            child.name = $"Block_{cell.x}_{cell.y}";

            FragileBlock childFragile = child.GetComponent<FragileBlock>();
            if (childFragile != null)
            {
                childFragile.enabled = false;
                Destroy(childFragile);
            }

            Rigidbody2D childRb = child.GetComponent<Rigidbody2D>();
            if (childRb != null)
            {
                childRb.simulated = false; // Мгновенно отключаем физику для WebGL
                Destroy(childRb);
            }

            BoxCollider2D childCollider = child.GetComponent<BoxCollider2D>();
            if (childCollider != null)
            {
                childCollider.compositeOperation = Collider2D.CompositeOperation.Merge; // Убрали варнинг
                childCollider.isTrigger = false;
            }
        }

        Rigidbody2D rb = parent.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        CompositeCollider2D composite = parent.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.offsetDistance = 0.0001f;

        FragileBlock fragileBlock = parent.AddComponent<FragileBlock>();

        if (prefabBlock != null)
        {
            fragileBlock.destroyEffectPrefab = prefabBlock.destroyEffectPrefab;
            fragileBlock.recoilRadius = prefabBlock.recoilRadius;
            fragileBlock.recoilForce = prefabBlock.recoilForce;
            fragileBlock.playerLayer = prefabBlock.playerLayer;
        }
    }
}