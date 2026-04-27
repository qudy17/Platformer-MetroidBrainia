using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class BarrierTilemapVisual : MonoBehaviour
{
    [Header("Визуал")]
    [Tooltip("Прозрачность когда преграда материальна")]
    [Range(0f, 1f)]
    public float solidAlpha = 0.9f;

    [Tooltip("Прозрачность когда преграда нематериальна")]
    [Range(0f, 1f)]
    public float phantomAlpha = 0.15f;

    [Tooltip("Скорость перехода между состояниями")]
    public float transitionSpeed = 4f;

    [Header("Цвет преграды")]
    public Color barrierColor = new Color(0.2f, 0.5f, 1f, 1f);

    // ───────────────────────────────────────────
    //  Приватные поля
    // ───────────────────────────────────────────
    private Tilemap tilemap;
    private TilemapCollider2D tilemapCollider;
    private bool isSolid = true;

    // Для плавного перехода
    private Coroutine transitionCoroutine;

    public bool IsSolid => isSolid;

    // ───────────────────────────────────────────
    //  Unity lifecycle
    // ───────────────────────────────────────────
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        tilemapCollider = GetComponent<TilemapCollider2D>();
    }

    // ───────────────────────────────────────────
    //  Публичные методы
    // ───────────────────────────────────────────

    // Установить начальное состояние БЕЗ анимации
    public void SetStateInstant(bool solid)
    {
        isSolid = solid;

        // Останавливаем текущий переход если был
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        // Применяем состояние мгновенно
        float alpha = isSolid ? solidAlpha : phantomAlpha;
        tilemap.color = new Color(
            barrierColor.r,
            barrierColor.g,
            barrierColor.b,
            alpha
        );

        if (tilemapCollider != null)
            tilemapCollider.enabled = isSolid;
    }

    // Установить состояние С анимацией
    public void SetState(bool solid)
    {
        isSolid = solid;

        // Коллайдер меняем сразу
        if (tilemapCollider != null)
            tilemapCollider.enabled = isSolid;

        // Запускаем плавный переход цвета
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionColor(isSolid));
    }

    // ───────────────────────────────────────────
    //  Плавный переход цвета
    // ───────────────────────────────────────────
    IEnumerator TransitionColor(bool toSolid)
    {
        float targetAlpha = toSolid ? solidAlpha : phantomAlpha;
        Color startColor = tilemap.color;
        Color endColor = new Color(
            barrierColor.r,
            barrierColor.g,
            barrierColor.b,
            targetAlpha
        );

        float elapsed = 0f;
        float duration = 1f / transitionSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Плавная интерполяция с ease-out
            float smoothT = 1f - Mathf.Pow(1f - t, 2f);

            tilemap.color = Color.Lerp(startColor, endColor, smoothT);
            yield return null;
        }

        tilemap.color = endColor;
        transitionCoroutine = null;
    }
}