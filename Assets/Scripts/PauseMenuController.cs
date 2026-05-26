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
    private bool isExitOverlayOpen = false;

    // UI элементы
    private VisualElement root;
    private VisualElement pauseMenuRoot;

    // Кнопка меню
    private Button menuButton;

    // Кнопки вкладок
    private Button audioTabButton;
    private Button statisticsTabButton;
    private Button infoTabButton;

    // Панели
    private VisualElement audioSettings;
    private VisualElement statisticsSettings;
    private VisualElement infoSettings;

    // Exit Overlay
    private VisualElement exitOverlay;
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
    private enum PauseTab { Audio, Statistics, Info }
    private PauseTab currentTab = PauseTab.Audio;

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

        // Скрываем оверлей при старте
        if (exitOverlay != null)
            exitOverlay.AddToClassList("hidden");

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
            if (isExitOverlayOpen)
            {
                // Если оверлей открыт - закрываем его
                CloseExitOverlay();
            }
            else if (isPaused)
            {
                // Если пауза открыта - возобновляем игру
                ResumeGame();
            }
            else
            {
                // Иначе открываем паузу
                PauseGame();
            }
        }

        // Обновляем статистику если меню открыто
        if (isPaused && !isExitOverlayOpen)
        {
            UpdateStatistics();
        }
    }

    #region Initialization

    void InitializeElements()
    {
        pauseMenuRoot = root.Q<VisualElement>("PauseMenuRoot");

        // Кнопка меню
        menuButton = root.Q<Button>("MenuButton");

        // Кнопки вкладок
        audioTabButton = root.Q<Button>("AudioTabButton");
        statisticsTabButton = root.Q<Button>("StatisticsTabButton");
        infoTabButton = root.Q<Button>("InfoTabButton");

        // Панели
        audioSettings = root.Q<VisualElement>("AudioSettings");
        statisticsSettings = root.Q<VisualElement>("StatisticsSettings");
        infoSettings = root.Q<VisualElement>("InfoSettings");

        // Exit Overlay
        exitOverlay = root.Q<VisualElement>("ExitOverlay");
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
        // Кнопка меню
        if (menuButton != null)
            menuButton.clicked += OnMenuButtonClicked;

        // Вкладки
        if (audioTabButton != null)
            audioTabButton.clicked += () => SwitchTab(PauseTab.Audio);

        if (statisticsTabButton != null)
            statisticsTabButton.clicked += () => SwitchTab(PauseTab.Statistics);

        if (infoTabButton != null)
            infoTabButton.clicked += () => SwitchTab(PauseTab.Info);

        // Кнопки Exit Overlay
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
        if (menuButton != null)
            menuButton.clicked -= OnMenuButtonClicked;

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
            SwitchTab(PauseTab.Audio); // Открываем Audio по умолчанию
        }

        Debug.Log("[PauseMenuController] Игра поставлена на паузу");
    }

    public void ResumeGame()
    {
        // Закрываем оверлей если он был открыт
        if (isExitOverlayOpen)
        {
            CloseExitOverlay();
        }

        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuRoot != null)
        {
            pauseMenuRoot.AddToClassList("hidden");
        }

        SaveAudioSettings();

        Debug.Log("[PauseMenuController] Игра продолжена");
    }

    void OnMenuButtonClicked()
    {
        Debug.Log("[PauseMenuController] Кнопка меню нажата");
        OpenExitOverlay();
    }

    void OpenExitOverlay()
    {
        isExitOverlayOpen = true;

        if (exitOverlay != null)
        {
            exitOverlay.RemoveFromClassList("hidden");
        }

        Debug.Log("[PauseMenuController] Exit Overlay открыт");
    }

    void CloseExitOverlay()
    {
        isExitOverlayOpen = false;

        if (exitOverlay != null)
        {
            exitOverlay.AddToClassList("hidden");
        }

        Debug.Log("[PauseMenuController] Exit Overlay закрыт");
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
        if (audioSettings != null) audioSettings.AddToClassList("hidden");
        if (statisticsSettings != null) statisticsSettings.AddToClassList("hidden");
        if (infoSettings != null) infoSettings.AddToClassList("hidden");

        // Убираем active со всех кнопок
        if (audioTabButton != null) audioTabButton.RemoveFromClassList("active");
        if (statisticsTabButton != null) statisticsTabButton.RemoveFromClassList("active");
        if (infoTabButton != null) infoTabButton.RemoveFromClassList("active");

        // Показываем нужную панель
        switch (tab)
        {
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