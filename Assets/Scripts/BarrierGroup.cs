using UnityEngine;
using System.Collections.Generic;
// тест русского языка
public class BarrierGroup : MonoBehaviour
{
    [Header("Группа A (начально материальна)")]
    public List<BarrierCell> groupACells = new List<BarrierCell>();

    [Header("Группа B (начально нематериальна)")]
    public List<BarrierCell> groupBCells = new List<BarrierCell>();

    [Header("Состояние")]
    [SerializeField] private bool isGroupAActive = true;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Start()
    {
        ApplyStatesInstant();
    }

    // ───────────────────────────────────────────
    //  Публичные методы
    // ───────────────────────────────────────────
    public void Toggle()
    {
        isGroupAActive = !isGroupAActive;
        ApplyStatesAnimated();

        Debug.Log($"[BarrierGroup] {gameObject.name}: " +
                  $"A={(isGroupAActive ? "SOLID" : "PHANTOM")}, " +
                  $"B={(!isGroupAActive ? "SOLID" : "PHANTOM")}");
    }

    public void ResetGroup()
    {
        isGroupAActive = true;
        ApplyStatesInstant();
    }

    // ───────────────────────────────────────────
    //  Применение состояний
    // ───────────────────────────────────────────
    void ApplyStatesInstant()
    {
        if (BarriersManager.Instance == null) return;

        if (isGroupAActive)
        {
            BarriersManager.Instance.SetCellsSolid(groupACells);
            BarriersManager.Instance.SetCellsPhantom(groupBCells);
        }
        else
        {
            BarriersManager.Instance.SetCellsPhantom(groupACells);
            BarriersManager.Instance.SetCellsSolid(groupBCells);
        }
    }

    void ApplyStatesAnimated()
    {
        // Сейчас без анимации, просто переключаем
        ApplyStatesInstant();
    }

    // ───────────────────────────────────────────
    //  Гизмо
    // ───────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        BarriersManager manager = FindFirstObjectByType<BarriersManager>();
        if (manager == null || manager.barriersTilemap == null) return;

        // Группа A — синий
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.6f);
        foreach (BarrierCell cell in groupACells)
        {
            Vector3 worldPos = manager.barriersTilemap
                .GetCellCenterWorld(cell.cellPosition);
            Gizmos.DrawCube(worldPos, Vector3.one * 0.9f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                worldPos + Vector3.up * 0.3f, "A"
            );
#endif
        }

        // Группа B — голубой
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        foreach (BarrierCell cell in groupBCells)
        {
            Vector3 worldPos = manager.barriersTilemap
                .GetCellCenterWorld(cell.cellPosition);
            Gizmos.DrawCube(worldPos, Vector3.one * 0.9f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(
                worldPos + Vector3.up * 0.3f, "B"
            );
#endif
        }
    }
}