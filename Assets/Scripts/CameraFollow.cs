using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Настройки")]
    public Transform player;
    public float transitionSpeed = 3f;
    public float transitionThreshold = 0.01f;

    [Header("Текущая комната(только для просмотра)")]
    [SerializeField] private Room currentRoom;

    // Приватные переменные
    private Vector3 targetPosition;
    private bool isTransitioning = false;
    private float cameraZ = -10f;

    void Start()
    {
        cameraZ = transform.position.z;

        FindCurrentRoom();

        if (currentRoom != null)
        {
            SetCameraToRoom(currentRoom, instant: true);
        }
    }

    void Update()
    {
        CheckRoomTransition();

        MoveCamera();
    }

    void CheckRoomTransition()
    {
        if (player == null) return;

        if (currentRoom == null || !currentRoom.ContainsPoint(player.position))
        {
            Room newRoom = FindRoomContainingPlayer();

            if (newRoom != null && newRoom != currentRoom)
            {
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
        }
        else
        {
            isTransitioning = true;
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
        }
    }

    Room FindRoomContainingPlayer()
    {
        if (player == null) return null;

        Room[] allRooms = FindObjectsByType<Room>(FindObjectsInactive.Exclude);

        foreach (Room room in allRooms)
        {
            if (room.ContainsPoint(player.position))
            {
                return room;
            }
        }

        return null;
    }

    void FindCurrentRoom()
    {
        currentRoom = FindRoomContainingPlayer();
    }
}