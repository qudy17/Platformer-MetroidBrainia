using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Состояние")]
    [Tooltip("Открыта ли дверь в начале")]
    public bool startOpen = false;

    [Header("Компоненты")]
    [Tooltip("Коллайдер который блокирует проход (отключается когда открыта)")]
    public Collider2D doorCollider;

    [Tooltip("Визуальный объект двери (скрывается когда открыта)")]
    public GameObject doorVisual;

    [Header("Отладка")]
    [SerializeField] private bool isOpen;

    // Сколько кнопок сейчас держат дверь открытой (для триггерных кнопок)
    private int activationCount = 0;

    void Start()
    {
        // Устанавливаем начальное состояние
        isOpen = startOpen;
        ApplyState();
    }

    // Вызывается триггерной кнопкой когда на неё встали
    public void AddActivation()
    {
        activationCount++;
        UpdateFromCount();
    }

    // Вызывается триггерной кнопкой когда с неё ушли
    public void RemoveActivation()
    {
        activationCount = Mathf.Max(0, activationCount - 1);
        UpdateFromCount();
    }

    // Вызывается кнопкой-переключателем
    public void Toggle()
    {
        isOpen = !isOpen;
        ApplyState();
        Debug.Log($"[Door] {gameObject.name}: переключено → {(isOpen ? "ОТКРЫТА" : "ЗАКРЫТА")}");
    }

    // Принудительно открыть (используется извне если нужно)
    public void Open()
    {
        isOpen = true;
        ApplyState();
    }

    // Принудительно закрыть
    public void Close()
    {
        isOpen = false;
        ApplyState();
    }

    public bool IsOpen => isOpen;

    void UpdateFromCount()
    {
        // Дверь открыта пока хотя бы одна триггерная кнопка активна
        bool shouldBeOpen = activationCount > 0;

        if (shouldBeOpen != isOpen)
        {
            isOpen = shouldBeOpen;
            ApplyState();
            Debug.Log($"[Door] {gameObject.name}: " +
                      $"активаций={activationCount} → {(isOpen ? "ОТКРЫТА" : "ЗАКРЫТА")}");
        }
    }

    void ApplyState()
    {
        // Включаем/выключаем коллайдер
        if (doorCollider != null)
            doorCollider.enabled = !isOpen;

        // Показываем/скрываем визуал
        if (doorVisual != null)
            doorVisual.SetActive(!isOpen);
    }

    void OnDrawGizmosSelected()
    {
        // Показываем состояние в редакторе
        Gizmos.color = isOpen ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}