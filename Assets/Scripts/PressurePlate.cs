using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    public enum PlateType
    {
        Trigger,
        Switch
    }

    public enum PlateColor
    {
        Green,
        Red,
        Blue    // ← управляет преградами
    }

    [Header("Тип кнопки")]
    public PlateType plateType = PlateType.Trigger;
    public PlateColor plateColor = PlateColor.Green;

    [Header("Связанные двери (зелёная кнопка)")]
    public List<Door> linkedDoors = new List<Door>();

    // ── НОВОЕ ──────────────────────────────────
    [Header("Связанные группы преград (синяя кнопка)")]
    public List<BarrierGroup> linkedBarrierGroups = new List<BarrierGroup>();
    // ───────────────────────────────────────────

    [Header("Слои которые активируют кнопку")]
    public LayerMask activatorLayers;

    [Header("Визуал кнопки")]
    public Sprite spriteIdle;
    public Sprite spritePressed;

    private SpriteRenderer spriteRenderer;
    private bool isPressed = false;
    private bool switchActivated = false;
    private int objectsOnPlate = 0;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActivator(other)) return;

        objectsOnPlate++;
        HandlePress();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsActivator(other)) return;

        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);
        HandleRelease();
    }

    void HandlePress()
    {
        if (plateType == PlateType.Switch)
        {
            if (!switchActivated)
            {
                switchActivated = true;
                isPressed = true;
                UpdateVisual();
                ActivateLinkedObjects();
                Debug.Log($"[PressurePlate] {gameObject.name}: SWITCH активирован!");
            }
        }
        else
        {
            if (objectsOnPlate == 1 && !isPressed)
            {
                isPressed = true;
                UpdateVisual();
                NotifyLinkedObjectsPressed();
                Debug.Log($"[PressurePlate] {gameObject.name}: TRIGGER нажат!");
            }
        }
    }

    void HandleRelease()
    {
        if (plateType == PlateType.Switch) return;

        if (objectsOnPlate == 0 && isPressed)
        {
            isPressed = false;
            UpdateVisual();
            NotifyLinkedObjectsReleased();
            Debug.Log($"[PressurePlate] {gameObject.name}: TRIGGER отпущен!");
        }
    }

    // ── Switch: одноразовое переключение ───────
    void ActivateLinkedObjects()
    {
        // Зелёная — переключаем двери
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.Toggle();
        }

        // Синяя — переключаем группы преград
        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            group.Toggle();
        }
    }

    // ── Trigger: нажатие ───────────────────────
    void NotifyLinkedObjectsPressed()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.AddActivation();
        }

        // Синяя триггерная: нажали — переключили
        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            group.Toggle();
        }
    }

    // ── Trigger: отпускание ────────────────────
    void NotifyLinkedObjectsReleased()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.RemoveActivation();
        }

        // Синяя триггерная: отпустили — переключили обратно
        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            group.Toggle();
        }
    }

    // ───────────────────────────────────────────
    bool IsActivator(Collider2D other)
    {
        return (activatorLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        if (isPressed && spritePressed != null)
            spriteRenderer.sprite = spritePressed;
        else if (!isPressed && spriteIdle != null)
            spriteRenderer.sprite = spriteIdle;

        if (spriteIdle == null && spritePressed == null)
        {
            spriteRenderer.color = isPressed
                ? GetPlateColor() * 1.5f
                : GetPlateColor();
        }
    }

    Color GetPlateColor()
    {
        switch (plateColor)
        {
            case PlateColor.Green: return Color.green;
            case PlateColor.Red: return Color.red;
            case PlateColor.Blue: return Color.blue;
            default: return Color.white;
        }
    }

    public void ResetPlate()
    {
        switchActivated = false;
        isPressed = false;
        objectsOnPlate = 0;
        UpdateVisual();

        if (plateType == PlateType.Switch)
        {
            foreach (Door door in linkedDoors)
            {
                if (door == null) continue;
                if (door.startOpen) door.Open();
                else door.Close();
            }

            // Сбрасываем группы преград
            foreach (BarrierGroup group in linkedBarrierGroups)
            {
                if (group == null) continue;
                group.ResetGroup();
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем линии к связанным дверям
        Gizmos.color = GetPlateColor();

        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            Gizmos.DrawLine(transform.position, door.transform.position);
        }

        // Линии к группам преград
        Gizmos.color = Color.cyan;
        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            Gizmos.DrawLine(transform.position, group.transform.position);
            Gizmos.DrawWireSphere(group.transform.position, 0.4f);
        }
    }
}