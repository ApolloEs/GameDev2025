using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float countdown;

    [SerializeField] private GameObject spawnPoint;

    public Wave[] waves;

    public int currentWaveIndex = 0;

    private bool readyToCountDown;

    // Events for GameManager to listen to
    public delegate void WaveEvent(int waveNumber);
    public static event WaveEvent OnWaveStart;
    public static event WaveEvent OnWaveEnd;
    public static event WaveEvent OnAllWavesCompleted;

    // Event for tracking enemy deaths
    public delegate void EnemyKilledEvent(int pointValue);
    public static event EnemyKilledEvent OnEnemyKilled;

    private void Update()
    {
        if (readyToCountDown == true)
        {
            countdown -= Time.deltaTime;
        }

        if(countdown <= 0)
        {
            readyToCountDown = false;
            countdown = 1000;
            countdown = waves[currentWaveIndex].timeToNextWave;
            StartCoroutine(SpawnWave());
        }
        if (waves[currentWaveIndex].enemiesLeft == 0)
        {
            readyToCountDown = true;
            currentWaveIndex++;

            if (OnWaveEnd != null)
            {
                OnWaveEnd(currentWaveIndex + 1);
            }
        }

        if (currentWaveIndex >= waves.Length - 1)
        {
            if (OnAllWavesCompleted != null)
            {
                OnAllWavesCompleted(waves.Length);

                // Prevent repeated triggers
                currentWaveIndex = -1;
            }
        }

    }
    private IEnumerator SpawnWave()
    {
        if (OnWaveStart != null)
        {
            OnWaveStart(currentWaveIndex + 1);
        }
        for (int i = 0; i < waves[currentWaveIndex].enemies.Length; i++)
        {
            Enemy enemy = Instantiate(waves[currentWaveIndex].enemies[i], spawnPoint.transform);

            enemy.transform.SetParent(spawnPoint.transform);

            yield return new WaitForSeconds(waves[currentWaveIndex].timeToNextEnemy);
        }
    }

    private void Start()
    {
        readyToCountDown = true;

        for(int i = 0; i<waves.Length; i++)
        {
            waves[i].enemiesLeft = waves[i].enemies.Length;
        }
    }


}

[System.Serializable]
public class Wave
{
    public Enemy[] enemies;
    public float timeToNextEnemy;
    public float timeToNextWave;

    [HideInInspector] public int enemiesLeft;
}
