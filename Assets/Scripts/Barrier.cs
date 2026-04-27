using UnityEngine;

public class Barrier : MonoBehaviour
{
    // ───────────────────────────────────────────
    //  Состояния преграды
    // ───────────────────────────────────────────
    public enum BarrierState
    {
        Solid,      // Материальная — блокирует всё
        Phantom     // Нематериальная — всё проходит насквозь
    }

    // ───────────────────────────────────────────
    //  Инспектор
    // ───────────────────────────────────────────
    [Header("Начальное состояние")]
    public BarrierState startState = BarrierState.Solid;

    [Header("Визуал")]
    [Tooltip("Прозрачность в материальном состоянии (0-1)")]
    [Range(0f, 1f)]
    public float solidAlpha = 0.9f;

    [Tooltip("Прозрачность в нематериальном состоянии (0-1)")]
    [Range(0f, 1f)]
    public float phantomAlpha = 0.2f;

    [Tooltip("Цвет в материальном состоянии")]
    public Color solidColor = new Color(0.2f, 0.5f, 1f, 1f);

    [Tooltip("Цвет в нематериальном состоянии")]
    public Color phantomColor = new Color(0.2f, 0.5f, 1f, 0.2f);

    [Header("Анимация перехода")]
    [Tooltip("Скорость перехода между состояниями")]
    public float transitionSpeed = 5f;

    // ───────────────────────────────────────────
    //  Приватные поля
    // ───────────────────────────────────────────
    private BarrierState currentState;
    private SpriteRenderer spriteRenderer;
    private Collider2D barrierCollider;

    // Для плавного перехода цвета
    private Color targetColor;
    private bool isTransitioning = false;

    public BarrierState CurrentState => currentState;
    public bool IsSolid => currentState == BarrierState.Solid;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        barrierCollider = GetComponent<Collider2D>();

        // Устанавливаем начальное состояние БЕЗ анимации
        currentState = startState;
        ApplyStateInstant();
    }

    void Update()
    {
        // Плавный переход цвета
        if (isTransitioning && spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(
                spriteRenderer.color,
                targetColor,
                transitionSpeed * Time.deltaTime
            );

            // Проверяем достаточно ли близко к целевому цвету
            if (ColorDistance(spriteRenderer.color, targetColor) < 0.01f)
            {
                spriteRenderer.color = targetColor;
                isTransitioning = false;
            }
        }
    }

    // ───────────────────────────────────────────
    //  Публичные методы
    // ───────────────────────────────────────────

    // Переключить в материальное состояние
    public void SetSolid()
    {
        if (currentState == BarrierState.Solid) return;

        currentState = BarrierState.Solid;
        ApplyStateAnimated();

        Debug.Log($"[Barrier] {gameObject.name}: → МАТЕРИАЛЬНАЯ");
    }

    // Переключить в нематериальное состояние
    public void SetPhantom()
    {
        if (currentState == BarrierState.Phantom) return;

        currentState = BarrierState.Phantom;
        ApplyStateAnimated();

        Debug.Log($"[Barrier] {gameObject.name}: → НЕМАТЕРИАЛЬНАЯ");
    }

    // Переключить состояние на противоположное
    public void Toggle()
    {
        if (currentState == BarrierState.Solid)
            SetPhantom();
        else
            SetSolid();
    }

    // Сбросить до начального состояния (при выходе из комнаты)
    public void ResetToStart()
    {
        currentState = startState;
        ApplyStateInstant();
    }

    // ───────────────────────────────────────────
    //  Применение состояния
    // ───────────────────────────────────────────

    // Мгновенное применение (без анимации) — для инициализации
    void ApplyStateInstant()
    {
        if (currentState == BarrierState.Solid)
        {
            // Включаем коллайдер
            if (barrierCollider != null)
                barrierCollider.enabled = true;

            // Устанавливаем цвет сразу
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(
                    solidColor.r, solidColor.g, solidColor.b, solidAlpha
                );
        }
        else // Phantom
        {
            // Выключаем коллайдер
            if (barrierCollider != null)
                barrierCollider.enabled = false;

            // Устанавливаем цвет сразу
            if (spriteRenderer != null)
                spriteRenderer.color = new Color(
                    phantomColor.r, phantomColor.g, phantomColor.b, phantomAlpha
                );
        }

        isTransitioning = false;
    }

    // Анимированное применение (с плавным переходом)
    void ApplyStateAnimated()
    {
        if (currentState == BarrierState.Solid)
        {
            // Коллайдер включаем СРАЗУ — нельзя проходить сквозь
            if (barrierCollider != null)
                barrierCollider.enabled = true;

            targetColor = new Color(
                solidColor.r, solidColor.g, solidColor.b, solidAlpha
            );
        }
        else // Phantom
        {
            // Коллайдер выключаем СРАЗУ — можно проходить сквозь
            if (barrierCollider != null)
                barrierCollider.enabled = false;

            targetColor = new Color(
                phantomColor.r, phantomColor.g, phantomColor.b, phantomAlpha
            );
        }

        isTransitioning = true;
    }

    // ───────────────────────────────────────────
    //  Вспомогательные методы
    // ───────────────────────────────────────────

    float ColorDistance(Color a, Color b)
    {
        return Mathf.Abs(a.r - b.r) +
               Mathf.Abs(a.g - b.g) +
               Mathf.Abs(a.b - b.b) +
               Mathf.Abs(a.a - b.a);
    }

    void OnDrawGizmosSelected()
    {
        // Показываем состояние в редакторе
        Gizmos.color = (currentState == BarrierState.Solid)
            ? new Color(0.2f, 0.5f, 1f, 0.8f)
            : new Color(0.2f, 0.5f, 1f, 0.2f);

        if (TryGetComponent<SpriteRenderer>(out var sr))
            Gizmos.DrawWireCube(transform.position, sr.bounds.size);
        else
            Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}