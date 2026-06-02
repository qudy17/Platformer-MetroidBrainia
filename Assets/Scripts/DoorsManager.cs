using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DoorsManager : MonoBehaviour
{
    public static DoorsManager Instance { get; private set; }

    [Header("Общий Tilemap для коллизий дверей")]
    public Tilemap doorsTilemap;
    public TilemapCollider2D doorsCollider;

    [Header("Префабы")]
    public GameObject closedDoorPrefab;
    public GameObject openDoorPrefab;

    public Transform doorsParent;

    private Dictionary<Vector3Int, TileBase> originalTiles
        = new Dictionary<Vector3Int, TileBase>();

    private Dictionary<Vector3Int, GameObject> currentDoorObjects
        = new Dictionary<Vector3Int, GameObject>();

    private Dictionary<Vector3Int, bool> doorIsOpen
        = new Dictionary<Vector3Int, bool>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (doorsParent == null)
        {
            doorsParent = new GameObject("Doors").transform;
            doorsParent.SetParent(transform);
        }

        SaveOriginalTiles();
        ReplaceAllTilesWithPrefabs();
    }

    void SaveOriginalTiles()
    {
        originalTiles.Clear();
        BoundsInt bounds = doorsTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = doorsTilemap.GetTile(pos);
            if (tile != null)
            {
                originalTiles[pos] = tile;
            }
        }
        Debug.Log($"[DoorsManager] Сохранено тайлов: {originalTiles.Count}");
    }

    void ReplaceAllTilesWithPrefabs()
    {
        // Находим все нижние клетки
        HashSet<Vector3Int> lowerCells = new HashSet<Vector3Int>();

        foreach (Vector3Int pos in originalTiles.Keys)
        {
            Vector3Int below = pos + Vector3Int.down;
            if (!originalTiles.ContainsKey(below))
            {
                lowerCells.Add(pos);
            }
        }

        // Удаляем ВСЕ тайлы из Tilemap
        doorsTilemap.ClearAllTiles();

        // Создаём префабы закрытых дверей
        foreach (Vector3Int lowerCell in lowerCells)
        {
            if (closedDoorPrefab != null)
            {
                GameObject doorObj = Instantiate(closedDoorPrefab, doorsParent);

                Vector3 worldLower = doorsTilemap.GetCellCenterWorld(lowerCell);
                Vector3 worldUpper = doorsTilemap.GetCellCenterWorld(lowerCell + Vector3Int.up);
                doorObj.transform.position = (worldLower + worldUpper) / 2f;

                currentDoorObjects[lowerCell] = doorObj;
                doorIsOpen[lowerCell] = false;
            }
        }

        RefreshCollider();
        Debug.Log($"[DoorsManager] Создано префабов: {currentDoorObjects.Count}");
    }

    public void OpenCells(List<DoorCell> cells)
    {
        HashSet<Vector3Int> processed = new HashSet<Vector3Int>();

        foreach (DoorCell cell in cells)
        {
            Vector3Int lowerCell = GetLowerCell(cell.cellPosition);
            if (processed.Contains(lowerCell)) continue;

            processed.Add(lowerCell);
            processed.Add(lowerCell + Vector3Int.up);

            // Удаляем старый префаб (закрытый)
            if (currentDoorObjects.TryGetValue(lowerCell, out GameObject oldObj))
            {
                Destroy(oldObj);
                currentDoorObjects.Remove(lowerCell);
            }

            // Создаём префаб открытой двери
            if (openDoorPrefab != null)
            {
                GameObject doorObj = Instantiate(openDoorPrefab, doorsParent);

                Vector3 worldLower = doorsTilemap.GetCellCenterWorld(lowerCell);
                Vector3 worldUpper = doorsTilemap.GetCellCenterWorld(lowerCell + Vector3Int.up);
                doorObj.transform.position = (worldLower + worldUpper) / 2f;

                currentDoorObjects[lowerCell] = doorObj;
            }

            doorIsOpen[lowerCell] = true;
        }
        RefreshCollider();
    }

    public void CloseCells(List<DoorCell> cells)
    {
        HashSet<Vector3Int> processed = new HashSet<Vector3Int>();

        foreach (DoorCell cell in cells)
        {
            Vector3Int lowerCell = GetLowerCell(cell.cellPosition);
            if (processed.Contains(lowerCell)) continue;

            processed.Add(lowerCell);
            processed.Add(lowerCell + Vector3Int.up);

            // Удаляем старый префаб (открытый)
            if (currentDoorObjects.TryGetValue(lowerCell, out GameObject oldObj))
            {
                Destroy(oldObj);
                currentDoorObjects.Remove(lowerCell);
            }

            // Создаём префаб закрытой двери
            if (closedDoorPrefab != null)
            {
                GameObject doorObj = Instantiate(closedDoorPrefab, doorsParent);

                Vector3 worldLower = doorsTilemap.GetCellCenterWorld(lowerCell);
                Vector3 worldUpper = doorsTilemap.GetCellCenterWorld(lowerCell + Vector3Int.up);
                doorObj.transform.position = (worldLower + worldUpper) / 2f;

                currentDoorObjects[lowerCell] = doorObj;
            }

            doorIsOpen[lowerCell] = false;
        }
        RefreshCollider();
    }

    public bool IsCellOpen(Vector3Int cellPos)
    {
        Vector3Int lowerCell = GetLowerCell(cellPos);
        return doorIsOpen.TryGetValue(lowerCell, out bool isOpen) && isOpen;
    }

    Vector3Int GetLowerCell(Vector3Int cellPos)
    {
        Vector3Int below = cellPos + Vector3Int.down;

        // Ищем нижнюю клетку по originalTiles
        if (originalTiles.ContainsKey(below))
            return below;

        // Или по currentDoorObjects
        if (currentDoorObjects.ContainsKey(below))
            return below;

        return cellPos;
    }

    void RefreshCollider()
    {
        if (doorsCollider != null)
        {
            doorsCollider.enabled = false;
            doorsCollider.enabled = true;
        }
    }
}