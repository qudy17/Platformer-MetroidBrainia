using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Flask : MonoBehaviour
{
    [Header("Настройки ядра")]
    [SerializeField] private int chargesToAdd = 1; // Сколько зарядов добавляет ядро

    [Header("Визуальные эффекты")]
    [SerializeField] private GameObject collectEffect; // Эффект при сборе

    private bool isCollected = false;
    private CircleCollider2D circleCollider;

    void Awake()
    {
        // Получаем или добавляем CircleCollider2D
        circleCollider = GetComponent<CircleCollider2D>();

        // Настраиваем коллайдер как триггер
        circleCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        // Проверяем, что это игрок
        if (other.CompareTag("Player"))
        {
            // Получаем компонент способности крика
            ScreamAbility screamAbility = other.GetComponent<ScreamAbility>();

            if (screamAbility != null)
            {
                CollectFlask(screamAbility);
            }
            else
            {
                Debug.LogWarning("[Flask] Игрок не имеет компонента ScreamAbility!");
            }
        }
    }

    void CollectFlask(ScreamAbility screamAbility)
    {
        isCollected = true;

        // Увеличиваем максимальное количество зарядов
        screamAbility.maxCharges += chargesToAdd;
        screamAbility.currentCharges += chargesToAdd;

        // Важно: обновляем массив таймеров восстановления
        UpdateRestoreTimers(screamAbility);

        // Регистрируем сбор в статистике
        GameStatsTracker.RegisterFlaskCollected();

        // Проигрываем эффект сбора если есть
        PlayCollectEffect();

        Debug.Log($"[Flask] Ядро собрано! Максимум зарядов: {screamAbility.maxCharges}");

        // Уничтожаем объект ядра
        Destroy(gameObject, 0.1f); // Небольшая задержка для проигрывания эффектов
    }

    void UpdateRestoreTimers(ScreamAbility screamAbility)
    {
        // Обновляем массив таймеров, добавляя новый элемент для дополнительного заряда
        float[] newTimers = new float[screamAbility.maxCharges];
        float[] oldTimers = GetRestoreTimers(screamAbility);

        // Копируем старые таймеры
        for (int i = 0; i < oldTimers.Length; i++)
        {
            newTimers[i] = oldTimers[i];
        }

        // Новый слот таймера инициализируется нулем
        newTimers[newTimers.Length - 1] = 0f;

        // Устанавливаем новый массив через рефлексию
        SetRestoreTimers(screamAbility, newTimers);
    }

    void PlayCollectEffect()
    {
        // Создаем эффект частиц если есть
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
    }

    // Вспомогательные методы для работы с приватным полем restoreTimers
    float[] GetRestoreTimers(ScreamAbility ability)
    {
        var field = typeof(ScreamAbility).GetField("restoreTimers",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            return (float[])field.GetValue(ability);
        }

        Debug.LogError("[Flask] Не удалось получить restoreTimers через рефлексию!");
        return new float[0];
    }

    void SetRestoreTimers(ScreamAbility ability, float[] newTimers)
    {
        var field = typeof(ScreamAbility).GetField("restoreTimers",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(ability, newTimers);
        }
        else
        {
            Debug.LogError("[Flask] Не удалось установить restoreTimers через рефлексию!");
        }
    }

    // Для визуальной отладки в редакторе
    void OnDrawGizmosSelected()
    {
        if (circleCollider == null)
            circleCollider = GetComponent<CircleCollider2D>();

        if (circleCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, circleCollider.radius);
        }
    }
}