using UnityEngine;
using System.Collections.Generic;

public class PressurePlate : MonoBehaviour
{
    // ───────────────────────────────────────────
    //  Типы кнопок
    // ───────────────────────────────────────────
    public enum PlateType
    {
        Trigger,    // Нужно стоять / держать блок
        Switch      // Одно нажатие меняет состояние навсегда
    }

    public enum PlateColor
    {
        Green,  // Управляет дверями
        Red,    // Управляет платформами (позже)
        Blue    // Управляет преградами (позже)
    }

    // ───────────────────────────────────────────
    //  Инспектор
    // ───────────────────────────────────────────
    [Header("Тип кнопки")]
    public PlateType plateType = PlateType.Trigger;
    public PlateColor plateColor = PlateColor.Green;

    [Header("Связанные двери")]
    [Tooltip("Список дверей которыми управляет эта кнопка")]
    public List<Door> linkedDoors = new List<Door>();

    [Header("Слои которые активируют кнопку")]
    public LayerMask activatorLayers; // Игрок + блоки

    [Header("Визуал кнопки")]
    [Tooltip("Спрайт когда кнопка НЕ нажата")]
    public Sprite spriteIdle;

    [Tooltip("Спрайт когда кнопка нажата")]
    public Sprite spritePressed;

    // ───────────────────────────────────────────
    //  Приватные поля
    // ───────────────────────────────────────────
    private SpriteRenderer spriteRenderer;
    private bool isPressed = false;         // Текущее состояние кнопки
    private bool switchActivated = false;   // Для Switch: был ли нажат хоть раз

    // Считаем объекты на кнопке (для триггерного режима)
    private int objectsOnPlate = 0;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }

    // Объект вошёл в триггер кнопки
    void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяем что объект на нужном слое
        if (!IsActivator(other)) return;

        objectsOnPlate++;

        Debug.Log($"[PressurePlate] {gameObject.name}: объект зашёл ({other.name}), " +
                  $"всего на кнопке: {objectsOnPlate}");

        // Обрабатываем нажатие
        HandlePress();
    }

    // Объект покинул триггер кнопки
    void OnTriggerExit2D(Collider2D other)
    {
        if (!IsActivator(other)) return;

        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);

        Debug.Log($"[PressurePlate] {gameObject.name}: объект ушёл ({other.name}), " +
                  $"всего на кнопке: {objectsOnPlate}");

        // Обрабатываем отпускание (только для триггерного типа)
        HandleRelease();
    }

    // ───────────────────────────────────────────
    //  Логика нажатия
    // ───────────────────────────────────────────
    void HandlePress()
    {
        if (plateType == PlateType.Switch)
        {
            // Switch: реагируем только на первое нажатие
            if (!switchActivated)
            {
                switchActivated = true;
                isPressed = true;
                UpdateVisual();
                ActivateDoors();
                Debug.Log($"[PressurePlate] {gameObject.name}: SWITCH активирован!");
            }
        }
        else // Trigger
        {
            // Trigger: нажимаем если это первый объект на кнопке
            if (objectsOnPlate == 1 && !isPressed)
            {
                isPressed = true;
                UpdateVisual();
                NotifyDoorsPressed();
                Debug.Log($"[PressurePlate] {gameObject.name}: TRIGGER нажат!");
            }
        }
    }

    void HandleRelease()
    {
        // Switch не реагирует на уход объекта
        if (plateType == PlateType.Switch) return;

        // Trigger: отпускаем когда никого нет на кнопке
        if (objectsOnPlate == 0 && isPressed)
        {
            isPressed = false;
            UpdateVisual();
            NotifyDoorsReleased();
            Debug.Log($"[PressurePlate] {gameObject.name}: TRIGGER отпущен!");
        }
    }

    // ───────────────────────────────────────────
    //  Взаимодействие с дверями
    // ───────────────────────────────────────────

    // Для Switch: переключаем двери
    void ActivateDoors()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.Toggle();
        }
    }

    // Для Trigger: сообщаем дверям что кнопка нажата
    void NotifyDoorsPressed()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.AddActivation();
        }
    }

    // Для Trigger: сообщаем дверям что кнопка отпущена
    void NotifyDoorsReleased()
    {
        foreach (Door door in linkedDoors)
        {
            if (door == null) continue;
            door.RemoveActivation();
        }
    }

    // ───────────────────────────────────────────
    //  Вспомогательные методы
    // ───────────────────────────────────────────

    bool IsActivator(Collider2D other)
    {
        // Проверяем слой объекта
        return (activatorLayers.value & (1 << other.gameObject.layer)) != 0;
    }

    void UpdateVisual()
    {
        if (spriteRenderer == null) return;

        // Меняем спрайт
        if (isPressed && spritePressed != null)
            spriteRenderer.sprite = spritePressed;
        else if (!isPressed && spriteIdle != null)
            spriteRenderer.sprite = spriteIdle;

        // Меняем цвет по типу кнопки
        // (если спрайты не назначены — цвет покажет состояние)
        Color baseColor = GetPlateColor();

        if (spriteIdle == null && spritePressed == null)
        {
            // Нет спрайтов — используем только цвет для отладки
            spriteRenderer.color = isPressed
                ? baseColor * 1.5f  // Ярче когда нажата
                : baseColor;
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

    // Сбросить состояние (вызывается при выходе из комнаты для Switch)
    public void ResetPlate()
    {
        switchActivated = false;
        isPressed = false;
        objectsOnPlate = 0;
        UpdateVisual();

        // Уведомляем двери о сбросе
        if (plateType == PlateType.Switch)
        {
            // Если Switch был активен — переключаем обратно
            foreach (Door door in linkedDoors)
            {
                if (door == null) continue;
                // Возвращаем дверь в начальное состояние
                if (door.startOpen) door.Open();
                else door.Close();
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
            Gizmos.DrawWireSphere(door.transform.position, 0.3f);
        }
    }
}