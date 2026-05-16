using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    private enum OverlayState
    {
        Settings,
        Statistics,
        Team
    }

    private OverlayState currentState = OverlayState.Settings;

    [Header("Audio Settings")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI masterVolumeText;
    public TextMeshProUGUI musicVolumeText;

    [Header("Volume Buttons")]
    public Button masterVolumeDecreaseButton;
    public Button masterVolumeIncreaseButton;
    public Button musicVolumeDecreaseButton;
    public Button musicVolumeIncreaseButton;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject statisticsPanel;
    public GameObject teamPanel;

    [Header("Statistics")]
    public TextMeshProUGUI gameTimeText;
    public TextMeshProUGUI deathCountText;

    [Header("References")]
    public MenuManager menuManager; // Для главного меню
    public PauseManager pauseManager; // Для игровой сцены

    private const float VOLUME_STEP = 0.05f;

    void Start()
    {
        LoadSettings();
        ShowSettings();

        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        masterVolumeDecreaseButton.onClick.AddListener(DecreaseMasterVolume);
        masterVolumeIncreaseButton.onClick.AddListener(IncreaseMasterVolume);
        musicVolumeDecreaseButton.onClick.AddListener(DecreaseMusicVolume);
        musicVolumeIncreaseButton.onClick.AddListener(IncreaseMusicVolume);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            HandleEscapeKey();
        }
    }

    void HandleEscapeKey()
    {
        switch (currentState)
        {
            case OverlayState.Statistics:
            case OverlayState.Team:
                ShowSettings();
                break;

            case OverlayState.Settings:
                CloseOptions();
                break;
        }
    }

    // Публичный метод для проверки состояния
    public bool IsInSettingsPanel()
    {
        return currentState == OverlayState.Settings;
    }

    void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        masterVolumeSlider.value = masterVolume;
        musicVolumeSlider.value = musicVolume;

        AudioListener.volume = masterVolume;

        UpdateVolumeText();
    }

    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        UpdateVolumeText();
    }

    public void DecreaseMasterVolume()
    {
        float newValue = Mathf.Clamp(masterVolumeSlider.value - VOLUME_STEP, 0f, 1f);
        masterVolumeSlider.value = newValue;
    }

    public void IncreaseMasterVolume()
    {
        float newValue = Mathf.Clamp(masterVolumeSlider.value + VOLUME_STEP, 0f, 1f);
        masterVolumeSlider.value = newValue;
    }

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat("MusicVolume", volume);
        UpdateVolumeText();
    }

    public void DecreaseMusicVolume()
    {
        float newValue = Mathf.Clamp(musicVolumeSlider.value - VOLUME_STEP, 0f, 1f);
        musicVolumeSlider.value = newValue;
    }

    public void IncreaseMusicVolume()
    {
        float newValue = Mathf.Clamp(musicVolumeSlider.value + VOLUME_STEP, 0f, 1f);
        musicVolumeSlider.value = newValue;
    }

    void UpdateVolumeText()
    {
        if (masterVolumeText != null)
            masterVolumeText.text = Mathf.RoundToInt(masterVolumeSlider.value * 100) + "%";

        if (musicVolumeText != null)
            musicVolumeText.text = Mathf.RoundToInt(musicVolumeSlider.value * 100) + "%";
    }

    public void ShowSettings()
    {
        currentState = OverlayState.Settings;
        settingsPanel.SetActive(true);
        statisticsPanel.SetActive(false);
        teamPanel.SetActive(false);
    }
    public void ShowTeam()
    {
        currentState = OverlayState.Team;
        settingsPanel.SetActive(false);
        statisticsPanel.SetActive(false);
        teamPanel.SetActive(true);
    }

    public void ShowStatistics()
    {
        currentState = OverlayState.Statistics;
        settingsPanel.SetActive(false);
        statisticsPanel.SetActive(true);
        teamPanel.SetActive(false);

        Debug.Log("ShowStatistics called");
        UpdateStatistics();
    }

    void UpdateStatistics()
    {
        float gameTime = PlayerPrefs.GetFloat("GameTime", 0f);
        int deathCount = PlayerPrefs.GetInt("DeathCount", 0);

        Debug.Log($"UpdateStatistics: GameTime={gameTime}, DeathCount={deathCount}");

        int hours = Mathf.FloorToInt(gameTime / 3600);
        int minutes = Mathf.FloorToInt((gameTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);

        string timeString = $"Время в игре: {hours:00}:{minutes:00}:{seconds:00}";
        string deathString = $"Количество смертей: {deathCount}";

        Debug.Log($"Time string: {timeString}");
        Debug.Log($"Death string: {deathString}");

        if (gameTimeText != null)
        {
            gameTimeText.text = timeString;
            Debug.Log("GameTime text updated");
        }
        else
        {
            Debug.LogError("gameTimeText is NULL!");
        }

        if (deathCountText != null)
        {
            deathCountText.text = deathString;
            Debug.Log("DeathCount text updated");
        }
        else
        {
            Debug.LogError("deathCountText is NULL!");
        }
    }

    public void CloseOptions()
    {
        ShowSettings();

        // Проверяем, в какой сцене мы находимся
        if (menuManager != null)
        {
            menuManager.CloseOptions();
        }
        else if (pauseManager != null)
        {
            pauseManager.Resume();
        }
    }
}