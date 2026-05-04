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
        Blue
    }

    [Header("Тип кнопки")]
    public PlateType plateType = PlateType.Trigger;
    public PlateColor plateColor = PlateColor.Green;

    [Header("Связанные двери (зелёная кнопка)")]
    public List<Door> linkedDoors = new List<Door>();

    [Header("Связанные группы преград (синяя кнопка)")]
    public List<BarrierGroup> linkedBarrierGroups = new List<BarrierGroup>();

    [Header("Слои которые активируют кнопку")]
    public LayerMask activatorLayers;

    [Header("Визуал кнопки")]
    public Sprite spriteIdle;
    public Sprite spritePressed;

    private SpriteRenderer spriteRenderer;
    private int objectsOnPlate = 0; // Простой счетчик
    private bool switchActivated = false;

    public System.Action<bool> OnPlateStateChanged;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        UpdateVisual();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActivator(other)) return;

        int oldCount = objectsOnPlate;
        objectsOnPlate++;

        if (oldCount == 0 && objectsOnPlate > 0)
        {
            // Кнопка была не нажата, стала нажата
            OnPressed();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsActivator(other)) return;

        int oldCount = objectsOnPlate;
        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);

        if (oldCount > 0 && objectsOnPlate == 0)
        {
            // Кнопка была нажата, стала не нажата
            OnReleased();
        }
    }

    void OnPressed()
    {
        if (plateType == PlateType.Switch)
        {
            if (!switchActivated)
            {
                switchActivated = true;
                UpdateVisual();
                ActivateLinkedObjects();
                OnPlateStateChanged?.Invoke(true);
                Debug.Log($"[PressurePlate] {gameObject.name}: SWITCH активирован!");
            }
        }
        else // Trigger
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

    void ActivateLinkedObjects()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.Toggle();
        }

        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            group.Toggle();
        }
    }

    void NotifyLinkedObjectsPressed()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.AddActivation();
        }

        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            group.Toggle();
        }
    }

    void NotifyLinkedObjectsReleased()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.RemoveActivation();
        }

        foreach (BarrierGroup group in linkedBarrierGroups)
        {
            if (group == null) continue;
            group.Toggle();
        }
    }

    bool IsActivator(Collider2D other)
    {
        return (activatorLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        bool isPressed = objectsOnPlate > 0 || switchActivated;

        // Используем соответствующий спрайт
        if (isPressed && spritePressed != null)
        {
            spriteRenderer.sprite = spritePressed;
        }
        else if (!isPressed && spriteIdle != null)
        {
            spriteRenderer.sprite = spriteIdle;
        }

        // ВАЖНО: Принудительно устанавливаем цвет
        Color targetColor = GetPlateColor();

        // Если кнопка нажата - делаем цвет ярче
        if (isPressed)
        {
            targetColor = targetColor * 1.5f;
        }

        // Устанавливаем material.color для уверенности
        spriteRenderer.color = targetColor;

        // Если материал существует, тоже меняем его цвет
        if (spriteRenderer.material != null)
        {
            spriteRenderer.material.color = targetColor;
        }

        Debug.Log($"[PressurePlate] UpdateVisual: sprite={spriteRenderer.sprite?.name}, color={spriteRenderer.color}, isPressed={isPressed}");
    }

    Color GetPlateColor()
    {
        switch (plateColor)
        {
            case PlateColor.Green:
                return Color.green;  // (0,1,0,1)
            case PlateColor.Red:
                return Color.red;    // (1,0,0,1)
            case PlateColor.Blue:
                return Color.blue;   // (0,0,1,1)
            default:
                return Color.white;
        }
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
            {
                if (group == null) continue;
                group.ResetGroup();
            }
        }

        OnPlateStateChanged?.Invoke(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = GetPlateColor();

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