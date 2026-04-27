using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

[RequireComponent(typeof(Tilemap))]
[RequireComponent(typeof(TilemapRenderer))]
public class DoorTilemapVisual : MonoBehaviour
{
    [Header("Визуал")]
    [Tooltip("Прозрачность когда дверь закрыта")]
    [Range(0f, 1f)]
    public float closedAlpha = 1f;

    [Tooltip("Прозрачность когда дверь открыта (0 = полностью невидима)")]
    [Range(0f, 1f)]
    public float openAlpha = 0f;

    [Tooltip("Скорость перехода")]
    public float transitionSpeed = 5f;

    [Header("Цвет двери")]
    public Color doorColor = new Color(0.2f, 0.8f, 0.2f, 1f);

    // ───────────────────────────────────────────
    //  Приватные поля
    // ───────────────────────────────────────────
    private Tilemap tilemap;
    private TilemapCollider2D tilemapCollider;
    private Coroutine transitionCoroutine;

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

    // Мгновенное применение (для инициализации)
    public void SetStateInstant(bool isOpen)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }

        float alpha = isOpen ? openAlpha : closedAlpha;
        tilemap.color = new Color(
            doorColor.r,
            doorColor.g,
            doorColor.b,
            alpha
        );

        // Коллайдер: закрыта = блокирует, открыта = не блокирует
        if (tilemapCollider != null)
            tilemapCollider.enabled = !isOpen;
    }

    // Анимированное применение
    public void SetState(bool isOpen)
    {
        // Коллайдер меняем сразу
        if (tilemapCollider != null)
            tilemapCollider.enabled = !isOpen;

        // Запускаем плавный переход
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(TransitionColor(isOpen));
    }

    // ───────────────────────────────────────────
    //  Плавный переход
    // ───────────────────────────────────────────
    IEnumerator TransitionColor(bool isOpen)
    {
        float targetAlpha = isOpen ? openAlpha : closedAlpha;
        Color startColor = tilemap.color;
        Color endColor = new Color(
            doorColor.r,
            doorColor.g,
            doorColor.b,
            targetAlpha
        );

        float elapsed = 0f;
        float duration = 1f / transitionSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease-out для плавности
            float smoothT = 1f - Mathf.Pow(1f - t, 2f);
            tilemap.color = Color.Lerp(startColor, endColor, smoothT);

            yield return null;
        }

        tilemap.color = endColor;
        transitionCoroutine = null;
    }
}