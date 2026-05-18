using UnityEngine;
// тест русского языка
/// <summary>
/// Отслеживает игровую статистику (время, смерти, колбы)
/// Добавьте на любой GameObject в игровой сцене
/// </summary>
public class GameStatsTracker : MonoBehaviour
{
    private static GameStatsTracker instance;

    private float sessionStartTime;
    private bool isTracking = false;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        StartTracking();
    }

    void Update()
    {
        if (isTracking)
        {
            // Обновляем время игры каждый кадр
            float currentPlayTime = PlayerPrefs.GetFloat("PlayTime", 0f);
            currentPlayTime += Time.deltaTime;
            PlayerPrefs.SetFloat("PlayTime", currentPlayTime);
        }
    }

    void OnApplicationQuit()
    {
        // Сохраняем при выходе
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Начать отслеживание статистики
    /// </summary>
    public void StartTracking()
    {
        isTracking = true;
        sessionStartTime = Time.time;
        Debug.Log("[GameStatsTracker] Отслеживание статистики начато");
    }

    /// <summary>
    /// Остановить отслеживание
    /// </summary>
    public void StopTracking()
    {
        isTracking = false;
        PlayerPrefs.Save();
        Debug.Log("[GameStatsTracker] Отслеживание статистики остановлено");
    }

    /// <summary>
    /// Зарегистрировать смерть игрока
    /// </summary>
    public static void RegisterDeath()
    {
        int deaths = PlayerPrefs.GetInt("Deaths", 0);
        deaths++;
        PlayerPrefs.SetInt("Deaths", deaths);
        PlayerPrefs.Save();
        Debug.Log($"[GameStatsTracker] Смерть зарегистрирована. Всего: {deaths}");
    }

    /// <summary>
    /// Зарегистрировать сбор колбы
    /// </summary>
    public static void RegisterFlaskCollected()
    {
        int flasks = PlayerPrefs.GetInt("FlasksCollected", 0);
        flasks++;
        PlayerPrefs.SetInt("FlasksCollected", flasks);
        PlayerPrefs.Save();
        Debug.Log($"[GameStatsTracker] Колба собрана. Всего: {flasks}");
    }

    /// <summary>
    /// Установить общее количество колб на уровне
    /// </summary>
    public static void SetTotalFlasks(int total)
    {
        PlayerPrefs.SetInt("TotalFlasks", total);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Сбросить всю статистику
    /// </summary>
    public static void ResetStats()
    {
        PlayerPrefs.SetFloat("PlayTime", 0f);
        PlayerPrefs.SetInt("Deaths", 0);
        PlayerPrefs.SetInt("FlasksCollected", 0);
        PlayerPrefs.Save();
        Debug.Log("[GameStatsTracker] Статистика сброшена");
    }
}