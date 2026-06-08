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
    public GameObject reversedClosedDoorPrefab;

    public Transform doorsParent;

    private Dictionary<Vector3Int, TileBase> originalTiles
        = new Dictionary<Vector3Int, TileBase>();

    private Dictionary<Vector3Int, GameObject> currentDoorObjects
        = new Dictionary<Vector3Int, GameObject>();

    private Dictionary<Vector3Int, bool> doorIsOpen
        = new Dictionary<Vector3Int, bool>();

    // Хранит какие двери перевёрнуты
    private HashSet<Vector3Int> reversedDoors
        = new HashSet<Vector3Int>();

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
        HashSet<Vector3Int> lowerCells = new HashSet<Vector3Int>();

        foreach (Vector3Int pos in originalTiles.Keys)
        {
            Vector3Int below = pos + Vector3Int.down;
            if (!originalTiles.ContainsKey(below))
            {
                lowerCells.Add(pos);
            }
        }

        doorsTilemap.ClearAllTiles();

        // Собираем информацию об отражённых дверях от Door-компонентов
        Door[] allDoors = FindObjectsByType<Door>(FindObjectsSortMode.None);
        foreach (Door door in allDoors)
        {
            if (!door.isReversed) continue;

            foreach (DoorCell cell in door.doorCells)
            {
                Vector3Int lowerCell = GetLowerCell(cell.cellPosition);
                reversedDoors.Add(lowerCell);
            }
        }

        foreach (Vector3Int lowerCell in lowerCells)
        {
            bool isReversed = reversedDoors.Contains(lowerCell);
            SpawnClosedDoor(lowerCell, isReversed);
        }

        RefreshCollider();
        Debug.Log($"[DoorsManager] Создано префабов: {currentDoorObjects.Count}");
    }

    // -------------------------------------------------------
    // Вспомогательные методы
    // -------------------------------------------------------

    Vector3 GetDoorWorldPosition(Vector3Int lowerCell)
    {
        Vector3 worldLower = doorsTilemap.GetCellCenterWorld(lowerCell);
        Vector3 worldUpper = doorsTilemap.GetCellCenterWorld(lowerCell + Vector3Int.up);
        return (worldLower + worldUpper) / 2f;
    }

    void SpawnClosedDoor(Vector3Int lowerCell, bool isReversed)
    {
        GameObject prefabToUse = (isReversed && reversedClosedDoorPrefab != null)
            ? reversedClosedDoorPrefab
            : closedDoorPrefab;

        if (prefabToUse == null) return;

        GameObject doorObj = Instantiate(prefabToUse, doorsParent);
        doorObj.transform.position = GetDoorWorldPosition(lowerCell);

        currentDoorObjects[lowerCell] = doorObj;
        doorIsOpen[lowerCell] = false;
    }

    void SpawnOpenDoor(Vector3Int lowerCell)
    {
        if (openDoorPrefab == null) return;

        GameObject doorObj = Instantiate(openDoorPrefab, doorsParent);
        doorObj.transform.position = GetDoorWorldPosition(lowerCell);

        currentDoorObjects[lowerCell] = doorObj;
        doorIsOpen[lowerCell] = true;
    }

    void DestroyDoorObject(Vector3Int lowerCell)
    {
        if (currentDoorObjects.TryGetValue(lowerCell, out GameObject oldObj))
        {
            Destroy(oldObj);
            currentDoorObjects.Remove(lowerCell);
        }
    }

    // -------------------------------------------------------
    // Публичные методы
    // -------------------------------------------------------

    public void OpenCells(List<DoorCell> cells)
    {
        HashSet<Vector3Int> processed = new HashSet<Vector3Int>();

        foreach (DoorCell cell in cells)
        {
            Vector3Int lowerCell = GetLowerCell(cell.cellPosition);
            if (processed.Contains(lowerCell)) continue;

            processed.Add(lowerCell);
            processed.Add(lowerCell + Vector3Int.up);

            DestroyDoorObject(lowerCell);
            SpawnOpenDoor(lowerCell);
        }
        RefreshCollider();
    }

    public void CloseCells(List<DoorCell> cells, bool isReversed = false)
    {
        HashSet<Vector3Int> processed = new HashSet<Vector3Int>();

        foreach (DoorCell cell in cells)
        {
            Vector3Int lowerCell = GetLowerCell(cell.cellPosition);
            if (processed.Contains(lowerCell)) continue;

            processed.Add(lowerCell);
            processed.Add(lowerCell + Vector3Int.up);

            // Обновляем словарь отражённых дверей
            if (isReversed)
                reversedDoors.Add(lowerCell);
            else
                reversedDoors.Remove(lowerCell);

            DestroyDoorObject(lowerCell);
            SpawnClosedDoor(lowerCell, isReversed);
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

        if (originalTiles.ContainsKey(below))
            return below;

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