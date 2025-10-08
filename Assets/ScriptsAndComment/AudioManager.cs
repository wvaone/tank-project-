using UnityEngine;

/// <summary>
/// Менеджер звуков. Реализует паттерн Singleton.
/// Обеспечивает существование только одного экземпляра на сцене.
/// Не уничтожается при смене сцен (DontDestroyOnLoad).
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Статический экземпляр AudioManager для глобального доступа.
    /// </summary>
    public static AudioManager Instance;

    /// <summary>
    /// Вызывается при создании объекта.
    /// Если Instance ещё не существует — назначает себя.
    /// Если уже есть — уничтожает дубликат.
    /// Делает объект неуничтожаемым при смене сцен.
    /// </summary>
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            // Уничтожаем дубликат
            Destroy(gameObject);
            return;
        }

        // Объект не будет уничтожен при загрузке новой сцены
        DontDestroyOnLoad(gameObject);
    }
}