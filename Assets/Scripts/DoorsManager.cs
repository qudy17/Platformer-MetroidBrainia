using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
// тест русского языка
public class DoorsManager : MonoBehaviour
{
    public static DoorsManager Instance { get; private set; }

    [Header("Общий Tilemap для всех дверей")]
    public Tilemap doorsTilemap;
    public TilemapCollider2D doorsCollider;

    // Хранит оригинальные тайлы каждой ячейки
    // Нужно чтобы восстановить дверь когда она закрывается
    private Dictionary<Vector3Int, TileBase> originalTiles
        = new Dictionary<Vector3Int, TileBase>();

    void Awake()
    {
        // Синглтон
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Запоминаем все тайлы которые есть в Tilemap
        // Это и есть все "закрытые" двери
        SaveOriginalTiles();
    }

    // Сохраняем оригинальные тайлы при старте
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

        Debug.Log($"[DoorsManager] Сохранено тайлов дверей: {originalTiles.Count}");
    }

    // ───────────────────────────────────────────
    //  Публичные методы для Door.cs
    // ───────────────────────────────────────────

    // Открыть конкретные ячейки (убрать тайлы)
    public void OpenCells(List<DoorCell> cells)
    {
        foreach (DoorCell cell in cells)
        {
            doorsTilemap.SetTile(cell.cellPosition, null);
        }

        // Обновляем коллайдер
        RefreshCollider();
    }

    // Закрыть конкретные ячейки (вернуть тайлы)
    public void CloseCells(List<DoorCell> cells)
    {
        foreach (DoorCell cell in cells)
        {
            if (originalTiles.TryGetValue(cell.cellPosition, out TileBase tile))
            {
                doorsTilemap.SetTile(cell.cellPosition, tile);
            }
            else
            {
                Debug.LogWarning($"[DoorsManager] Не найден оригинальный тайл " +
                                 $"для позиции {cell.cellPosition}");
            }
        }

        RefreshCollider();
    }

    // Проверить открыта ли ячейка
    public bool IsCellOpen(Vector3Int cellPos)
    {
        return doorsTilemap.GetTile(cellPos) == null;
    }

    void RefreshCollider()
    {
        if (doorsCollider != null)
        {
            // Обновляем физический коллайдер
            doorsCollider.enabled = false;
            doorsCollider.enabled = true;
        }
    }
}