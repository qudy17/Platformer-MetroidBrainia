using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
// тест русского языка
public class PauseManager : MonoBehaviour
{
    [Header("Pause Menu")]
    public GameObject pauseOverlay;
    public GameObject gameUI; // Ваш игровой UI (опционально)

    [Header("References")]
    public OptionsManager optionsManager;

    private bool isPaused = false;

    void Start()
    {
        // Убеждаемся, что игра не на паузе при старте
        Resume();
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Если оверлей настроек открыт, пусть OptionsManager обработает ESC
            if (pauseOverlay.activeSelf && !IsInMainPauseMenu())
            {
                return; // OptionsManager сам обработает
            }

            // Переключаем паузу
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    bool IsInMainPauseMenu()
    {
        // Проверяем, находимся ли мы в главном меню паузы
        return optionsManager != null && optionsManager.IsInSettingsPanel();
    }

    public void Pause()
    {
        isPaused = true;
        pauseOverlay.SetActive(true);

        if (gameUI != null)
            gameUI.SetActive(false);

        Time.timeScale = 0f; // Останавливаем игру

        // Показываем панель настроек по умолчанию
        if (optionsManager != null)
            optionsManager.ShowSettings();
    }

    public void Resume()
    {
        isPaused = false;
        pauseOverlay.SetActive(false);

        if (gameUI != null)
            gameUI.SetActive(true);

        Time.timeScale = 1f; // Возобновляем игру
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Важно! Сбрасываем timeScale перед сменой сцены
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}