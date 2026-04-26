using UnityEngine;

public class AcousticMirror : MonoBehaviour
{
    [Header("Отладка")]
    [Tooltip("Показывать нормаль в редакторе")]
    public bool showNormalGizmo = true;

    private BoxCollider2D boxCollider;

    void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        boxCollider.isTrigger = false;
    }

    public Vector2 GetSurfaceNormal(Vector2 hitPoint)
    {
        // Нормаль указывает вправо от локального направления объекта
        // Если объект повёрнут на 45° — нормаль тоже будет под 45°
        Vector2 rightDirection = transform.right;
        return rightDirection.normalized;
    }

    void OnDrawGizmos()
    {
        if (showNormalGizmo && boxCollider != null)
        {
            Vector2 normal = transform.right;

            // Рисуем нормаль
            Gizmos.color = Color.cyan;
            Vector3 start = transform.position;
            Gizmos.DrawLine(start, start + (Vector3)(normal * 0.5f));
            Gizmos.DrawWireSphere(start + (Vector3)(normal * 0.5f), 0.05f);

            // Рисуем саму поверхность (перпендикулярно нормали)
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.8f);
            Vector2 surfaceDir = new Vector2(-normal.y, normal.x); // Перпендикуляр
            float surfaceLength = boxCollider.bounds.size.magnitude * 0.5f;
            Gizmos.DrawLine(start + (Vector3)(surfaceDir * surfaceLength),
                           start - (Vector3)(surfaceDir * surfaceLength));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, boxCollider ?
            boxCollider.bounds.size : Vector3.one);

        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.7f,
            $"MIRROR\nRotation: {transform.rotation.eulerAngles.z:F0}°"
        );
    }
}