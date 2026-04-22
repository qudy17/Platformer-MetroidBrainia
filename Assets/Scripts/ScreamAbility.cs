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
    public float recoilForce = 10f;

    [Header("Смещение спавна волны")]
    [Tooltip("На сколько юнитов волна спавнится впереди игрока " +
             "чтобы не попадать в его коллайдер")]
    public float spawnOffset = 0.8f;

    private float[] restoreTimers;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

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
                             Keyboard.current.eKey.wasPressedThisFrame;

        if (screamPressed && currentCharges > 0)
        {
            PerformScream();
        }
    }

    void PerformScream()
    {
        Vector2 direction = playerMovement.FacingDirection;

        SpawnWave(direction);
        ApplyRecoil(direction);
        ConsumeCharge();

        // Лог после уменьшения заряда — показывает правильное число
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

        // Смещаем точку спавна вперёд — волна не попадает в коллайдер игрока
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
            wave.Initialize(direction, waveSpeed, waveMaxDistance);
        }
        else
        {
            Debug.LogError("[Scream] На prefab волны нет компонента SoundWave!");
        }
    }

    void ApplyRecoil(Vector2 screamDirection)
    {
        Vector2 recoilDir = -screamDirection;

        Debug.Log($"[Recoil] Направление отдачи: {recoilDir} | " +
                  $"Сила: {recoilForce} | " +
                  $"Скорость до: {rb.linearVelocity}");

        if (recoilDir.y > 0.3f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            Debug.Log("[Recoil] Сброс вертикальной скорости перед прыжком");
        }

        rb.AddForce(recoilDir * recoilForce, ForceMode2D.Impulse);

        Debug.Log($"[Recoil] Скорость после: {rb.linearVelocity}");
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