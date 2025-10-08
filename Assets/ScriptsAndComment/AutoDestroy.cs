using UnityEngine;

/// <summary>
/// Универсальный скрипт для автоматического уничтожения объекта через заданное время.
/// Используется для вспышек, взрывов, снарядов и других временных эффектов.
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    /// <summary>
    /// Время жизни объекта в секундах. По истечении — объект уничтожается.
    /// </summary>
    public float lifetime = 2f;

    /// <summary>
    /// Вызывается при старте.
    /// Запускает уничтожение объекта через lifetime секунд.
    /// </summary>
    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}