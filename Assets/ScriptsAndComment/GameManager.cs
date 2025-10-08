using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Менеджер игры. Отвечает за:
/// - Спавн волн врагов
/// - Отображение номера текущей волны на экране
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Текстовый элемент, отображающий текущую волну.
    /// </summary>
    public Text waveText;

    /// <summary>
    /// Номер текущей волны. Начинается с 1.
    /// </summary>
    public int currentWave = 1;

    /// <summary>
    /// Префаб вражеского танка, который будет спавниться.
    /// </summary>
    public GameObject enemyPrefab;

    /// <summary>
    /// Массив точек спавна врагов (пустые GameObject на карте).
    /// Враги появляются в случайной точке из этого массива.
    /// </summary>
    public Transform[] spawnPoints;

    /// <summary>
    /// Вызывается при старте игры.
    /// Запускает первую волну.
    /// </summary>
    void Start()
    {
        SpawnWave();
    }

    /// <summary>
    /// Спавнит волну врагов.
    /// Количество врагов = currentWave * 3.
    /// После спавна устанавливает таймер на 30 секунд для следующей волны.
    /// Обновляет текст волны на экране.
    /// </summary>
    void SpawnWave()
    {
        // Обновляем текст волны
        waveText.text = $"WAVE {currentWave}";

        // Рассчитываем количество врагов
        int count = currentWave * 3;

        // Спавним каждого врага в случайной точке спавна
        for (int i = 0; i < count; i++)
        {
            // Выбираем случайную точку из массива
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Создаём врага
            Instantiate(enemyPrefab, spawn.position, spawn.rotation);
        }

        // Увеличиваем номер волны
        currentWave++;

        // Запускаем следующую волну через 30 секунд
        Invoke("SpawnWave", 30f);
    }
}