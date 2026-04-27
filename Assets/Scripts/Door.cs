using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Состояние")]
    public bool startOpen = false;

    [Header("Tilemap двери")]
    [Tooltip("Компонент визуала на дочернем Tilemap-объекте")]
    public DoorTilemapVisual tilemapVisual;

    [Header("Отладка")]
    [SerializeField] private bool isOpen;

    // Счётчик активаций для триггерных кнопок
    private int activationCount = 0;

    public bool IsOpen => isOpen;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Start()
    {
        isOpen = startOpen;
        // Применяем начальное состояние без анимации
        if (tilemapVisual != null)
            tilemapVisual.SetStateInstant(isOpen);
        else
            Debug.LogWarning($"[Door] {gameObject.name}: tilemapVisual не назначен!");
    }

    // ───────────────────────────────────────────
    //  Публичные методы (вызываются кнопками)
    // ───────────────────────────────────────────

    // Для триггерной кнопки — встали на кнопку
    public void AddActivation()
    {
        activationCount++;
        UpdateFromCount();
    }

    // Для триггерной кнопки — ушли с кнопки
    public void RemoveActivation()
    {
        activationCount = Mathf.Max(0, activationCount - 1);
        UpdateFromCount();
    }

    // Для кнопки-переключателя
    public void Toggle()
    {
        isOpen = !isOpen;
        ApplyState();
        Debug.Log($"[Door] {gameObject.name}: → {(isOpen ? "ОТКРЫТА" : "ЗАКРЫТА")}");
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
            Debug.Log($"[Door] {gameObject.name}: " +
                      $"активаций={activationCount} → " +
                      $"{(isOpen ? "ОТКРЫТА" : "ЗАКРЫТА")}");
        }
    }

    void ApplyState()
    {
        if (tilemapVisual != null)
            tilemapVisual.SetState(isOpen);
    }

    // ───────────────────────────────────────────
    //  Гизмо
    // ───────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);

        if (tilemapVisual != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, tilemapVisual.transform.position);
        }
    }
}