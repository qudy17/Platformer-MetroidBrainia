using UnityEngine;
using System.Collections.Generic;

public class BarrierGroup : MonoBehaviour
{
    [Header("Группа A (начально материальна)")]
    [Tooltip("Tilemap-объекты преград группы A")]
    public List<BarrierTilemapVisual> groupA = new List<BarrierTilemapVisual>();

    [Header("Группа B (начально нематериальна)")]
    [Tooltip("Tilemap-объекты преград группы B")]
    public List<BarrierTilemapVisual> groupB = new List<BarrierTilemapVisual>();

    [Header("Состояние")]
    [SerializeField] private bool isGroupAActive = true;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Start()
    {
        // Применяем начальные состояния без анимации
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
        foreach (var visual in groupA)
        {
            if (visual == null) continue;
            visual.SetStateInstant(isGroupAActive);
        }

        foreach (var visual in groupB)
        {
            if (visual == null) continue;
            visual.SetStateInstant(!isGroupAActive);
        }
    }

    void ApplyStatesAnimated()
    {
        foreach (var visual in groupA)
        {
            if (visual == null) continue;
            visual.SetState(isGroupAActive);
        }

        foreach (var visual in groupB)
        {
            if (visual == null) continue;
            visual.SetState(!isGroupAActive);
        }
    }

    // ───────────────────────────────────────────
    //  Гизмо
    // ───────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.8f);
        foreach (var v in groupA)
        {
            if (v == null) continue;
            Gizmos.DrawLine(transform.position, v.transform.position);
            Gizmos.DrawWireCube(v.transform.position, Vector3.one);
        }

        Gizmos.color = new Color(0f, 0.8f, 1f, 0.3f);
        foreach (var v in groupB)
        {
            if (v == null) continue;
            Gizmos.DrawLine(transform.position, v.transform.position);
            Gizmos.DrawWireCube(v.transform.position, Vector3.one);
        }
    }
}