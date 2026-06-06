using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MovableBlockSpawner : MonoBehaviour
{
    [Header("Ссылки")]
    public Tilemap markerTilemap;
    public GameObject movableBlockPrefab;
    public TileBase markerTile;

    [Header("Выравнивание")]
    public Vector2 spawnOffset = Vector2.zero;

    private List<Vector3Int> originalMarkerPositions = new List<Vector3Int>();

    // ВАЖНО: ИЗМЕНЕНО НА Start
    void Start()
    {
        if (markerTilemap == null || movableBlockPrefab == null) return;

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
            if (isMarkerTile) originalMarkerPositions.Add(cellPos);
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
                    current + Vector3Int.right, current + Vector3Int.left,
                    current + Vector3Int.up, current + Vector3Int.down
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

        foreach (var group in groups) SpawnBlockGroup(group);

        foreach (var pos in processedCells) markerTilemap.SetTile(pos, null);
    }

    void SpawnBlockGroup(List<Vector3Int> cells)
    {
        if (cells.Count == 0) return;

        GameObject parent = new GameObject($"BlockGroup_{cells[0].x}_{cells[0].y}");
        parent.layer = movableBlockPrefab.layer;

        Vector3 centerWorld = Vector3.zero;
        foreach (var cell in cells) centerWorld += markerTilemap.GetCellCenterWorld(cell);

        centerWorld /= cells.Count;
        centerWorld += (Vector3)spawnOffset;
        parent.transform.position = centerWorld;

        MovableBlock prefabBlock = movableBlockPrefab.GetComponent<MovableBlock>();
        float prefabMass = prefabBlock != null ? prefabBlock.blockMass : 2f;

        foreach (var cell in cells)
        {
            Vector3 cellWorldPos = markerTilemap.GetCellCenterWorld(cell) + (Vector3)spawnOffset;
            Vector3 localPos = cellWorldPos - parent.transform.position;

            GameObject child = Instantiate(movableBlockPrefab, parent.transform);
            child.transform.localPosition = localPos;
            child.transform.localRotation = Quaternion.identity;
            child.name = $"Block_{cell.x}_{cell.y}";

            MovableBlock childMovable = child.GetComponent<MovableBlock>();
            if (childMovable != null)
            {
                childMovable.enabled = false;
                Destroy(childMovable);
            }

            Rigidbody2D childRb = child.GetComponent<Rigidbody2D>();
            if (childRb != null)
            {
                childRb.simulated = false; // Мгновенно отключаем физику!
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
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.mass = prefabMass * cells.Count;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;

        CompositeCollider2D composite = parent.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        composite.offsetDistance = 0.0001f;

        MovableBlock movableBlock = parent.AddComponent<MovableBlock>();

        if (prefabBlock != null)
        {
            movableBlock.friction = prefabBlock.friction;
            movableBlock.maxSpeed = prefabBlock.maxSpeed;
            movableBlock.blockMass = rb.mass;
            movableBlock.waveForceMultiplier = prefabBlock.waveForceMultiplier * cells.Count;
            movableBlock.groundLayer = prefabBlock.groundLayer;
            movableBlock.playerLayer = prefabBlock.playerLayer;
            movableBlock.movableBlockLayer = prefabBlock.movableBlockLayer;
        }
    }
}