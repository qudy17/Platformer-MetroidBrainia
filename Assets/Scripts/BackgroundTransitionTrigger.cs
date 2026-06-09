using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundTransitionTrigger : MonoBehaviour
{
    [Header("Целевой спрайт (новый фон финальной комнаты)")]
    [SerializeField] private Sprite targetSprite;

    [Header("Спрайт за чёрным экраном (финальная сцена)")]
    [SerializeField] private Sprite finalSprite;

    [Header("Настройки первого перехода (смена фона)")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Задержка перед iris эффектом")]
    [SerializeField] private float delayBeforeIris = 2.0f;

    [Header("Iris эффект")]
    [SerializeField] private IrisWipeEffect irisEffect;

    [Header("Настройки")]
    [SerializeField] private string playerTag = "Player";

    // Компоненты
    private Room _finalRoom;
    private SpriteRenderer _roomSpriteRenderer;
    private SpriteRenderer _overlayRenderer;

    private bool _triggered = false;

    private void Start()
    {
        FindFinalRoom();
        CreateOverlayRenderer();
    }

    private void FindFinalRoom()
    {
        _finalRoom = GetComponentInParent<Room>();

        if (_finalRoom == null)
        {
            Room[] allRooms = FindObjectsByType<Room>(FindObjectsSortMode.None);
            foreach (Room room in allRooms)
            {
                if (room.isFinalRoom)
                {
                    _finalRoom = room;
                    break;
                }
            }
        }

        if (_finalRoom == null)
        {
            Debug.LogError("[BackgroundTransitionTrigger] Финальная комната не найдена!");
            return;
        }

        _roomSpriteRenderer = _finalRoom.GetComponent<SpriteRenderer>();

        if (_roomSpriteRenderer == null)
            Debug.LogError("[BackgroundTransitionTrigger] SpriteRenderer не найден!");
    }

    private void CreateOverlayRenderer()
    {
        if (_finalRoom == null || _roomSpriteRenderer == null) return;

        GameObject overlayObj = new GameObject("BackgroundOverlay");
        overlayObj.transform.SetParent(_finalRoom.transform);
        overlayObj.transform.localPosition = Vector3.zero;
        overlayObj.transform.localScale = Vector3.one;

        _overlayRenderer = overlayObj.AddComponent<SpriteRenderer>();
        _overlayRenderer.sprite = targetSprite;
        _overlayRenderer.sortingLayerID = _roomSpriteRenderer.sortingLayerID;
        _overlayRenderer.sortingOrder = _roomSpriteRenderer.sortingOrder + 1;

        SetAlpha(_overlayRenderer, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag(playerTag)) return;
        if (_roomSpriteRenderer == null || _overlayRenderer == null) return;

        _triggered = true;
        StartCoroutine(FullSequence());
    }

    private IEnumerator FullSequence()
    {
        // ── Шаг 1: Плавная смена фона ──────────────────────────────
        yield return StartCoroutine(FadeBackground());

        // ── Шаг 2: Задержка перед iris ─────────────────────────────
        yield return new WaitForSeconds(delayBeforeIris);

        // ── Шаг 3: Iris закрывается ────────────────────────────────
        if (irisEffect != null)
            yield return StartCoroutine(irisEffect.CloseIris());

        // ── Шаг 4: Меняем спрайт пока экран чёрный ────────────────
        if (finalSprite != null)
            _roomSpriteRenderer.sprite = finalSprite;

        GameObject finalPrefab = GameObject.Find("FinalPrefab");
        Destroy(finalPrefab);

        // Убираем оверлей (он уже не нужен)
        if (_overlayRenderer != null)
        {
            Destroy(_overlayRenderer.gameObject);
            _overlayRenderer = null;
        }

        // ── Шаг 5: Iris открывается ────────────────────────────────
        if (irisEffect != null)
            yield return StartCoroutine(irisEffect.OpenIris());

        Debug.Log("[BackgroundTransitionTrigger] Полная последовательность завершена.");

        float finaltimer = 0f;
        float maxfinaltimer = 500f;
        while (finaltimer < maxfinaltimer)
        {
            finaltimer += Time.deltaTime;
        }

        SceneManager.LoadScene("MainMenu");
        Checkpoint.ClearSavedCheckpoint();
        GameStatsTracker.ResetStats();

    }

    private IEnumerator FadeBackground()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float smoothed = Mathf.SmoothStep(0f, 1f, t);

            SetAlpha(_overlayRenderer, smoothed);
            yield return null;
        }

        SetAlpha(_overlayRenderer, 1f);

        // Фиксируем результат на основном рендерере
        _roomSpriteRenderer.sprite = targetSprite;
        SetAlpha(_overlayRenderer, 0f);
    }

    private void SetAlpha(SpriteRenderer sr, float alpha)
    {
        if (sr == null) return;
        Color c = sr.color;
        c.a = alpha;
        sr.color = c;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = new Color(1f, 0.84f, 0f, 0.35f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.offset, box.size);

        Gizmos.color = new Color(1f, 0.84f, 0f, 1f);
        Gizmos.DrawWireCube(box.offset, box.size);
    }
#endif
}