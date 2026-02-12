using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

// Spawns enemies periodically and controls wave progression (Singleton version)
public class EnemySpawner : MonoBehaviour
{
    // Singleton instance (core)
    private static EnemySpawner _instance;

    // Public accessor for the singleton instance
    public static EnemySpawner Instance
    {
        get
        {
            // If the instance is null, try to find one in the scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnemySpawner>();

                // If none is found, create a new GameObject and attach EnemySpawner
                if (_instance == null)
                {
                    GameObject spawnerObj = new GameObject("EnemySpawner (Singleton)");
                    _instance = spawnerObj.AddComponent<EnemySpawner>();
                }
            }
            return _instance;
        }
    }

    [Header("LEVELID")]
    public int LevelId = 1;

    [Header("Spawn Settings")]
    public Waypoint startWaypoint;          // Enemy starting waypoint
    public GameObject enemyPrefab;          // Enemy prefab to spawn
    public float spawnInterval = 1f;        // Time between spawns (seconds)

    public float enemySpeedMultiplier = 1f; // Enemy speed multiplier
    public float enemyHealthMultiplier = 1f;// Enemy health multiplier

    public int waveCount = 5;               // Number of enemies spawned per wave
    public float waveInterval = 10f;        // Time between waves (seconds)

    [Header("Difficulty Limits")]
    public int maxWaveCount = 20;           // Maximum enemies per wave
    public float minSpawnInterval = 0.5f;   // Minimum spawn interval
    public int maxTotalWaves = 0;           // Maximum number of waves (0 = infinite)

    public int playerHP = 10;               // Current player HP

    [HideInInspector]
    public int playerHPMax;                 // Maximum player HP (initialized from playerHP)

    [Header("GameConfig")]
    public TextMeshProUGUI waveTxt;         // UI text showing the current wave
    public Button startBtn;                // Start button

    public GameObject progressPanel;        // UI panel shown after starting
    public TextMeshProUGUI waveCountdownTxt;// UI text showing the countdown between waves
    public GameObject waveTimePanel;        // UI panel for the wave interval countdown

    public Coroutine spawnCoroutine;        // Reference to the spawning coroutine
    public int currentWave = 1;             // Current wave index
    public bool isSpawning = false;         // Whether spawning is currently active

    private void Awake()
    {
        // Initialize singleton instance
        if (_instance == null)
        {
            _instance = this;

            // Optional: keep this object across scene loads
            // DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            // Destroy duplicate singleton instance
            Destroy(gameObject);
            return;
        }

        // Hide countdown panel at startup
        if (waveCountdownTxt != null && waveTimePanel != null)
        {
            waveTimePanel.SetActive(false);
        }

        InitHp();
    }

    private void Start()
    {
        // Optional: auto-start spawning
        // StartSpawnWaves();
    }

    // Initialize the player's max HP value
    public void InitHp()
    {
        playerHPMax = playerHP;
    }

    // Called by the start button to begin wave spawning
    public void OnStartBtn()
    {
        // Show progress UI if available
        if (progressPanel != null)
        {
            progressPanel.SetActive(true);
        }

        // Disable the start button to prevent repeated clicks
        startBtn.enabled = false;

        StartSpawnWaves();
    }

    // Start spawning waves if not already spawning
    public void StartSpawnWaves()
    {
        if (isSpawning || spawnCoroutine != null) return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnEnemyWaves());
    }

    // Coroutine that spawns enemies in waves and increases difficulty over time
    private IEnumerator SpawnEnemyWaves()
    {
        currentWave = 1;

        // Continue spawning until stopped or until maxTotalWaves is reached (if not infinite)
        while (isSpawning && (maxTotalWaves == 0 || currentWave <= maxTotalWaves))
        {
            Debug.Log($"Spawning wave {currentWave}");
            waveTxt.text = currentWave + "/" + maxTotalWaves;

            // Spawn enemies for the current wave
            for (int i = 0; i < waveCount && isSpawning; i++)
            {
                if (!isSpawning) break;

                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            // Stop if spawning ended or we reached the last wave
            if (!isSpawning || (maxTotalWaves > 0 && currentWave >= maxTotalWaves))
            {
                break;
            }

            // Show countdown UI between waves if available
            if (waveCountdownTxt != null && waveTimePanel != null)
            {
                waveTimePanel.SetActive(true);
                float remainingTime = waveInterval;

                while (remainingTime > 0 && isSpawning)
                {
                    waveCountdownTxt.text = Mathf.FloorToInt(remainingTime).ToString();
                    yield return new WaitForSeconds(0.1f);
                    remainingTime -= 0.1f;
                }

                waveTimePanel.SetActive(false);
            }
            else
            {
                // Fallback: wait full interval without UI
                yield return new WaitForSeconds(waveInterval);
            }

            // Increase difficulty after each wave
            if (maxTotalWaves == 0)
            {
                // Infinite mode: no cap on waveCount / multipliers (except those applied below)
                waveCount = waveCount + 2;
                enemyHealthMultiplier = enemyHealthMultiplier + 0.1f;
            }
            else
            {
                // Limited mode: apply caps
                waveCount = Mathf.Min(waveCount + 2, maxWaveCount);
                enemyHealthMultiplier = Mathf.Min(enemyHealthMultiplier + 0.1f, 3f);
            }

            // Reduce spawn interval (down to minimum) and increase enemy speed (up to cap)
            spawnInterval = Mathf.Max(spawnInterval - 0.3f, minSpawnInterval);
            enemySpeedMultiplier = Mathf.Min(enemySpeedMultiplier + 0.2f, 3f);

            Debug.Log("spawnInterval=" + spawnInterval);

            currentWave++;
        }

        StopSpawnWaves();
        Debug.Log($"Spawning ended. Total waves spawned: {currentWave - 1}");
    }

    // Spawn a single enemy and apply difficulty multipliers
    private void SpawnEnemy()
    {
        // Validate required references
        if (enemyPrefab == null || startWaypoint == null)
        {
            Debug.LogError("Enemy prefab or start waypoint is not assigned!");
            return;
        }

        // Instantiate enemy at the start waypoint position
        GameObject enemyObj = Instantiate(enemyPrefab, startWaypoint.transform.position, Quaternion.identity);

        // Apply multipliers if the enemy uses BaseEnemy component
        BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.moveSpeed *= enemySpeedMultiplier;
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * enemyHealthMultiplier);

            // Set initial waypoint/path target
            enemy.SetStartWaypoint(startWaypoint);
        }
    }

    // Stop wave spawning and cleanup coroutine reference
    public void StopSpawnWaves()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // Reduce player HP by a given amount and return remaining HP (0 if dead)
    public int LoseHP(int num)
    {
        playerHP = playerHP - num;

        if (playerHP <= 0)
        {
            return 0;
        }
        else
        {
            return playerHP;
        }
    }

    // Increase player HP by a given amount and return the new HP value
    public int HealHP(int num)
    {
        playerHP = playerHP + num;
        return playerHP;
    }

    private void OnDestroy()
    {
        // Ensure spawning stops when object is destroyed
        StopSpawnWaves();

        // Clear singleton reference if this instance is being destroyed
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnDisable()
    {
        // Stop spawning when the component is disabled
        StopSpawnWaves();
    }
}
