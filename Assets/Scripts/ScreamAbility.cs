using UnityEngine;
using UnityEngine.InputSystem;

public class ScreamAbility : MonoBehaviour
{
    [Header("Заряды")]
    public int maxCharges = 1;
    [SerializeField] private int currentCharges = 1;

    [Header("Восстановление заряда")]
    public float chargeRestoreTime = 0.8f;

    [Header("Параметры волны")]
    public GameObject soundWavePrefab;
    public float waveSpeed = 15f;
    public float waveMaxDistance = 20f;

    [Header("Отдача")]
    [Tooltip("Сила с которой поверхность отталкивает игрока")]
    public float recoilForce = 15f;

    [Header("Смещение спавна волны")]
    [Tooltip("На сколько юнитов волна спавнится впереди игрока")]
    public float spawnOffset = 0.8f;

    private float[] restoreTimers;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    // Публичное свойство — SoundWave возьмёт отсюда силу отдачи
    public float RecoilForce => recoilForce;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();

        if (playerMovement == null)
            Debug.LogError("[Scream] PlayerMovement не найден!");

        currentCharges = maxCharges;
        restoreTimers = new float[maxCharges];
    }

    void Update()
    {
        HandleInput();
        HandleRestore();
    }

    void HandleInput()
    {
        bool screamPressed = Keyboard.current != null &&
                             Keyboard.current.spaceKey.wasPressedThisFrame;

        if (screamPressed && currentCharges > 0)
        {
            PerformScream();
        }
    }

    void PerformScream()
    {
        Vector2 direction = playerMovement.FacingDirection;

        SpawnWave(direction);
        ConsumeCharge();

        Debug.Log($"[Scream] Крик в направлении: {direction}. " +
                  $"Зарядов осталось: {currentCharges}");
    }

    void SpawnWave(Vector2 direction)
    {
        if (soundWavePrefab == null)
        {
            Debug.LogError("[Scream] soundWavePrefab не назначен!");
            return;
        }

        Vector3 spawnPosition = transform.position +
                                (Vector3)(direction * spawnOffset);

        GameObject waveObj = Instantiate(
            soundWavePrefab,
            spawnPosition,
            Quaternion.identity
        );

        SoundWave wave = waveObj.GetComponent<SoundWave>();
        if (wave != null)
        {
            // Передаём волне: направление, скорость, дистанцию,
            // ссылку на Rigidbody игрока и силу отдачи
            wave.Initialize(
                direction,
                waveSpeed,
                waveMaxDistance,
                rb,
                recoilForce
            );
        }
        else
        {
            Debug.LogError("[Scream] На prefab волны нет компонента SoundWave!");
        }
    }

    void ConsumeCharge()
    {
        currentCharges--;

        for (int i = 0; i < restoreTimers.Length; i++)
        {
            if (restoreTimers[i] <= 0f)
            {
                restoreTimers[i] = chargeRestoreTime;
                break;
            }
        }
    }

    void HandleRestore()
    {
        for (int i = 0; i < restoreTimers.Length; i++)
        {
            if (restoreTimers[i] > 0f)
            {
                restoreTimers[i] -= Time.deltaTime;

                if (restoreTimers[i] <= 0f)
                {
                    restoreTimers[i] = 0f;

                    if (currentCharges < maxCharges)
                    {
                        currentCharges++;
                        Debug.Log($"[Scream] Заряд восстановлен. " +
                                  $"Текущие: {currentCharges}/{maxCharges}");
                    }
                }
            }
        }
    }
}