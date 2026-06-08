using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
public class MainMenuController : MonoBehaviour
{
    [Header("UI Document")]
    [Tooltip("UIDocument компонент с главным меню")]
    public UIDocument uiDocument;

    [Header("Scene Settings")]
    [Tooltip("Название сцены для загрузки при нажатии Start Game")]
    public string gameSceneName = "GameScene";

    [Header("Audio Settings")]
    [Tooltip("Шаг изменения громкости при нажатии +/-")]
    [Range(1f, 10f)]
    public float volumeStep = 5f;

    // Корневой элемент
    private VisualElement root;

    // Главное меню
    private Button startGameButton;
    private Button optionsButton;

    // Меню настроек
    private VisualElement optionsMenu;
    private Button closeOptionsButton;

    // Кнопки переключения вкладок
    private Button audioTabButton;
    private Button statisticsTabButton;
    private Button infoTabButton;

    // Панели настроек
    private VisualElement audioSettings;
    private VisualElement statisticsSettings;
    private VisualElement infoSettings;

    // Аудио элементы
    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Button masterVolumeDown;
    private Button masterVolumeUp;
    private Button musicVolumeDown;
    private Button musicVolumeUp;

    // Статистика элементы
    private Label timeStatText;
    private Label deathsStatText;
    private Label flasksStatText;

    // Текущая активная вкладка
    private enum OptionsTab { Audio, Statistics, Info }
    private OptionsTab currentTab = OptionsTab.Audio;

    void OnEnable()
    {
        // Проверяем какая сейчас сцена
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            Debug.Log($"[MainMenuController] Не MainMenu, отключаю UI. Текущая сцена: {SceneManager.GetActiveScene().name}");
            // Отключаем весь UI
            if (uiDocument != null)
            {
                uiDocument.enabled = false;
            }
            this.enabled = false;
            return;
        }

        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (uiDocument == null)
        {
            Debug.LogError("[MainMenuController] UIDocument не найден!");
            return;
        }

        root = uiDocument.rootVisualElement;

        InitializeMainMenuElements();
        InitializeOptionsElements();
        InitializeAudioElements();
        InitializeStatisticsElements();

        SubscribeToEvents();

        // Убеждаемся, что меню настроек скрыто
        if (optionsMenu != null)
        {
            optionsMenu.AddToClassList("hidden");
        }

        // Устанавливаем начальные значения
        LoadAudioSettings();

        Debug.Log("[MainMenuController] Главное меню инициализировано");
    }

    void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    void Update()
    {
        // Обновляем статистику в реальном времени (если меню настроек открыто)
        if (optionsMenu != null && !optionsMenu.ClassListContains("hidden"))
        {
            UpdateStatistics();
        }
    }

    #region Initialization

    void InitializeMainMenuElements()
    {
        startGameButton = root.Q<Button>("StartGameButton");
        optionsButton = root.Q<Button>("OptionsButton");

        if (startGameButton == null)
            Debug.LogError("[MainMenuController] StartGameButton не найдена!");

        if (optionsButton == null)
            Debug.LogError("[MainMenuController] OptionsButton не найдена!");
    }

    void InitializeOptionsElements()
    {
        optionsMenu = root.Q<VisualElement>("OptionsMenu");
        closeOptionsButton = root.Q<Button>("CloseOptionsButton");

        // Кнопки переключения вкладок
        audioTabButton = root.Q<Button>("AudioTabButton");
        statisticsTabButton = root.Q<Button>("StatisticsTabButton");
        infoTabButton = root.Q<Button>("InfoTabButton");

        // Панели настроек
        audioSettings = root.Q<VisualElement>("AudioSettings");
        statisticsSettings = root.Q<VisualElement>("StatisticsSettings");
        infoSettings = root.Q<VisualElement>("InfoSettings");

        if (optionsMenu == null)
            Debug.LogError("[MainMenuController] OptionsMenu не найдено!");
    }

    void InitializeAudioElements()
    {
        // Слайдеры
        masterVolumeSlider = root.Q<Slider>("MasterVolumeSlider");
        musicVolumeSlider = root.Q<Slider>("MusicVolumeSlider");


        if (masterVolumeSlider != null)
            masterVolumeSlider.fill = true;

        if (musicVolumeSlider != null)
            musicVolumeSlider.fill = true;


        // Кнопки +/-
        masterVolumeDown = root.Q<Button>("MasterVolumeDown");
        masterVolumeUp = root.Q<Button>("MasterVolumeUp");
        musicVolumeDown = root.Q<Button>("MusicVolumeDown");
        musicVolumeUp = root.Q<Button>("MusicVolumeUp");
    }

    void InitializeStatisticsElements()
    {
        timeStatText = root.Q<Label>("TimeStatText");
        deathsStatText = root.Q<Label>("DeathsStatText");
        flasksStatText = root.Q<Label>("FlasksStatText");
    }

    #endregion

    #region Event Subscription

    void SubscribeToEvents()
    {
        // Главное меню
        if (startGameButton != null)
            startGameButton.clicked += OnStartGameClicked;

        if (optionsButton != null)
            optionsButton.clicked += OnOptionsClicked;

        // Закрытие настроек
        if (closeOptionsButton != null)
            closeOptionsButton.clicked += OnCloseOptionsClicked;

        // Переключение вкладок
        if (audioTabButton != null)
            audioTabButton.clicked += () => SwitchTab(OptionsTab.Audio);

        if (statisticsTabButton != null)
            statisticsTabButton.clicked += () => SwitchTab(OptionsTab.Statistics);

        if (infoTabButton != null)
            infoTabButton.clicked += () => SwitchTab(OptionsTab.Info);

        // Слайдеры громкости
        if (masterVolumeSlider != null)
            masterVolumeSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);

        // Кнопки +/- для Master Volume
        if (masterVolumeDown != null)
            masterVolumeDown.clicked += () => AdjustVolume(masterVolumeSlider, -volumeStep);

        if (masterVolumeUp != null)
            masterVolumeUp.clicked += () => AdjustVolume(masterVolumeSlider, volumeStep);

        // Кнопки +/- для Music Volume
        if (musicVolumeDown != null)
            musicVolumeDown.clicked += () => AdjustVolume(musicVolumeSlider, -volumeStep);

        if (musicVolumeUp != null)
            musicVolumeUp.clicked += () => AdjustVolume(musicVolumeSlider, volumeStep);
    }

    void UnsubscribeFromEvents()
    {
        if (startGameButton != null)
            startGameButton.clicked -= OnStartGameClicked;

        if (optionsButton != null)
            optionsButton.clicked -= OnOptionsClicked;

        if (closeOptionsButton != null)
            closeOptionsButton.clicked -= OnCloseOptionsClicked;

        if (audioTabButton != null)
            audioTabButton.clicked -= () => SwitchTab(OptionsTab.Audio);

        if (statisticsTabButton != null)
            statisticsTabButton.clicked -= () => SwitchTab(OptionsTab.Statistics);

        if (infoTabButton != null)
            infoTabButton.clicked -= () => SwitchTab(OptionsTab.Info);

        if (masterVolumeSlider != null)
            masterVolumeSlider.UnregisterValueChangedCallback(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.UnregisterValueChangedCallback(OnMusicVolumeChanged);
    }

    #endregion

    #region Main Menu Events

    void OnStartGameClicked()
    {
        Debug.Log("[MainMenuController] Start Game нажата");

        // СНАЧАЛА скрываем UI
        if (uiDocument != null)
        {
            uiDocument.rootVisualElement.style.display = DisplayStyle.None;
        }

        // ПОТОМ загружаем сцену
        StartCoroutine(LoadGameScene());
    }

    IEnumerator LoadGameScene()
    {
        // Ждем кадр чтобы UI скрылся
        yield return null;

        Debug.Log("[MainMenuController] Загружаю GameScene...");
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    void OnOptionsClicked()
    {
        Debug.Log("[MainMenuController] Options нажата - открываю меню настроек");

        if (optionsMenu != null)
        {
            optionsMenu.RemoveFromClassList("hidden");
            SwitchTab(OptionsTab.Audio); // Открываем вкладку Audio по умолчанию
        }
    }

    void OnCloseOptionsClicked()
    {
        Debug.Log("[MainMenuController] Закрываю меню настроек");

        if (optionsMenu != null)
        {
            optionsMenu.AddToClassList("hidden");
        }

        // Сохраняем настройки при закрытии
        SaveAudioSettings();
    }

    #endregion

    #region Tab Switching

    void SwitchTab(OptionsTab tab)
    {
        currentTab = tab;

        // Скрываем все панели
        if (audioSettings != null)
            audioSettings.AddToClassList("hidden");

        if (statisticsSettings != null)
            statisticsSettings.AddToClassList("hidden");

        if (infoSettings != null)
            infoSettings.AddToClassList("hidden");

        // Убираем active класс со всех кнопок
        if (audioTabButton != null)
            audioTabButton.RemoveFromClassList("active");

        if (statisticsTabButton != null)
            statisticsTabButton.RemoveFromClassList("active");

        if (infoTabButton != null)
            infoTabButton.RemoveFromClassList("active");

        // Показываем нужную панель и активируем кнопку
        switch (tab)
        {
            case OptionsTab.Audio:
                if (audioSettings != null)
                    audioSettings.RemoveFromClassList("hidden");
                if (audioTabButton != null)
                    audioTabButton.AddToClassList("active");
                Debug.Log("[MainMenuController] Переключено на вкладку Audio");
                break;

            case OptionsTab.Statistics:
                if (statisticsSettings != null)
                    statisticsSettings.RemoveFromClassList("hidden");
                if (statisticsTabButton != null)
                    statisticsTabButton.AddToClassList("active");
                UpdateStatistics(); // Обновляем статистику при открытии
                Debug.Log("[MainMenuController] Переключено на вкладку Statistics");
                break;

            case OptionsTab.Info:
                if (infoSettings != null)
                    infoSettings.RemoveFromClassList("hidden");
                if (infoTabButton != null)
                    infoTabButton.AddToClassList("active");
                Debug.Log("[MainMenuController] Переключено на вкладку Info");
                break;
        }
    }

    #endregion

    #region Audio Settings

    void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        float volume = evt.newValue / 100f; // Конвертируем 0-100 в 0-1
        AudioListener.volume = volume;
        Debug.Log($"[MainMenuController] Master Volume изменен: {evt.newValue}%");
    }

    void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        float volume = evt.newValue / 100f;

        // TODO: Здесь можно управлять громкостью музыки через AudioSource
        // Например, если у вас есть GameManager или AudioManager:
        // AudioManager.Instance.SetMusicVolume(volume);

        Debug.Log($"[MainMenuController] Music Volume изменен: {evt.newValue}%");
    }

    void AdjustVolume(Slider slider, float delta)
    {
        if (slider == null) return;

        float newValue = Mathf.Clamp(slider.value + delta, slider.lowValue, slider.highValue);
        slider.value = newValue;
    }

    void LoadAudioSettings()
    {
        // Загружаем сохраненные настройки из PlayerPrefs
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 100f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 100f);

        if (masterVolumeSlider != null)
            masterVolumeSlider.value = masterVolume;

        if (musicVolumeSlider != null)
            musicVolumeSlider.value = musicVolume;

        // Применяем громкость
        AudioListener.volume = masterVolume / 100f;

        Debug.Log($"[MainMenuController] Настройки аудио загружены: Master={masterVolume}%, Music={musicVolume}%");
    }

    void SaveAudioSettings()
    {
        if (masterVolumeSlider != null)
            PlayerPrefs.SetFloat("MasterVolume", masterVolumeSlider.value);

        if (musicVolumeSlider != null)
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);

        PlayerPrefs.Save();

        Debug.Log("[MainMenuController] Настройки аудио сохранены");
    }

    #endregion

    #region Statistics

    void UpdateStatistics()
    {
        // Время игры
        if (timeStatText != null)
        {
            float playTime = PlayerPrefs.GetFloat("PlayTime", 0f);
            TimeSpan timeSpan = TimeSpan.FromSeconds(playTime);
            timeStatText.text = $"{timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        // Количество смертей
        if (deathsStatText != null)
        {
            int deaths = PlayerPrefs.GetInt("Deaths", 0);
            deathsStatText.text = $"{deaths}";
        }

        // Количество колб (заглушка)
        if (flasksStatText != null)
        {
            int flasksCollected = PlayerPrefs.GetInt("FlasksCollected", 0);
            int totalFlasks = PlayerPrefs.GetInt("TotalFlasks", 1);
            flasksStatText.text = $"{flasksCollected}/{totalFlasks}";
        }
    }

    #endregion
}