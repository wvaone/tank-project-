using UnityEngine;

/// <summary>
/// Скрипт, прикреплённый к префабу снаряда.
/// Обрабатывает столкновения и уничтожение врагов.
/// </summary>
public class Shell : MonoBehaviour
{
    /// <summary>
    /// Вызывается при столкновении с другим коллайдером.
    /// Проверяет, не является ли объект врагом (EnemyAI).
    /// Если да — наносит урон, передавая точку попадания для эффекта взрыва.
    /// После этого уничтожает сам снаряд.
    /// </summary>
    /// <param name="collision">Информация о столкновении</param>
    void OnCollisionEnter(Collision collision)
    {
        // Пытаемся получить компонент EnemyAI у объекта, с которым столкнулись
        EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();

        // Если враг найден — наносим урон
        if (enemy != null)
        {
            // Передаём точку столкновения для красивого взрыва
            enemy.TakeDamage(transform.position);
        }

        // Уничтожаем снаряд в любом случае
        Destroy(gameObject);
    }
}