using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using YG;

public class YandexManager : MonoBehaviour
{
    private static YandexManager instance;

    void Awake()
    {
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
        SetupContextMenuBlocker();
        StartCoroutine(WaitForSDKAndReady());
    }

    void SetupContextMenuBlocker()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystem = eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            if (mainCamera.GetComponent<ContextMenuBlocker>() == null)
            {
                mainCamera.gameObject.AddComponent<ContextMenuBlocker>();
            }
        }
    }

    System.Collections.IEnumerator WaitForSDKAndReady()
    {
        yield return new WaitForSeconds(1f);

        YG2.onGetSDKData += OnSDKReady;

        if (YG2.isSDKEnabled)
        {
            OnSDKReady();
        }
    }

    void OnSDKReady()
    {
        Debug.Log("[YandexManager] SDK готов! Вызываю GameReadyAPI");
        YG2.GameReadyAPI();
        YG2.onGetSDKData -= OnSDKReady;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!YG2.isSDKEnabled) return;

        if (scene.name == "GameScene" || scene.name.Contains("Level"))
        {
            YG2.GameplayStart();
            Debug.Log($"[YandexManager] GameplayStart: {scene.name}");
        }
        else
        {
            YG2.GameplayStop();
            Debug.Log($"[YandexManager] GameplayStop: {scene.name}");
        }

        SetupContextMenuBlocker();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            AudioListener.pause = true;
            Debug.Log("[YandexManager] Игра свернута");
        }
        else
        {
            AudioListener.pause = false;
            Debug.Log("[YandexManager] Игра развернута");
        }
    }

    public class ContextMenuBlocker : MonoBehaviour
    {
        void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.f5Key.wasPressedThisFrame ||
                    Keyboard.current.f11Key.wasPressedThisFrame ||
                    Keyboard.current.f12Key.wasPressedThisFrame)
                {
                    // Блокируем системные клавиши
                }
            }

            if (Mouse.current != null)
            {
                if (Mouse.current.rightButton.wasPressedThisFrame)
                {
                    // Блокируем правый клик
                }
            }
        }

        void OnGUI()
        {
            if (Event.current != null && Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 1)
                {
                    Event.current.Use();
                }
            }
        }
    }
}