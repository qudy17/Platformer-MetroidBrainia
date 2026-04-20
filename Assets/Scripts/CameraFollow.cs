using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Настройки")]
    public Transform player;              // Ссылка на игрока
    public float transitionSpeed = 3f;    // Скорость перехода между комнатами
    public float transitionThreshold = 0.01f; // Порог завершения перехода

    [Header("Текущая комната (только для просмотра)")]
    [SerializeField] private Room currentRoom;  // Текущая комната

    // Приватные переменные
    private Vector3 targetPosition;       // Куда движется камера
    private bool isTransitioning = false; // Идёт ли переход
    private float cameraZ = -10f;         // Z позиция камеры (всегда фиксирована)

    void Start()
    {
        cameraZ = transform.position.z;

        // Ищем комнату в которой начинает игрок
        FindCurrentRoom();

        // Если нашли комнату — сразу ставим камеру в её центр
        if (currentRoom != null)
        {
            SetCameraToRoom(currentRoom, instant: true);
        }
    }

    void Update()
    {
        // Постоянно проверяем не вышел ли игрок в другую комнату
        CheckRoomTransition();

        // Двигаем камеру к цели
        MoveCamera();
    }

    // Проверка смены комнаты
    void CheckRoomTransition()
    {
        if (player == null) return;

        // Если игрок вышел из текущей комнаты
        if (currentRoom == null || !currentRoom.ContainsPoint(player.position))
        {
            Room newRoom = FindRoomContainingPlayer();

            if (newRoom != null && newRoom != currentRoom)
            {
                // Игрок вошёл в новую комнату
                currentRoom = newRoom;
                SetCameraToRoom(currentRoom, instant: false);
            }
        }
    }

    // Устанавливаем цель камеры = центр комнаты
    void SetCameraToRoom(Room room, bool instant)
    {
        targetPosition = new Vector3(
            room.RoomCenter.x,
            room.RoomCenter.y,
            cameraZ
        );

        if (instant)
        {
            // Мгновенно перемещаем камеру (для старта)
            transform.position = targetPosition;
            isTransitioning = false;
        }
        else
        {
            // Запускаем плавный переход
            isTransitioning = true;
        }
    }

    // Плавное движение камеры к цели
    void MoveCamera()
    {
        if (!isTransitioning) return;

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            transitionSpeed * Time.deltaTime
        );

        // Проверяем достигли ли цели
        if (Vector3.Distance(transform.position, targetPosition) < transitionThreshold)
        {
            transform.position = targetPosition;
            isTransitioning = false;
        }
    }

    // Ищем комнату содержащую игрока
    Room FindRoomContainingPlayer()
    {
        if (player == null) return null;

        // Находим все комнаты на сцене
        Room[] allRooms = FindObjectsByType<Room>(FindObjectsInactive.Exclude);

        foreach (Room room in allRooms)
        {
            if (room.ContainsPoint(player.position))
            {
                return room;
            }
        }

        return null; // Игрок между комнатами
    }

    // Ищем стартовую комнату
    void FindCurrentRoom()
    {
        currentRoom = FindRoomContainingPlayer();
    }
}