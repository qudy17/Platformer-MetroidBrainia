// Замени ВЕСЬ BarriersManager.cs на этот:

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BarriersManager : MonoBehaviour
{
    public static BarriersManager Instance { get; private set; }

    [Header("Tilemap барьеров")]
    public Tilemap barriersTilemap;
    public TilemapCollider2D barriersCollider;

    // Хранит оригинальные тайлы
    private Dictionary<Vector3Int, TileBase> originalTiles
        = new Dictionary<Vector3Int, TileBase>();

    // Хранит текущее состояние: true = тайл есть (solid), false = тайла нет (phantom)
    private Dictionary<Vector3Int, bool> cellStates
        = new Dictionary<Vector3Int, bool>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (barriersTilemap == null)
        {
            barriersTilemap = GetComponent<Tilemap>();
            if (barriersTilemap == null)
                barriersTilemap = GetComponentInChildren<Tilemap>();
        }

        if (barriersCollider == null)
        {
            barriersCollider = GetComponent<TilemapCollider2D>();
            if (barriersCollider == null)
                barriersCollider = GetComponentInChildren<TilemapCollider2D>();
        }

        if (barriersTilemap != null)
            SaveOriginalTiles();
    }

    void SaveOriginalTiles()
    {
        originalTiles.Clear();
        cellStates.Clear();

        BoundsInt bounds = barriersTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = barriersTilemap.GetTile(pos);
            if (tile != null)
            {
                originalTiles[pos] = tile;
                cellStates[pos] = true; // Все тайлы изначально solid
            }
        }

        Debug.Log($"[BarriersManager] Сохранено тайлов: {originalTiles.Count}");
    }

    // Установить ячейки как SOLID (вернуть тайлы)
    public void SetCellsSolid(List<BarrierCell> cells)
    {
        foreach (BarrierCell cell in cells)
        {
            if (originalTiles.TryGetValue(cell.cellPosition, out TileBase tile))
            {
                // Проверяем нужно ли менять
                if (!cellStates.ContainsKey(cell.cellPosition) || !cellStates[cell.cellPosition])
                {
                    barriersTilemap.SetTile(cell.cellPosition, tile);
                    cellStates[cell.cellPosition] = true;
                }
            }
        }
        RefreshCollider();
    }

    // Установить ячейки как PHANTOM (убрать тайлы)
    public void SetCellsPhantom(List<BarrierCell> cells)
    {
        foreach (BarrierCell cell in cells)
        {
            if (originalTiles.ContainsKey(cell.cellPosition))
            {
                if (!cellStates.ContainsKey(cell.cellPosition) || cellStates[cell.cellPosition])
                {
                    barriersTilemap.SetTile(cell.cellPosition, null);
                    cellStates[cell.cellPosition] = false;
                }
            }
        }
        RefreshCollider();
    }

    // Проверить solid ли ячейка
    public bool IsCellSolid(Vector3Int cellPos)
    {
        if (cellStates.TryGetValue(cellPos, out bool state))
            return state;
        return barriersTilemap.GetTile(cellPos) != null;
    }

    void RefreshCollider()
    {
        if (barriersCollider != null)
        {
            barriersCollider.enabled = false;
            barriersCollider.enabled = true;
        }
    }
}