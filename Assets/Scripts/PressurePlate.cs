using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    public enum PlateType
    {
        Trigger,
        Switch
    }

    [Header("Тип кнопки")]
    public PlateType plateType = PlateType.Trigger;

    [Header("Связанные двери")]
    public List<Door> linkedDoors = new List<Door>();

    [Header("Связанные группы преград")]
    public List<BarrierGroup> linkedBarrierGroups = new List<BarrierGroup>();

    [Header("Слои которые активируют кнопку")]
    public LayerMask activatorLayers;

    [Header("Визуал кнопки")]
    public Sprite spriteIdle;
    public Sprite spritePressed;

    private SpriteRenderer spriteRenderer;
    private int objectsOnPlate = 0;
    private bool switchActivated = false;

    public System.Action<bool> OnPlateStateChanged;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActivator(other)) return;

        int oldCount = objectsOnPlate;
        objectsOnPlate++;

        if (oldCount == 0)
            OnPressed();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsActivator(other)) return;

        int oldCount = objectsOnPlate;
        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);

        if (oldCount > 0 && objectsOnPlate == 0)
            OnReleased();
    }

    void OnPressed()
    {
        if (plateType == PlateType.Switch)
        {
            if (switchActivated) return;

            switchActivated = true;
            UpdateVisual();
            ActivateLinkedObjects();
            OnPlateStateChanged?.Invoke(true);
            Debug.Log($"[PressurePlate] {gameObject.name}: SWITCH активирован!");
        }
        else
        {
            UpdateVisual();
            NotifyLinkedObjectsPressed();
            OnPlateStateChanged?.Invoke(true);
            Debug.Log($"[PressurePlate] {gameObject.name}: TRIGGER нажат!");
        }
    }

    void OnReleased()
    {
        if (plateType == PlateType.Switch) return;

        UpdateVisual();
        NotifyLinkedObjectsReleased();
        OnPlateStateChanged?.Invoke(false);
        Debug.Log($"[PressurePlate] {gameObject.name}: TRIGGER отпущен!");
    }

    // Switch: одноразовое переключение
    void ActivateLinkedObjects()
    {
        foreach (Door door in linkedDoors)
            door?.Toggle();

        foreach (BarrierGroup group in linkedBarrierGroups)
            group?.Toggle();
    }

    // Trigger: удерживаемое нажатие
    void NotifyLinkedObjectsPressed()
    {
        foreach (Door door in linkedDoors)
            door?.AddActivation();

        foreach (BarrierGroup group in linkedBarrierGroups)
            group?.Toggle();
    }

    void NotifyLinkedObjectsReleased()
    {
        foreach (Door door in linkedDoors)
            door?.RemoveActivation();

        foreach (BarrierGroup group in linkedBarrierGroups)
            group?.Toggle();
    }

    bool IsActivator(Collider2D other)
    {
        return (activatorLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        bool isPressed = objectsOnPlate > 0 || switchActivated;
        spriteRenderer.sprite = isPressed ? spritePressed : spriteIdle;
    }

    public bool IsPressed()
    {
        return objectsOnPlate > 0 || switchActivated;
    }

    public void ResetPlate()
    {
        switchActivated = false;
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

            foreach (BarrierGroup group in linkedBarrierGroups)
                group?.ResetGroup();
        }

        OnPlateStateChanged?.Invoke(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            Gizmos.DrawLine(transform.position, door.transform.position);
        }

        Gizmos.color = Color.cyan;

        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            Gizmos.DrawLine(transform.position, group.transform.position);
            Gizmos.DrawWireSphere(group.transform.position, 0.4f);
        }
    }
}