using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Wave
{
    [Header("Wave Configuration")]
    public string waveName = "Wave";
    public Enemy[] enemies;

    [Tooltip("Time between spawning each enemy")]
    [Range(0.1f, 30f)]
    public float timeToNextEnemy;

    [Tooltip("Time to wait before spawning the next wave")]
    [Range(1f, 30f)]
    public float timeToNextWave;

    [Header("Spawn Settings")]
    [Tooltip("If true, all enemies will spawn at random points")]
    public bool useRandomSpawnPoints = false;

    [Tooltip("Radius around spawn point to randomly place enemies")]
    [Range(0f, 20f)]
    public float randomSpawnRadius = 5f;

    [HideInInspector] public int enemiesLeft;

    // For visualization in the inspector
    public string WaveStatus => $"{waveName}: {enemiesLeft} enemies remaining";
}
[System.Serializable]
public class WaveEvent : UnityEvent<int> { }

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private bool autoStart = true;


    [Header("Difficulty Scaling")]
    [SerializeField] private bool enableDifficultyScaling = true;

    [Tooltip("Base health multiplier on wave 1")]
    [SerializeField] private float baseHealthMultiplier = 1.0f;

    [Tooltip("How much the health multiplier increases each wave (linear)")]
    [SerializeField] private float healthScalingPerWave = 0.1f;

    [Tooltip("Base damage multiplier applied to wave 1")]
    [SerializeField] private float baseDamageMultiplier = 1.0f;

    [Tooltip("How much the damage multiplier increases each wave (linear)")]
    [SerializeField] private float damageScalingPerWave = 0.05f;


    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [Tooltip("If no spawn points are assigned, use this transfomr")]
    [SerializeField] private Transform defaultSpawnPoint;

    [Header("Timers")]
    [SerializeField] private float initialDelay = 3f;
    [SerializeField] private float countdown;

    [Header("UI Feedback")]
    [SerializeField] private bool showDebugMessages = true;

    [Header("Events")]
    public WaveEvent onWaveStart;
    public WaveEvent onWaveEnd;
    public WaveEvent onAllWavesCompleted;
    public UnityEvent<int> onEnemyKilled;

    // State tracking
    [Header("Current State")]
    [SerializeField] private int currentWaveIndex = 0;
    [SerializeField] private bool isSpawning = false;
    [SerializeField] private bool isWaitingForNextWave = false;
    [SerializeField] private bool allWavesCompleted = false;

    // Properties for external access
    public int CurrentWaveNumber => currentWaveIndex + 1;
    public int TotalWave => waves.Length;
    public bool IsActive => !allWavesCompleted;
    public Wave[] Waves => waves;
    public Wave CurrentWave => currentWaveIndex < waves.Length ? waves[currentWaveIndex] : null;


    private void Start()
    {
        InitializeWaves();

        if (spawnPoints.Length == 0 && defaultSpawnPoint == null)
        {
            Debug.LogError("No SpawnPoints assigned");
        }

        if (autoStart)
        {
            countdown = initialDelay;
            isWaitingForNextWave = true;
        }
    }


    private void Update()
    {
        if (allWavesCompleted) return;

        // Handle countdown to next wave
        if (isWaitingForNextWave)
        {
            countdown -= Time.deltaTime;

            if (countdown <= 0)
            {
                isWaitingForNextWave = false;
                StartWave(currentWaveIndex);
            }
        }

        // Check if current wave is completed
        if (!isSpawning && !isWaitingForNextWave && CurrentWave != null && CurrentWave.enemiesLeft <= 0)
        {
            CompleteWave(currentWaveIndex);
        }
    }

    private void InitializeWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            waves[i].enemiesLeft = waves[i].enemies.Length;

            // Validate wave settings
            if (waves[i].enemies.Length == 0)
            {
                Debug.LogWarning($"Wave {i + 1} has no enemies assigned!");
            }
        }
    }

    private (float healthMultiplier, float damageMultiplier) CalculateDifficultyForWave(int waveIndex)
    {
        if (!enableDifficultyScaling || waveIndex <= 0)
        {
            return (baseHealthMultiplier, baseDamageMultiplier);
        }

        // Calculate health multiplier
        float healthMultiplier = baseHealthMultiplier + (healthScalingPerWave * (waveIndex));

        // Calclulate damage multiplier
        float damageMultiplier = baseDamageMultiplier + (damageScalingPerWave * (waveIndex));

        return (healthMultiplier, damageMultiplier); 
    }

    private void ApplyDifficultyToZombie(GameObject zombieObject, float healthMultiplier, float damageMultiplier)
    {
        if (zombieObject == null) return;

        // Scale Health
        ZombieStateMachine zombie = zombieObject.GetComponent<ZombieStateMachine>();
        if (zombie != null)
        {
            // Get the base health value
            int originalMaxHealth = zombie.maxHealth;

            int newMaxHealth = Mathf.RoundToInt(originalMaxHealth * healthMultiplier);
            zombie.maxHealth = newMaxHealth;

            // Also set current health to max health
            typeof(ZombieStateMachine).GetField("currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(zombie, newMaxHealth);
        }

        // Scale Damage
        ZombieAttack zombieAttack = zombieObject.GetComponent<ZombieAttack>();
        if (zombieAttack != null)
        {
            float originalDamage = zombieAttack.damageAmount;
            zombieAttack.damageAmount = originalDamage * damageMultiplier;
        } 
    }

    private void StartWave(int waveIndex)
    {
        if (waveIndex >= waves.Length)
        {
            Debug.LogWarning("Trying to start a wave beyond the available waves");
        }

        Debug.Log($"Starting Wave {waveIndex + 1}: {waves[waveIndex].waveName}");
        onWaveStart?.Invoke(waveIndex + 1);

        StartCoroutine(SpawnWaveEnemies(waveIndex));
    }

    private IEnumerator SpawnWaveEnemies(int waveIndex)
    {
        Wave wave = waves[waveIndex];
        isSpawning = true;

        // Calculate difficulty for this wave
        var (healthMultiplier, damageMultiplier) = CalculateDifficultyForWave(waveIndex);

        for (int i = 0; i < wave.enemies.Length; i++)
        {
            Transform spawnPoint = GetSpawnPoint(wave);

            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = spawnPoint.rotation;

            // Apply random position if enabled
            if (wave.useRandomSpawnPoints && wave.randomSpawnRadius > 0)
            {
                Vector2 randomCircle = Random.insideUnitCircle * wave.randomSpawnRadius;
                spawnPosition += new Vector3(randomCircle.x, 0, randomCircle.y);
            }

            // Spawn enemy
            Enemy enemy = Instantiate(wave.enemies[i], spawnPosition, spawnRotation);

            // Apply difficulty scaling to spawned enemy
            ApplyDifficultyToZombie(enemy.gameObject, healthMultiplier, damageMultiplier);

            // RegisterEnemyCallbacks(enemy);

            yield return new WaitForSeconds(wave.timeToNextEnemy);
        }

        isSpawning = false;
        Debug.Log($"All enemies for Wave {waveIndex + 1} have been spawned. {wave.enemiesLeft} remaining.");
    }

    private void CompleteWave(int waveIndex)
    {
        onWaveEnd?.Invoke(waveIndex + 1);

        // Check if it was the last wave;
        if (waveIndex >= waves.Length - 1)
        {
            CompleteAllWaves();
            return;
        }

        // Prepare for next wave
        currentWaveIndex++;
        countdown = waves[waveIndex].timeToNextWave;
        isWaitingForNextWave = true;

        Debug.Log($"Next wave starting in {countdown} seconds...");
    }

    private void CompleteAllWaves()
    {
        Debug.Log("All waves completed");
        allWavesCompleted = true;
        onAllWavesCompleted?.Invoke(waves.Length);
    }

    private Transform GetSpawnPoint(Wave wave)
    {
        if (spawnPoints.Length == 0)
        {
            return defaultSpawnPoint;
        }
        
        // Select a spawn point
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    // private void RegisterEnemyCallbacks(Enemy enemy)
    // {
    //     ZombieStateMachine zombieState = enemy.GetComponent<ZombieStateMachine>();
    //     if (zombieState != null)
    //     {

    //     }
    // }

    public void OnEnemyDeath()
    {
        if (currentWaveIndex >= 0 && currentWaveIndex < waves.Length)
        {
            waves[currentWaveIndex].enemiesLeft--;

            onEnemyKilled?.Invoke(50);
        }
    }

    public void StartSpawner()
    {
        if (allWavesCompleted)
        {
            return;
        }

        countdown = initialDelay;
        isWaitingForNextWave = true;
    }

    public void PauseSpawner()
    {
        isWaitingForNextWave = false;
        StopAllCoroutines();
        isSpawning = false;
    }

    public void SkipToNextWave()
    {
        if (currentWaveIndex >= waves.Length - 1) return;

        // Kill all enemies
        waves[currentWaveIndex].enemiesLeft = 0;

        // Stop spawning
        StopAllCoroutines();
        isSpawning = false;

        // Complete wave
        CompleteWave(currentWaveIndex);
    }
}


