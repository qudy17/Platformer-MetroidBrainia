using UnityEngine;
using System.Collections.Generic;

// Группа преград которые переключаются вместе
// Группа A активна → Группа B неактивна, и наоборот
public class BarrierGroup : MonoBehaviour
{
    // ───────────────────────────────────────────
    //  Инспектор
    // ───────────────────────────────────────────
    [Header("Группа A (начально активна)")]
    [Tooltip("Преграды которые начинают в материальном состоянии")]
    public List<Barrier> groupA = new List<Barrier>();

    [Header("Группа B (начально неактивна)")]
    [Tooltip("Преграды которые начинают в нематериальном состоянии")]
    public List<Barrier> groupB = new List<Barrier>();

    [Header("Состояние")]
    [Tooltip("Если true — Группа A активна, Группа B нет. И наоборот.")]
    [SerializeField] private bool isGroupAActive = true;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Start()
    {
        // Применяем начальные состояния
        ApplyGroupStates();
    }

    // ───────────────────────────────────────────
    //  Публичные методы
    // ───────────────────────────────────────────

    // Переключить группы (вызывается синей кнопкой)
    public void Toggle()
    {
        isGroupAActive = !isGroupAActive;
        ApplyGroupStates();

        Debug.Log($"[BarrierGroup] {gameObject.name}: переключено → " +
                  $"Группа A {(isGroupAActive ? "ACTIVE" : "PHANTOM")}, " +
                  $"Группа B {(!isGroupAActive ? "ACTIVE" : "PHANTOM")}");
    }

    // Сбросить до начального состояния
    public void ResetGroup()
    {
        isGroupAActive = true;
        ApplyGroupStates();
        Debug.Log($"[BarrierGroup] {gameObject.name}: сброс до начального состояния");
    }

    // ───────────────────────────────────────────
    //  Логика переключения
    // ───────────────────────────────────────────
    void ApplyGroupStates()
    {
        // Группа A
        foreach (Barrier barrier in groupA)
        {
            if (barrier == null) continue;

            if (isGroupAActive)
                barrier.SetSolid();
            else
                barrier.SetPhantom();
        }

        // Группа B — всегда противоположна группе A
        foreach (Barrier barrier in groupB)
        {
            if (barrier == null) continue;

            if (isGroupAActive)
                barrier.SetPhantom();
            else
                barrier.SetSolid();
        }
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем связи с преградами группы A
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.8f);
        foreach (Barrier b in groupA)
        {
            if (b == null) continue;
            Gizmos.DrawLine(transform.position, b.transform.position);
            Gizmos.DrawWireCube(b.transform.position, Vector3.one * 0.8f);
        }

        // Рисуем связи с преградами группы B
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.4f);
        foreach (Barrier b in groupB)
        {
            if (b == null) continue;
            Gizmos.DrawLine(transform.position, b.transform.position);
            Gizmos.DrawWireCube(b.transform.position, Vector3.one * 0.8f);
        }
    }
}