using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
// тест русского языка
public class BarriersManager : MonoBehaviour
{
    public static BarriersManager Instance { get; private set; }

    [Header("Tilemap барьеров")]
    public Tilemap barriersTilemap;
    public TilemapCollider2D barriersCollider;

    [Header("Настройки прозрачности")]
    [Range(0f, 1f)]
    public float phantomAlpha = 0.3f;

    // Хранит оригинальные тайлы
    private Dictionary<Vector3Int, TileBase> originalTiles
        = new Dictionary<Vector3Int, TileBase>();

    // Хранит текущее состояние: true = тайл solid, false = тайл phantom
    private Dictionary<Vector3Int, bool> cellStates
        = new Dictionary<Vector3Int, bool>();

    // Отдельный tilemap для отображения прозрачных тайлов
    private Tilemap phantomTilemap;
    private TilemapRenderer phantomRenderer;

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

        // Создаем дочерний объект для прозрачных тайлов
        CreatePhantomTilemap();

        if (barriersTilemap != null)
            SaveOriginalTiles();
    }

    void CreatePhantomTilemap()
    {
        // Проверяем, не существует ли уже такой объект
        Transform existingPhantom = transform.Find("PhantomTilemap");
        GameObject phantomObj;

        if (existingPhantom != null)
        {
            phantomObj = existingPhantom.gameObject;
            phantomTilemap = phantomObj.GetComponent<Tilemap>();
            phantomRenderer = phantomObj.GetComponent<TilemapRenderer>();
        }
        else
        {
            // Создаем дочерний GameObject для фантомных тайлов
            phantomObj = new GameObject("PhantomTilemap");
            phantomObj.transform.SetParent(transform);
            phantomObj.transform.localPosition = Vector3.zero;
            phantomObj.transform.localScale = Vector3.one;

            // Добавляем компоненты
            phantomTilemap = phantomObj.AddComponent<Tilemap>();
            phantomRenderer = phantomObj.AddComponent<TilemapRenderer>();
        }

        // Настройка отображения
        if (phantomRenderer != null)
        {
            // Копируем настройки сортировки с основного тайлмапа
            TilemapRenderer mainRenderer = barriersTilemap?.GetComponent<TilemapRenderer>();
            if (mainRenderer != null)
            {
                phantomRenderer.sortingLayerID = mainRenderer.sortingLayerID;
                phantomRenderer.sortingOrder = mainRenderer.sortingOrder;
            }
            else
            {
                phantomRenderer.sortingLayerName = "Default";
                phantomRenderer.sortingOrder = 0;
            }

            // Устанавливаем материал с поддержкой прозрачности
            Material phantomMaterial = new Material(Shader.Find("Sprites/Default"));
            phantomMaterial.color = new Color(1f, 1f, 1f, phantomAlpha);
            phantomRenderer.material = phantomMaterial;
        }

        // Убираем коллайдер с фантомного тайлмапа
        TilemapCollider2D phantomCollider = phantomObj.GetComponent<TilemapCollider2D>();
        if (phantomCollider != null)
            Destroy(phantomCollider);

        // Делаем объект видимым в иерархии
        phantomObj.hideFlags = HideFlags.None;

        Debug.Log($"[BarriersManager] Phantom tilemap created: {phantomObj.name}, Alpha: {phantomAlpha}");
    }

    void SaveOriginalTiles()
    {
        originalTiles.Clear();
        cellStates.Clear();

        // Получаем все тайлы из основного тайлмапа
        BoundsInt bounds = barriersTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = barriersTilemap.GetTile(pos);
            if (tile != null)
            {
                originalTiles[pos] = tile;
                cellStates[pos] = true; // Все тайлы изначально solid
                Debug.Log($"[BarriersManager] Found tile at {pos}: {tile.name}");
            }
        }

        Debug.Log($"[BarriersManager] Сохранено тайлов: {originalTiles.Count}");
    }

    // Установить ячейки как SOLID (вернуть тайлы, убрать с фантомного слоя)
    public void SetCellsSolid(List<BarrierCell> cells)
    {
        if (phantomTilemap == null)
        {
            Debug.LogError("[BarriersManager] Phantom tilemap is null!");
            return;
        }

        bool changed = false;

        foreach (BarrierCell cell in cells)
        {
            if (originalTiles.TryGetValue(cell.cellPosition, out TileBase tile))
            {
                if (!cellStates.ContainsKey(cell.cellPosition) || !cellStates[cell.cellPosition])
                {
                    barriersTilemap.SetTile(cell.cellPosition, tile);
                    phantomTilemap.SetTile(cell.cellPosition, null); // Убираем фантом
                    cellStates[cell.cellPosition] = true;
                    changed = true;
                    Debug.Log($"[BarriersManager] Set SOLID at {cell.cellPosition}: {tile.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[BarriersManager] No original tile found at {cell.cellPosition}");
            }
        }

        if (changed)
            RefreshCollider();
    }

    // Установить ячейки как PHANTOM (убрать с основного слоя, показать на фантомном)
    public void SetCellsPhantom(List<BarrierCell> cells)
    {
        if (phantomTilemap == null)
        {
            Debug.LogError("[BarriersManager] Phantom tilemap is null!");
            return;
        }

        bool changed = false;

        foreach (BarrierCell cell in cells)
        {
            if (originalTiles.ContainsKey(cell.cellPosition))
            {
                if (!cellStates.ContainsKey(cell.cellPosition) || cellStates[cell.cellPosition])
                {
                    TileBase originalTile = originalTiles[cell.cellPosition];
                    barriersTilemap.SetTile(cell.cellPosition, null); // Убираем solid
                    phantomTilemap.SetTile(cell.cellPosition, originalTile); // Добавляем прозрачный
                    cellStates[cell.cellPosition] = false;
                    changed = true;
                    Debug.Log($"[BarriersManager] Set PHANTOM at {cell.cellPosition}: {originalTile.name}, Alpha: {phantomAlpha}");
                }
            }
            else
            {
                Debug.LogWarning($"[BarriersManager] No original tile found at {cell.cellPosition}");
            }
        }

        if (changed)
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
            // Принудительно обновляем коллайдер после изменений в tilemap
            barriersCollider.enabled = false;

            // Даем физическому движку время обработать отключение
            Physics2D.SyncTransforms();

            barriersCollider.enabled = true;

            // Синхронизируем физику снова
            Physics2D.SyncTransforms();

            Debug.Log($"[BarriersManager] Collider refreshed. Solid cells: {GetSolidCellCount()}");
        }
    }

    private int GetSolidCellCount()
    {
        int count = 0;
        foreach (var state in cellStates.Values)
        {
            if (state) count++;
        }
        return count;
    }

    // Публичный метод для изменения прозрачности в рантайме
    public void SetPhantomAlpha(float alpha)
    {
        phantomAlpha = Mathf.Clamp01(alpha);
        if (phantomRenderer != null && phantomRenderer.material != null)
        {
            Color color = phantomRenderer.material.color;
            color.a = phantomAlpha;
            phantomRenderer.material.color = color;
        }
    }

    // Визуализация для отладки в редакторе
    void OnDrawGizmos()
    {
        if (barriersTilemap == null) return;

        BoundsInt bounds = barriersTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = barriersTilemap.GetTile(pos);
            if (tile != null)
            {
                // Рисуем зеленый куб для активных тайлов
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Vector3 worldPos = barriersTilemap.CellToWorld(pos);
                Gizmos.DrawWireCube(worldPos + new Vector3(0.5f, 0.5f, 0), Vector3.one * 0.9f);
            }
        }

        // Рисуем фантомные тайлы синим
        if (phantomTilemap != null)
        {
            BoundsInt phantomBounds = phantomTilemap.cellBounds;
            foreach (Vector3Int pos in phantomBounds.allPositionsWithin)
            {
                TileBase tile = phantomTilemap.GetTile(pos);
                if (tile != null)
                {
                    Gizmos.color = new Color(0, 0, 1, 0.3f);
                    Vector3 worldPos = phantomTilemap.CellToWorld(pos);
                    Gizmos.DrawWireCube(worldPos + new Vector3(0.5f, 0.5f, 0), Vector3.one * 0.9f);
                }
            }
        }
    }
}