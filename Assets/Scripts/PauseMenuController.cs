using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument;

    [Header("Scene Settings")]
    [Tooltip("Название главной сцены меню")]
    public string mainMenuSceneName = "MainMenu";

    [Header("Audio Settings")]
    [Range(1f, 10f)]
    public float volumeStep = 5f;

    // Состояние паузы
    private bool isPaused = false;

    // UI элементы
    private VisualElement root;
    private VisualElement pauseMenuRoot;

    // Кнопки вкладок
    private Button resumeTabButton;
    private Button audioTabButton;
    private Button statisticsTabButton;
    private Button infoTabButton;

    // Панели
    private VisualElement resumePanel;
    private VisualElement audioSettings;
    private VisualElement statisticsSettings;
    private VisualElement infoSettings;

    // Кнопки действий
    private Button resumeButton;
    private Button exitToMenuButton;

    // Аудио элементы
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Button masterVolumeDown;
    private Button masterVolumeUp;
    private Button musicVolumeDown;
    private Button musicVolumeUp;

    // Статистика
    private Label timeStatText;
    private Label deathsStatText;
    private Label flasksStatText;

    // Текущая вкладка
    private enum PauseTab { Resume, Audio, Statistics, Info }
    private PauseTab currentTab = PauseTab.Resume;

    void OnEnable()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("[PauseMenuController] UIDocument не найден!");
            return;
        }

        root = uiDocument.rootVisualElement;

        InitializeElements();
        SubscribeToEvents();

        // Скрываем меню паузы при старте
        if (pauseMenuRoot != null)
            pauseMenuRoot.AddToClassList("hidden");

        LoadAudioSettings();

        Debug.Log("[PauseMenuController] Pause Menu инициализировано");
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void Update()
    {
        // Обработка нажатия ESC
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }

        // Обновляем статистику если меню открыто
        if (isPaused)
        {
            UpdateStatistics();
        }
    }

    #region Initialization

    void InitializeElements()
    {
        pauseMenuRoot = root.Q<VisualElement>("PauseMenuRoot");

        // Кнопки вкладок
        resumeTabButton = root.Q<Button>("ResumeTabButton");
        audioTabButton = root.Q<Button>("AudioTabButton");
        statisticsTabButton = root.Q<Button>("StatisticsTabButton");
        infoTabButton = root.Q<Button>("InfoTabButton");

        // Панели
        resumePanel = root.Q<VisualElement>("ResumePanel");
        audioSettings = root.Q<VisualElement>("AudioSettings");
        statisticsSettings = root.Q<VisualElement>("StatisticsSettings");
        infoSettings = root.Q<VisualElement>("InfoSettings");

        // Кнопки действий
        resumeButton = root.Q<Button>("ResumeButton");
        exitToMenuButton = root.Q<Button>("ExitToMenuButton");

        // Аудио
        masterVolumeSlider = root.Q<Slider>("MasterVolumeSlider");
        musicVolumeSlider = root.Q<Slider>("MusicVolumeSlider");
        masterVolumeDown = root.Q<Button>("MasterVolumeDown");
        masterVolumeUp = root.Q<Button>("MasterVolumeUp");
        musicVolumeDown = root.Q<Button>("MusicVolumeDown");
        musicVolumeUp = root.Q<Button>("MusicVolumeUp");

        // Статистика
        timeStatText = root.Q<Label>("TimeStatText");
        deathsStatText = root.Q<Label>("DeathsStatText");
        flasksStatText = root.Q<Label>("FlasksStatText");
    }

    #endregion

    #region Event Subscription

    void SubscribeToEvents()
    {
        // Вкладки
        if (resumeTabButton != null)
            resumeTabButton.clicked += () => SwitchTab(PauseTab.Resume);

        if (audioTabButton != null)
            audioTabButton.clicked += () => SwitchTab(PauseTab.Audio);

        if (statisticsTabButton != null)
            statisticsTabButton.clicked += () => SwitchTab(PauseTab.Statistics);

        if (infoTabButton != null)
            infoTabButton.clicked += () => SwitchTab(PauseTab.Info);

        // Кнопки действий
        if (resumeButton != null)
            resumeButton.clicked += ResumeGame;

        if (exitToMenuButton != null)
            exitToMenuButton.clicked += ExitToMainMenu;

        // Слайдеры
        if (masterVolumeSlider != null)
            masterVolumeSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);

        // Кнопки +/-
        if (masterVolumeDown != null)
            masterVolumeDown.clicked += () => AdjustVolume(masterVolumeSlider, -volumeStep);

        if (masterVolumeUp != null)
            masterVolumeUp.clicked += () => AdjustVolume(masterVolumeSlider, volumeStep);

        if (musicVolumeDown != null)
            musicVolumeDown.clicked += () => AdjustVolume(musicVolumeSlider, -volumeStep);

        if (musicVolumeUp != null)
            musicVolumeUp.clicked += () => AdjustVolume(musicVolumeSlider, volumeStep);
    }

    void UnsubscribeFromEvents()
    {
        if (resumeTabButton != null)
            resumeTabButton.clicked -= () => SwitchTab(PauseTab.Resume);

        if (audioTabButton != null)
            audioTabButton.clicked -= () => SwitchTab(PauseTab.Audio);

        if (statisticsTabButton != null)
            statisticsTabButton.clicked -= () => SwitchTab(PauseTab.Statistics);

        if (infoTabButton != null)
            infoTabButton.clicked -= () => SwitchTab(PauseTab.Info);

        if (resumeButton != null)
            resumeButton.clicked -= ResumeGame;

        if (exitToMenuButton != null)
            exitToMenuButton.clicked -= ExitToMainMenu;

        if (masterVolumeSlider != null)
            masterVolumeSlider.UnregisterValueChangedCallback(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.UnregisterValueChangedCallback(OnMusicVolumeChanged);
    }

    #endregion

    #region Pause Control

    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.RemoveFromClassList("hidden");
            SwitchTab(PauseTab.Resume);
        }

        Debug.Log("[PauseMenuController] Игра поставлена на паузу");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.AddToClassList("hidden");
        }

        SaveAudioSettings();

        Debug.Log("[PauseMenuController] Игра продолжена");
    }

    void ExitToMainMenu()
    {
        Debug.Log("[PauseMenuController] Выход в главное меню");

        // Возвращаем время перед загрузкой сцены
        Time.timeScale = 1f;

        SaveAudioSettings();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    #endregion

    #region Tab Switching

    void SwitchTab(PauseTab tab)
    {
        currentTab = tab;

        // Скрываем все панели
        if (resumePanel != null) resumePanel.AddToClassList("hidden");
        if (audioSettings != null) audioSettings.AddToClassList("hidden");
        if (statisticsSettings != null) statisticsSettings.AddToClassList("hidden");
        if (infoSettings != null) infoSettings.AddToClassList("hidden");

        // Убираем active со всех кнопок
        if (resumeTabButton != null) resumeTabButton.RemoveFromClassList("active");
        if (audioTabButton != null) audioTabButton.RemoveFromClassList("active");
        if (statisticsTabButton != null) statisticsTabButton.RemoveFromClassList("active");
        if (infoTabButton != null) infoTabButton.RemoveFromClassList("active");

        // Показываем нужную панель
        switch (tab)
        {
            case PauseTab.Resume:
                if (resumePanel != null) resumePanel.RemoveFromClassList("hidden");
                if (resumeTabButton != null) resumeTabButton.AddToClassList("active");
                break;

            case PauseTab.Audio:
                if (audioSettings != null) audioSettings.RemoveFromClassList("hidden");
                if (audioTabButton != null) audioTabButton.AddToClassList("active");
                break;

            case PauseTab.Statistics:
                if (statisticsSettings != null) statisticsSettings.RemoveFromClassList("hidden");
                if (statisticsTabButton != null) statisticsTabButton.AddToClassList("active");
                UpdateStatistics();
                break;

            case PauseTab.Info:
                if (infoSettings != null) infoSettings.RemoveFromClassList("hidden");
                if (infoTabButton != null) infoTabButton.AddToClassList("active");
                break;
        }
    }

    #endregion

    #region Audio Settings

    void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        AudioListener.volume = evt.newValue / 100f;
    }

    void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        float volume = evt.newValue / 100f;
        // TODO: Управление музыкой через AudioManager
    }

    void AdjustVolume(Slider slider, float delta)
    {
        if (slider == null) return;
        slider.value = Mathf.Clamp(slider.value + delta, slider.lowValue, slider.highValue);
    }

    void LoadAudioSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 100f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 100f);

        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVolume;

        AudioListener.volume = masterVolume / 100f;
    }

    void SaveAudioSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);

        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);

        PlayerPrefs.Save();
    }

    #endregion

    #region Statistics

    void UpdateStatistics()
    {
        if (timeStatText != null)
        {
            float playTime = PlayerPrefs.GetFloat("PlayTime", 0f);
            TimeSpan timeSpan = TimeSpan.FromSeconds(playTime);
            timeStatText.text = $"Time: {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        if (deathsStatText != null)
        {
            int deaths = PlayerPrefs.GetInt("Deaths", 0);
            deathsStatText.text = $"Deaths: {deaths}";
        }

        if (flasksStatText != null)
        {
            int flasksCollected = PlayerPrefs.GetInt("FlasksCollected", 0);
            int totalFlasks = PlayerPrefs.GetInt("TotalFlasks", 10);
            flasksStatText.text = $"Flasks: {flasksCollected}/{totalFlasks}";
        }
    }

    #endregion
}