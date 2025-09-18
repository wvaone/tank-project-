using UnityEngine;

/// <summary>
/// Скрипт ИИ для вражеского танка.
/// Автоматически движется к игроку, поворачивается в его сторону.
/// Имеет здоровье, при смерти создаёт взрыв и звук.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    /// <summary>
    /// Ссылка на трансформ игрока (танка игрока).
    /// Если не задан в инспекторе — ищет автоматически при старте.
    /// </summary>
    public Transform player;

    /// <summary>
    /// Скорость движения врага вперёд (в единицах в секунду).
    /// </summary>
    public float moveSpeed = 5f;

    /// <summary>
    /// Скорость плавного поворота в сторону игрока.
    /// Чем выше — тем резче поворот.
    /// </summary>
    public float rotationSpeed = 3f;

    /// <summary>
    /// Текущее здоровье врага. При значении <= 0 — уничтожается.
    /// </summary>
    public int health = 3;

    /// <summary>
    /// Префаб эффекта взрыва, создаваемого при смерти.
    /// Должен содержать Particle System и AutoDestroy.cs.
    /// </summary>
    public GameObject explosionEffect;

    /// <summary>
    /// Звуковой клип, проигрываемый при уничтожении врага.
    /// </summary>
    public AudioClip deathSound;

    /// <summary>
    /// Вызывается при старте.
    /// Если player не задан в инспекторе — ищет TankController на сцене.
    /// </summary>
    private void Start()
    {
        if (player == null)
        {
            // Ищем танк игрока по компоненту TankController
            player = FindObjectOfType<TankController>().transform;
        }
    }

    /// <summary>
    /// Вызывается каждый кадр.
    /// Поворачивает врага в сторону игрока и двигает его вперёд.
    /// </summary>
    void Update()
    {
        // Если игрок не найден — ничего не делаем
        if (player == null) return;

        // Рассчитываем направление от врага к игроку
        Vector3 direction = (player.position - transform.position).normalized;

        // Создаём кватернион поворота, игнорируя наклон по Y (только горизонтальный поворот)
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Плавно поворачиваем врага в сторону игрока
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);

        // Двигаем врага вперёд относительно его локальной системы координат
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime, Space.Self);
    }

    /// <summary>
    /// Наносит урон врагу.
    /// Если здоровье <= 0 — создаёт взрыв, звук, добавляет очко игроку и уничтожает врага.
    /// </summary>
    /// <param name="hitPoint">Точка попадания (для создания эффекта взрыва)</param>
    public void TakeDamage(Vector3 hitPoint)
    {
        health--;

        if (health <= 0)
        {
            // Создаём эффект взрыва в точке попадания, если префаб задан
            if (explosionEffect != null)
            {
                Instantiate(explosionEffect, hitPoint, Quaternion.identity);
            }

            // Проигрываем звук смерти
            AudioSource.PlayClipAtPoint(deathSound, transform.position, 1f);

            // Находим контроллер игрока и добавляем ему одно убийство
            var playerController = FindObjectOfType<TankController>();
            if (playerController != null)
            {
                playerController.AddKill();
            }

            // Уничтожаем врага
            Destroy(gameObject);
        }
    }
}