using System.Collections;
using UnityEngine;

public class IrisWipeEffect : MonoBehaviour
{
    [Header("Объекты эффекта")]
    [SerializeField] private SpriteRenderer blackPanel;
    [SerializeField] private SpriteMask circleMask;

    [Header("Настройки анимации")]
    [SerializeField] private float closeDuration = 1.0f;
    [SerializeField] private float openDuration = 1.0f;

    // Максимальный размер маски — покрывает экран с запасом
    private float _maxSize;
    private const float MIN_SIZE = 0f;

    private void Awake()
    {
        // Считаем размер в мировых единицах через камеру
        Camera cam = Camera.main;
        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        _maxSize = Mathf.Sqrt(camWidth * camWidth + camHeight * camHeight) * 6f;

        InitBlackPanel();

        // Стартовое состояние — эффект выключен (маска максимальная = экран виден сквозь дыру)
        SetCircleSize(_maxSize);
        SetVisible(false);
    }

    private void InitBlackPanel()
    {
        if (blackPanel == null) return;

        // Чёрный цвет, сортировка поверх всего
        blackPanel.color = Color.black;

        // Чёрная панель маскируется — там где маска, панель НЕ рисуется
        blackPanel.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
    }

    private void SetCircleSize(float size)
    {
        if (circleMask == null) return;
        circleMask.transform.localScale = new Vector3(size, size, 1f);
    }

    private void SetVisible(bool visible)
    {
        if (blackPanel != null)
            blackPanel.gameObject.SetActive(visible);

        if (circleMask != null)
            circleMask.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Закрыть iris: круг сужается → чёрный экран
    /// </summary>
    public IEnumerator CloseIris()
    {
        SetVisible(true);
        SetCircleSize(_maxSize);

        float elapsed = 0f;

        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / closeDuration);
            float smoothed = Mathf.SmoothStep(0f, 1f, t);

            // MAX → 0 : дыра сужается
            SetCircleSize(Mathf.Lerp(_maxSize, MIN_SIZE, smoothed));
            yield return null;
        }

        SetCircleSize(MIN_SIZE);
    }

    /// <summary>
    /// Открыть iris: круг расширяется → экран виден
    /// </summary>
    public IEnumerator OpenIris()
    {
        // Убеждаемся что видимо
        SetVisible(true);
        SetCircleSize(MIN_SIZE);

        float elapsed = 0f;

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            float smoothed = Mathf.SmoothStep(0f, 1f, t);

            // 0 → MAX : дыра расширяется
            SetCircleSize(Mathf.Lerp(MIN_SIZE, _maxSize, smoothed));
            yield return null;
        }

        SetCircleSize(_maxSize);

        // Скрываем — эффект завершён
        SetVisible(false);
    }
}