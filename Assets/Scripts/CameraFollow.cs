using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Настройки")]
    public Transform player;
    public float transitionSpeed = 3f;
    public float transitionThreshold = 0.01f;

    [Header("Текущая комната (только для просмотра)")]
    [SerializeField] private Room currentRoom;

    // Приватные переменные
    private Vector3 targetPosition;
    private bool isTransitioning = false;
    private float cameraZ = -10f;
    private bool isInitialized = false;

    void Awake()
    {
        // ВАЖНО: Инициализация в Awake, ДО Start других скриптов
        cameraZ = transform.position.z;

        // Сразу находим комнату и телепортируем камеру
        FindCurrentRoom();

        if (currentRoom != null)
        {
            // Мгновенно устанавливаем позицию камеры БЕЗ анимации
            Vector3 roomCenter = new Vector3(
                currentRoom.RoomCenter.x,
                currentRoom.RoomCenter.y,
                cameraZ
            );
            transform.position = roomCenter;
            targetPosition = roomCenter;
            isTransitioning = false;

            Debug.Log($"[CameraFollow] Камера инициализирована в комнате: {currentRoom.name} на позиции {roomCenter}");
        }

        isInitialized = true;
    }

    void Start()
    {
        // Дополнительная проверка на случай если Awake не сработал
        if (!isInitialized)
        {
            cameraZ = transform.position.z;
            FindCurrentRoom();

            if (currentRoom != null)
            {
                SetCameraToRoom(currentRoom, instant: true);
            }

            isInitialized = true;
        }
    }

    void LateUpdate()
    {
        // Используем LateUpdate вместо Update для более плавной работы
        CheckRoomTransition();
        MoveCamera();
    }

    void CheckRoomTransition()
    {
        if (player == null) return;

        // Проверяем, находится ли игрок в текущей комнате
        if (currentRoom == null || !currentRoom.ContainsPoint(player.position))
        {
            Room newRoom = FindRoomContainingPlayer();

            if (newRoom != null && newRoom != currentRoom)
            {
                Debug.Log($"[CameraFollow] Переход в комнату: {newRoom.name}");
                currentRoom = newRoom;
                SetCameraToRoom(currentRoom, instant: false);
            }
        }
    }

    void SetCameraToRoom(Room room, bool instant)
    {
        targetPosition = new Vector3(
            room.RoomCenter.x,
            room.RoomCenter.y,
            cameraZ
        );

        if (instant)
        {
            transform.position = targetPosition;
            isTransitioning = false;
            Debug.Log($"[CameraFollow] Мгновенное перемещение в {targetPosition}");
        }
        else
        {
            isTransitioning = true;
            Debug.Log($"[CameraFollow] Начало перехода к {targetPosition}");
        }
    }

    void MoveCamera()
    {
        if (!isTransitioning) return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            transitionSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < transitionThreshold)
        {
            transform.position = targetPosition;
            isTransitioning = false;
            Debug.Log($"[CameraFollow] Переход завершён");
        }
    }

    Room FindRoomContainingPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("[CameraFollow] Player не назначен!");
            return null;
        }

        Room[] allRooms = FindObjectsByType<Room>(FindObjectsSortMode.None);

        foreach (Room room in allRooms)
        {
            if (room.ContainsPoint(player.position))
            {
                return room;
            }
        }

        Debug.LogWarning($"[CameraFollow] Комната не найдена для позиции игрока: {player.position}");
        return null;
    }

    void FindCurrentRoom()
    {
        currentRoom = FindRoomContainingPlayer();

        if (currentRoom != null)
        {
            Debug.Log($"[CameraFollow] Найдена текущая комната: {currentRoom.name}");
        }
        else
        {
            Debug.LogWarning("[CameraFollow] Текущая комната не найдена!");
        }
    }

    // Публичный метод для принудительной установки камеры (если нужно)
    public void ForceSetToCurrentRoom()
    {
        FindCurrentRoom();
        if (currentRoom != null)
        {
            SetCameraToRoom(currentRoom, instant: true);
        }
    }
}