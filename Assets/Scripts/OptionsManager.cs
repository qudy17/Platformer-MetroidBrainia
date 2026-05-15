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
    public MenuManager menuManager;

    private const float VOLUME_STEP = 0.05f;

    void Start()
    {
        LoadSettings();
        ShowSettings();

        // Слушатели для слайдеров
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        // Слушатели для кнопок
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

    void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        masterVolumeSlider.value = masterVolume;
        musicVolumeSlider.value = musicVolume;

        AudioListener.volume = masterVolume;

        UpdateVolumeText();
    }

    // === Master Volume ===
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

    // === Music Volume ===
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

    public void ShowStatistics()
    {
        currentState = OverlayState.Statistics;
        settingsPanel.SetActive(false);
        statisticsPanel.SetActive(true);
        teamPanel.SetActive(false);

        UpdateStatistics();
    }

    public void ShowTeam()
    {
        currentState = OverlayState.Team;
        settingsPanel.SetActive(false);
        statisticsPanel.SetActive(false);
        teamPanel.SetActive(true);
    }

    void UpdateStatistics()
    {
        float gameTime = PlayerPrefs.GetFloat("GameTime", 0f);
        int deathCount = PlayerPrefs.GetInt("DeathCount", 0);

        int hours = Mathf.FloorToInt(gameTime / 3600);
        int minutes = Mathf.FloorToInt((gameTime % 3600) / 60);
        int seconds = Mathf.FloorToInt(gameTime % 60);

        gameTimeText.text = $"Время в игре: {hours:00}:{minutes:00}:{seconds:00}";
        deathCountText.text = $"Количество смертей: {deathCount}";
    }

    public void CloseOptions()
    {
        ShowSettings();
        menuManager.CloseOptions();
    }
}