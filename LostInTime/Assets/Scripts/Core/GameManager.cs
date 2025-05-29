using UnityEngine;
using UnityEngine.SceneManagement;
using AdvancedShooterKit;

//TEMPORARY UI manager stub
public class UIManager
{
    public static UIManager Instance;
    public void UpdateScoreDisplay(int score) { Debug.Log($"Score updated {score}"); }
    public void ShowGameOverScreen(int score, int wave) { Debug.Log($"Game Over! Score: {score}, Wave: {wave}"); }
    public void ShowVictoryScreen(int score) { Debug.Log($"Victory! Score: {score}"); }
    public void ShowWaveTransition(int wave) { Debug.Log($"Wave {wave} starting..."); }
}

// TEMPORARY wave manager stub
public class WaveManager
{
    public static WaveManager Instance;
    public void StartNextWave() { Debug.Log("Starting next wave!"); }
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


    public enum GameState { MainMenu, Playing, Paused, GameOver, Victory }
    [Header("Game State")]
    private GameState _currentState;

    [Header("Player Settings")]
    [SerializeField] private PlayerCharacter playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    private PlayerCharacter _player;

    [Header("Game Progress")]
    public int currentWave = 0;
    public int totalWaves = 10;
    public int currentScore = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (UIManager.Instance == null) UIManager.Instance = new UIManager();
        if (WaveManager.Instance == null) WaveManager.Instance = new WaveManager();

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Initialize game in appropriate state
        ChangeState(GameState.MainMenu);
    }

    public void StartGame()
    {
        // Reset game values
        currentWave = 0;
        currentScore = 0;

        // Spawn player
        _player = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        // Change state to playing
        ChangeState(GameState.Playing);

        // Notify wave manager to start first wave
        WaveManager.Instance.StartNextWave();
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        // Show game over UI
        UIManager.Instance.ShowGameOverScreen(currentScore, currentWave);
    }

    public void Victory()
    {
        ChangeState(GameState.Victory);
        // Show victory UI
        UIManager.Instance.ShowVictoryScreen(currentScore);
    }

    public void ChangeState(GameState newState)
    {
        _currentState = newState;

        // Handle state-specific logic
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.GameOver:
                // Disable player controls
                break;

            case GameState.Victory:
                // Disable player controls
                break;
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        // Update UI
        UIManager.Instance.UpdateScoreDisplay(currentScore);
    }

    public void WaveCompleted()
    {
        currentWave++;

        if (currentWave >= totalWaves)
        {
            Victory();
        }
        else
        {
            // Show wave transition UI
            UIManager.Instance.ShowWaveTransition(currentWave);

            // Start next wave after delay
            Invoke("StartNextWave", 3f);
        }
    }

    private void StartNextWave()
    {
        WaveManager.Instance.StartNextWave();
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Game State: {_currentState}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Wave: {currentWave}/{totalWaves}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Score: {currentScore}");
    }
    // Update is called once per frame
    void Update()
    {
        // Test controls
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("Starting game test");
            StartGame();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("Testing game over");
            GameOver();
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("Testing victory");
            Victory();
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Debug.Log("Adding 100 score");
            AddScore(100);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Debug.Log("Testing wave complete");
            WaveCompleted();
        }
    }
}
