using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectRatio : MonoBehaviour
{
    [Header("Целевое соотношение сторон")]
    public float targetAspectWidth = 16f;
    public float targetAspectHeight = 9f;

    [Header("Цвет полос (letterbox/pillarbox)")]
    public Color barsColor = Color.black;

    private Camera cam;
    private float targetAspect;
    private int lastScreenWidth;
    private int lastScreenHeight;

    void Awake()
    {
        cam = GetComponent<Camera>();
        targetAspect = targetAspectWidth / targetAspectHeight;

        // Применяем сразу
        ApplyAspectRatio();

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    void Update()
    {
        // Проверяем изменение размера окна
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ApplyAspectRatio();
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }

    void ApplyAspectRatio()
    {
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // Окно слишком узкое → pillarbox (полосы по бокам)
            // Нет, это letterbox (полосы сверху и снизу)
            Rect rect = cam.rect;

            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;

            cam.rect = rect;
        }
        else
        {
            // Окно слишком широкое → pillarbox (полосы по бокам)
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;

            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;

            cam.rect = rect;
        }

        Debug.Log($"[CameraAspect] Screen: {Screen.width}x{Screen.height}, " +
                  $"WindowAspect: {windowAspect:F3}, " +
                  $"CamRect: {cam.rect}");
    }

    // Рисуем чёрные полосы через GL (для WebGL)
    void OnPreCull()
    {
        GL.Clear(true, true, barsColor);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        targetAspect = targetAspectWidth / targetAspectHeight;
    }
#endif
}