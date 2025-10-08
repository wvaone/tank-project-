using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text waveText;
    public int currentWave = 1;
    public GameObject enemyPrefab;
    public Transform[] spawnPoints;

    void Start()
    {
        SpawnWave();
    }

    void SpawnWave()
    {
        waveText.text = $"WAVE {currentWave}";
        int count = currentWave * 3;
        for (int i = 0; i < count; i++)
        {
            Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(enemyPrefab, spawn.position, spawn.rotation);
        }
        currentWave++;
        Invoke("SpawnWave", 30f);
    }
}