using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsOverlay;

    void Start()
    {
        ShowMainMenu();
    }

    void Update()
    {
    }

    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsOverlay.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsOverlay.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
        if (optionsOverlay != null)
            optionsOverlay.SetActive(false);
    }
}