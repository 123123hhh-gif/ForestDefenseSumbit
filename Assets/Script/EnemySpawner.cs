using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

// Spawn enemies at fixed intervals, control waves - Singleton version
public class EnemySpawner : MonoBehaviour
{
    // Singleton instance (Core)
    private static EnemySpawner _instance;
    // Public access property
    public static EnemySpawner Instance
    {
        get
        {
            // If instance is null, try to find in scene
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnemySpawner>();

                // If not found, automatically create a host object
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
    public Waypoint startWaypoint; // Enemy start waypoint
    public GameObject enemyPrefab; // Enemy prefab
    public float spawnInterval = 1f; // Spawn interval (seconds)

    public float enemySpeedMultiplier = 1f; // Enemy speed multiplier
    public float enemyHealthMultiplier = 1f; // Enemy health multiplier
    public int waveCount = 5; // Number of enemies per wave
    public float waveInterval = 10f; // Wave interval (seconds between waves)

    [Header("Difficulty Limits")]
    public int maxWaveCount = 20; // Maximum number of enemies per wave
    public float minSpawnInterval = 0.5f; // Minimum spawn interval
    public int maxTotalWaves = 0; // Maximum total waves (0 = infinite)

    public int playerHP = 10;
    [HideInInspector]
    public int playerHPMax;

    [Header("GameConfig")]
    public TextMeshProUGUI waveTxt;
    public Button startBtn;

    public GameObject progressPanel;
    public TextMeshProUGUI waveCountdownTxt;
    public GameObject waveTimePanel;

    public Coroutine spawnCoroutine;
    public int currentWave = 1;
    public bool isSpawning = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (waveCountdownTxt != null && waveTimePanel != null)
        {
            waveTimePanel.SetActive(false);
        }
        initHp();
    }

    private void Start()
    {
        // StartSpawnWaves();
    }

    public void initHp()
    {
        playerHPMax = playerHP;
    }

    public void onStartBtn()
    {
        if (progressPanel != null)
        {
            progressPanel.SetActive(true);
        }
        startBtn.enabled = false;

        StartSpawnWaves();
    }

    public void StartSpawnWaves()
    {
        if (isSpawning || spawnCoroutine != null) return;

        isSpawning = true;
        spawnCoroutine = StartCoroutine(SpawnEnemyWaves());
    }

    private IEnumerator SpawnEnemyWaves()
    {
        currentWave = 1;

        while (isSpawning && (maxTotalWaves == 0 || currentWave <= maxTotalWaves))
        {
            Debug.Log($"Spawning wave {currentWave} enemies");
            waveTxt.text = currentWave + "/" + maxTotalWaves;

            for (int i = 0; i < waveCount && isSpawning; i++)
            {
                if (!isSpawning) break;

                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }

            if (!isSpawning || (maxTotalWaves > 0 && currentWave >= maxTotalWaves))
            {
                break;
            }

            if (waveCountdownTxt != null && waveTimePanel != null)
            {
                waveTimePanel.SetActive(true);
                float remainingTime = waveInterval;

                while (remainingTime > 0 && isSpawning)
                {
                    waveCountdownTxt.text = Mathf.FloorToInt(remainingTime) + "";
                    yield return new WaitForSeconds(0.1f);
                    remainingTime -= 0.1f;
                }

                waveTimePanel.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(waveInterval);
            }

            if (maxTotalWaves == 0)
            {
                waveCount = waveCount + 2;
                enemyHealthMultiplier = enemyHealthMultiplier + 0.1f;
            }
            else
            {
                waveCount = Mathf.Min(waveCount + 2, maxWaveCount);
                enemyHealthMultiplier = Mathf.Min(enemyHealthMultiplier + 0.1f, 3f);
            }

            spawnInterval = Mathf.Max(spawnInterval - 0.3f, minSpawnInterval);
            enemySpeedMultiplier = Mathf.Min(enemySpeedMultiplier + 0.2f, 3f);

            Debug.Log("spawnInterval=" + spawnInterval);

            currentWave++;
        }

        StopSpawnWaves();
        Debug.Log($"Enemy spawning finished, total waves spawned: {currentWave - 1}");
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || startWaypoint == null)
        {
            Debug.LogError("Enemy prefab or start waypoint not assigned!");
            return;
        }

        GameObject enemyObj = Instantiate(enemyPrefab, startWaypoint.transform.position, Quaternion.identity);

        BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.moveSpeed *= enemySpeedMultiplier;
            enemy.maxHealth = Mathf.RoundToInt(enemy.maxHealth * enemyHealthMultiplier);

            enemy.SetStartWaypoint(startWaypoint);
        }
    }

    public void StopSpawnWaves()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

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

    public int HealHP(int num)
    {
        playerHP = playerHP + num;
        return playerHP;
    }

    private void OnDestroy()
    {
        StopSpawnWaves();

        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnDisable()
    {
        StopSpawnWaves();
    }
}