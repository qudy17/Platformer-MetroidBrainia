using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Размеры комнаты (в юнитах)")]
    public float roomWidth = 60f;
    public float roomHeight = 33.75f;

    public Vector3 RoomCenter => transform.position;

    // Границы комнаты
    public Bounds RoomBounds => new Bounds(
        transform.position,
        new Vector3(roomWidth, roomHeight, 0)
    );

    public bool ContainsPoint(Vector2 point)
    {
        return RoomBounds.Contains(new Vector3(point.x, point.y, 0));
    }

    // Рисуем границы комнаты в редакторе
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(roomWidth, roomHeight, 0));

        // Крестик в центре
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(
            transform.position + Vector3.left * 0.5f,
            transform.position + Vector3.right * 0.5f
        );
        Gizmos.DrawLine(
            transform.position + Vector3.down * 0.5f,
            transform.position + Vector3.up * 0.5f
        );
    }
}