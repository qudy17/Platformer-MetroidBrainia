using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Отслеживает игровую статистику (время, смерти, колбы)
/// Добавьте на любой GameObject в игровой сцене
/// </summary>
public class GameStatsTracker : MonoBehaviour
{
    private static GameStatsTracker instance;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene"; // Укажите имя вашей игровой сцены

    private float sessionStartTime;
    private bool isTracking = false;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            // Подписываемся на события смены сцены
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Проверяем текущую сцену при старте
        CheckCurrentScene();
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

    void OnDestroy()
    {
        // Отписываемся от событий
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnApplicationQuit()
    {
        // Сохраняем при выходе
        StopTracking();
    }

    /// <summary>
    /// Обработчик загрузки сцены
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckCurrentScene();
    }

    /// <summary>
    /// Проверяет текущую сцену и запускает/останавливает отслеживание
    /// </summary>
    private void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == gameSceneName)
        {
            StartTracking();
        }
        else
        {
            StopTracking();
        }
    }

    /// <summary>
    /// Начать отслеживание статистики
    /// </summary>
    public void StartTracking()
    {
        if (!isTracking)
        {
            isTracking = true;
            sessionStartTime = Time.time;
            Debug.Log("[GameStatsTracker] Отслеживание статистики начато");
        }
    }

    /// <summary>
    /// Остановить отслеживание
    /// </summary>
    public void StopTracking()
    {
        if (isTracking)
        {
            isTracking = false;
            PlayerPrefs.Save();
            Debug.Log("[GameStatsTracker] Отслеживание статистики остановлено");
        }
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