using UnityEngine;
using System.Collections;
// тест русского языка
public class ResonancePipe : MonoBehaviour
{
    public enum PipeType
    {
        Input,
        Output
    }

    [Header("Тип трубы")]
    public PipeType pipeType = PipeType.Input;

    [Header("Связь")]
    [Tooltip("Уникальный ID пары труб. Должен совпадать у входа и выхода")]
    public string pipeID = "Pipe_1";

    [Header("Настройки задержки")]
    [Tooltip("Задержка перед излучением волны из выхода")]
    public float delay = 0.5f;

    [Header("Настройки выхода")]
    [Tooltip("Направление излучения волны (только для Output)")]
    public Vector2 outputDirection = Vector2.right;

    [Header("Отладка")]
    [Tooltip("Показывать связь в редакторе")]
    public bool showConnectionGizmo = true;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isActive = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (pipeType != PipeType.Input) return;
        if (isActive) return;

        SoundWave wave = other.GetComponent<SoundWave>();
        if (wave != null)
        {
            // Проверяем, с правильной ли стороны пришла волна
            if (IsCorrectDirection(wave))
            {
                ReceiveWave(wave);
            }
            else
            {
                Debug.Log($"[ResonancePipe] Вход '{pipeID}': Волна с неправильной стороны — уничтожаю.");
                Destroy(wave.gameObject);
            }
        }
    }

    bool IsCorrectDirection(SoundWave wave)
    {
        // Получаем направление волны
        Vector2 waveDirection = wave.GetDirection();

        // Вход должен быть СВЕРХУ-ВНИЗ относительно спрайта
        // transform.up — это "верх" спрайта
        // Нам нужно, чтобы волна летела ПРОТИВОПОЛОЖНО transform.up (т.е. вниз)
        Vector2 inputAcceptDirection = -transform.up;

        // Сравниваем направления
        float dotProduct = Vector2.Dot(waveDirection, inputAcceptDirection);

        // Если dotProduct > 0.5f — волна летит примерно в правильном направлении
        bool isCorrect = dotProduct > 0.5f;

        Debug.Log($"[ResonancePipe] Вход '{pipeID}': " +
                  $"Волна: {waveDirection}, Ожидается: {inputAcceptDirection}, " +
                  $"Dot: {dotProduct:F2}, Принято: {isCorrect}");

        return isCorrect;
    }

    void ReceiveWave(SoundWave wave)
    {
        Debug.Log($"[ResonancePipe] Вход '{pipeID}': Волна получена!");

        // СНАЧАЛА сохраняем параметры
        SaveWaveParameters(wave);

        // ПОТОМ уничтожаем волну
        Destroy(wave.gameObject);

        // Активируем визуал
        isActive = true;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }

        // Запускаем таймер
        StartCoroutine(DelayedEmit());
    }

    // Сохраняем параметры волны
    private Vector2 savedWaveDirection;
    private float savedWaveSpeed;
    private float savedWaveMaxDistance;
    private Rigidbody2D savedPlayerRb;
    private float savedRecoilForce;

    void SaveWaveParameters(SoundWave wave)
    {
        // Получаем параметры через рефлексию или публичные методы
        // Нужно добавить геттеры в SoundWave
        savedWaveDirection = wave.GetDirection();
        savedWaveSpeed = wave.GetSpeed();
        savedWaveMaxDistance = wave.GetMaxDistance();
        savedPlayerRb = wave.GetPlayerRb();
        savedRecoilForce = wave.GetRecoilForce();
    }

    IEnumerator DelayedEmit()
    {
        Debug.Log($"[ResonancePipe] Вход '{pipeID}': Задержка {delay} сек...");

        yield return new WaitForSeconds(delay);

        ResonancePipe outputPipe = FindOutputPipe(pipeID);

        if (outputPipe != null && outputPipe != this)
        {
            Debug.Log($"[ResonancePipe] Выход '{pipeID}': Излучаю волну!");
            outputPipe.EmitWave(savedWaveDirection, savedWaveSpeed, savedWaveMaxDistance,
                               savedPlayerRb, savedRecoilForce);
        }
        else
        {
            Debug.LogWarning($"[ResonancePipe] Выход с ID '{pipeID}' не найден!");
        }

        isActive = false;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    ResonancePipe FindOutputPipe(string id)
    {
        ResonancePipe[] allPipes = FindObjectsByType<ResonancePipe>(FindObjectsSortMode.None);

        foreach (var pipe in allPipes)
        {
            if (pipe.pipeID == id && pipe.pipeType == PipeType.Output && pipe != this)
            {
                return pipe;
            }
        }

        return null;
    }

    public void EmitWave(Vector2 direction, float speed, float maxDistance, Rigidbody2D playerRb, float recoilForce)
    {
        ScreamAbility screamAbility = FindFirstObjectByType<ScreamAbility>();
        if (screamAbility == null) return;

        GameObject wavePrefab = screamAbility.soundWavePrefab;
        if (wavePrefab == null) return;

        Vector2 emitDirection = outputDirection.normalized;
        Vector3 spawnPos = transform.position + (Vector3)(emitDirection * 0.8f);
        GameObject waveObj = Instantiate(wavePrefab, spawnPos, Quaternion.identity);

        SoundWave wave = waveObj.GetComponent<SoundWave>();
        if (wave != null)
        {
            wave.Initialize(
                emitDirection,
                screamAbility.waveSpeed,
                screamAbility.waveMaxDistance,
                playerRb,
                screamAbility.recoilForce
            );

            Debug.Log($"[ResonancePipe] Волна создана с дистанцией {screamAbility.waveMaxDistance}");
        }

        StartCoroutine(FlashOutput());
    }

    IEnumerator FlashOutput()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.cyan;
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = originalColor;
        }
    }

    void OnDrawGizmos()
    {
        if (pipeType == PipeType.Input)
        {
            // Вход — показываем КУДА должна лететь волна
            Gizmos.color = isActive ? Color.yellow : Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);

            // Стрелка ПРИНИМАЕМОГО направления (противоположно transform.up)
            Gizmos.color = Color.green;
            Vector3 acceptDir = transform.up;
            Gizmos.DrawLine(transform.position, transform.position + acceptDir * 1.5f);
            Gizmos.DrawWireSphere(transform.position + acceptDir * 1.5f, 0.1f);
        }
        else
        {
            // Выход — показываем направление излучения
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.8f);

            Gizmos.color = Color.cyan;
            Vector3 dir = outputDirection.normalized;
            Gizmos.DrawLine(transform.position, transform.position + dir * 1.5f);
            Gizmos.DrawWireSphere(transform.position + dir * 1.5f, 0.1f);
        }

        // Линия связи между трубами
        if (showConnectionGizmo)
        {
            ResonancePipe[] allPipes = FindObjectsByType<ResonancePipe>(FindObjectsSortMode.None);
            foreach (var pipe in allPipes)
            {
                if (pipe != this && pipe.pipeID == this.pipeID && pipe.pipeType != this.pipeType)
                {
                    Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
                    Gizmos.DrawLine(transform.position, pipe.transform.position);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 1f,
            pipeType == PipeType.Input ?
                $"INPUT\nID: {pipeID}\nAccept: {-transform.up}" :
                $"OUTPUT\nID: {pipeID}\nEmit: {outputDirection}"
        );
#endif
    }
}