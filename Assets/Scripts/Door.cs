using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using System.Collections;

public class Door : MonoBehaviour
{
    [Header("Идентификатор")]
    public string doorID = "door_1";

    [Header("Состояние")]
    public bool startOpen = false;

    [Header("Ячейки этой двери в общем Tilemap")]
    [Tooltip("Список ячеек которые принадлежат этой двери")]
    public List<DoorCell> doorCells = new List<DoorCell>();

    [Header("Визуал (анимация прозрачности)")]
    [Tooltip("Скорость появления/исчезновения")]
    public float transitionSpeed = 5f;

    [SerializeField] private bool isOpen;
    private int activationCount = 0;

    public bool IsOpen => isOpen;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Start()
    {
        isOpen = startOpen;
        ApplyStateInstant();
    }

    // ───────────────────────────────────────────
    //  Публичные методы
    // ───────────────────────────────────────────
    public void AddActivation()
    {
        activationCount++;
        UpdateFromCount();
    }

    public void RemoveActivation()
    {
        activationCount = Mathf.Max(0, activationCount - 1);
        UpdateFromCount();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        ApplyState();
        Debug.Log($"[Door] {gameObject.name} (ID:{doorID}): " +
                  $"→ {(isOpen ? "ОТКРЫТА" : "ЗАКРЫТА")}");
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;
        ApplyState();
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;
        ApplyState();
    }

    // ───────────────────────────────────────────
    //  Внутренняя логика
    // ───────────────────────────────────────────
    void UpdateFromCount()
    {
        bool shouldBeOpen = activationCount > 0;
        if (shouldBeOpen != isOpen)
        {
            isOpen = shouldBeOpen;
            ApplyState();
        }
    }

    // Мгновенное применение при старте
    void ApplyStateInstant()
    {
        if (DoorsManager.Instance == null)
        {
            Debug.LogError($"[Door] {gameObject.name}: DoorsManager не найден!");
            return;
        }

        if (isOpen)
            DoorsManager.Instance.OpenCells(doorCells);
        else
            DoorsManager.Instance.CloseCells(doorCells);
    }

    // Анимированное применение
    void ApplyState()
    {
        if (DoorsManager.Instance == null) return;

        if (isOpen)
            DoorsManager.Instance.OpenCells(doorCells);
        else
            DoorsManager.Instance.CloseCells(doorCells);
    }

    // ───────────────────────────────────────────
    //  Гизмо — показывает ячейки двери в редакторе
    // ───────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        // Ищем DoorsManager чтобы получить Tilemap
        DoorsManager manager = FindFirstObjectByType<DoorsManager>();
        if (manager == null || manager.doorsTilemap == null) return;

        Tilemap tilemap = manager.doorsTilemap;

        Gizmos.color = isOpen
            ? new Color(0f, 1f, 0f, 0.3f)
            : new Color(1f, 0f, 0f, 0.3f);

        foreach (DoorCell cell in doorCells)
        {
            // Переводим координаты ячейки в мировые координаты
            Vector3 worldPos = tilemap.GetCellCenterWorld(cell.cellPosition);
            Gizmos.DrawCube(worldPos, Vector3.one * 0.9f);
        }

        // Рисуем ID рядом с первой ячейкой
        if (doorCells.Count > 0)
        {
            Vector3 labelPos = tilemap.GetCellCenterWorld(
                doorCells[0].cellPosition
            ) + Vector3.up * 0.5f;

#if UNITY_EDITOR
            UnityEditor.Handles.Label(labelPos, $"ID: {doorID}");
#endif
        }
    }
}