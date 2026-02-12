using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [Header("Basic Configuration")]
    public int startGold = 100; // Initial gold amount when the game starts/resets
    public TextMeshProUGUI coinTxt; // UI text that displays current gold
    public TextMeshProUGUI hpTxt; // UI text that displays player HP
    public TextMeshProUGUI killTxt; // UI text that displays kill count
    public TextMeshProUGUI Remaining; // UI text that displays remaining enemies alive

    public AudioClip bgmWarriors; // Background music clip to play during gameplay

    public int killNum = 0; // Total number of enemies killed by the player

    private int _currentGold; // Current gold the player has

    private float timer = 0f; // Timer accumulator for periodic checks
    public float callInterval = 1f; // Interval (seconds) for periodic updates

    private bool isGameOver = false; // True when game has ended (win or lose)
    private bool isVictory = false; // True when game ended with victory

    private void Awake()
    {
        if (Instance == null) { Instance = this; } else { Destroy(gameObject); return; } // Ensure only one GameManager exists

        _currentGold = startGold; // Initialize gold at the start
        UpdateGoldUI(); 
    }

    void Start()
    {
        //AudioManager.Instance.PlayBGM(bgmWarriors); 
    }

    void Update()
    {
        timer += Time.deltaTime; // Accumulate time each frame

        if (timer >= callInterval) // Run periodic logic once per interval to reduce per-frame cost
        {
            PerSecondMethod(); 
            CheckVictoryCondition(); 
            timer -= callInterval; 
        }
    }

    public void PerSecondMethod()
    {
        List<BaseEnemy> enemies = EnemyManager.Instance.GetAllAliveEnemies(); // Query all currently alive enemies from EnemyManager
        Remaining.text = enemies.Count + ""; // Update remaining enemy count in UI
        killTxt.text = killNum + ""; // Update kill count in UI
    }

    private void CheckVictoryCondition()
    {
        if (isGameOver || EnemySpawner.Instance.maxTotalWaves == 0 || EnemySpawner.Instance.isSpawning) { return; } // Skip if game already ended, waves not configured, or still spawning

        bool allWavesSpawned = EnemySpawner.Instance.currentWave >= EnemySpawner.Instance.maxTotalWaves; 
        bool noEnemiesAlive = EnemyManager.Instance.GetAllAliveEnemies().Count == 0; 
        bool playerAlive = EnemySpawner.Instance.playerHP > 0; 

        if (allWavesSpawned && noEnemiesAlive && playerAlive) { OnVictory(); } // Win if all waves done, no enemies remain, and player survived
    }

    private void OnVictory()
    {
        isGameOver = true; // Lock game state to prevent further lose/win triggers
        isVictory = true; // Mark victory state
        EnemySpawner.Instance.StopSpawnWaves(); // Stop any further spawning to freeze wave system
        UIManager.Instance.onOpenVictory(); // Open victory UI panel

        Debug.Log("Game victory."); 
    }

    public void TakeDamage(int damageToPlayer)
    {
        if (isGameOver) return; // Ignore damage if the game already ended

        int num = EnemySpawner.Instance.LoseHP(damageToPlayer); // Apply damage through spawner/playerHP controller and get updated HP
        Debug.Log("Player HP after damage: " + num); // Log updated HP in English only

        hpTxt.text = num + ""; // Update HP UI immediately when damage occurs

        if (num <= 0) 
        {
            isGameOver = true; 
            UIManager.Instance.onOpenLose(); 
            EnemySpawner.Instance.StopSpawnWaves(); // Stop wave spawning immediately
            Debug.Log("Game lose."); 
        }
    }

    public BaseTower PlaceTower(TowerPlace place, TowerData _data)
    {
        SpendGold(_data.cost); // Deduct tower cost immediately when placing

        Vector3 spawnPos = place.PlacePosition; // Use the placement node position as spawn location
        spawnPos.y += 0.2f; // Slightly raise tower to avoid z-fighting / clipping into ground

        GameObject towerObj = Instantiate(_data.towerPrefab, spawnPos, Quaternion.identity); 
        BaseTower tower = towerObj.GetComponent<BaseTower>(); 

        if (tower != null)
        {
            tower.init(_data); // Initialize tower stats/behavior from TowerData
            tower.towerPlace = place; // Record which TowerPlace this tower belongs to
            place.SetTower(tower); // Mark the TowerPlace as occupied and store reference
        }

        UIManager.Instance.onCloseTowerSelectPanel(); 
        return tower; 
    }

    public bool CheckEnoughGold(int cost)
    {
        return _currentGold >= cost; // Validate whether player can afford a cost
    }

    public void SpendGold(int cost)
    {
        _currentGold -= cost; // Decrease current gold by cost
        UpdateGoldUI(); // Refresh gold UI after spending
        Debug.Log($"Gold remaining: {_currentGold}"); 
    }

    public void AddGold(int amount)
    {
        _currentGold += amount; // Increase gold after rewards (e.g., enemy killed)
        UpdateGoldUI(); // Refresh gold UI after gain
        Debug.Log($"Gold gained: {amount}, remaining: {_currentGold}"); 
    }

    private void UpdateGoldUI()
    {
        if (coinTxt != null) { coinTxt.text = _currentGold + ""; } // Update gold UI if reference is assigned
    }

    public void ResetGame()
    {
        _currentGold = startGold; // Reset gold to starting amount
        killNum = 0; // Reset kill counter
        isGameOver = false; // Clear game over flag
        isVictory = false; // Clear victory flag

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.StopSpawnWaves(); // Ensure any running wave coroutine is stopped

            EnemySpawner.Instance.currentWave = 1; // Reset wave index to the first wave
            EnemySpawner.Instance.isSpawning = false; // Reset spawning state flag
            EnemySpawner.Instance.spawnCoroutine = null; // Clear coroutine reference to avoid stale state

            EnemySpawner.Instance.waveCount = 5; // Reset wave count to a default value (ensure this matches your design)
            EnemySpawner.Instance.spawnInterval = 3f; // Reset spawn interval to default value

            EnemySpawner.Instance.playerHP = 10; // Reset player HP to default
            //EnemySpawner.Instance.initHp(); // Refresh HP UI/logic in spawner (if implemented there)

            if (EnemySpawner.Instance.waveTxt != null) { EnemySpawner.Instance.waveTxt.text = "1/" + EnemySpawner.Instance.maxTotalWaves; } // Reset wave progress text
            if (EnemySpawner.Instance.waveTimePanel != null) { EnemySpawner.Instance.waveTimePanel.SetActive(false); } // Hide wave timer UI
            if (EnemySpawner.Instance.progressPanel != null) { EnemySpawner.Instance.progressPanel.SetActive(false); } // Hide progress UI
            if (EnemySpawner.Instance.startBtn != null) { EnemySpawner.Instance.startBtn.enabled = true; } // Re-enable start button for a new run
        }

        if (EnemyManager.Instance != null)
        {
            List<BaseEnemy> allEnemies = EnemyManager.Instance.GetAllAliveEnemies(); // Get all alive enemies tracked by EnemyManager
            foreach (BaseEnemy enemy in allEnemies) { if (enemy != null && enemy.gameObject != null) { Destroy(enemy.gameObject); } } // Destroy each enemy object in the scene

            System.Reflection.FieldInfo field = typeof(EnemyManager).GetField("_aliveEnemies", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance); // Access private list via reflection (not recommended long-term)
            if (field != null) { field.SetValue(EnemyManager.Instance, new List<BaseEnemy>()); } // Replace the list with a fresh empty list to clear tracking
        }

        BaseTower[] allTowers = FindObjectsOfType<BaseTower>(); // Find all towers currently in the scene
        foreach (BaseTower tower in allTowers) { if (tower != null && tower.gameObject != null) { Destroy(tower.gameObject); } } // Destroy all tower objects

        TowerPlace[] allTowerPlaces = FindObjectsOfType<TowerPlace>(); // Find all placement nodes in the scene
        foreach (TowerPlace place in allTowerPlaces)
        {
            if (place != null)
            {
                place.RemoveTower(); // Clear occupancy state so new towers can be placed
                if (place.placeRenderer != null) { place.placeRenderer.material.color = place.normalColor; } // Restore placement node visual color to default
            }
        }

        //Arrow[] allArrows = FindObjectsOfType<Arrow>(); // Find all arrow projectiles
        //foreach (Arrow arrow in allArrows) { if (arrow != null && arrow.gameObject != null) { Destroy(arrow.gameObject); } } // Destroy leftover arrows

        //Cannon[] allCannons = FindObjectsOfType<Cannon>(); // Find all cannon projectiles
        //foreach (Cannon cannon in allCannons) { if (cannon != null && cannon.gameObject != null) { Destroy(cannon.gameObject); } } // Destroy leftover cannon shots

        if (UIManager.Instance != null)
        {
            UIManager.Instance.onCloseVictory(); 
            UIManager.Instance.onCloseLose(); 
            UIManager.Instance.HideUpgradePanel(); 
            UIManager.Instance.onCloseTowerSelectPanel(); 
        }

        UpdateGoldUI(); // Refresh gold UI after resetting
        if (hpTxt != null) { hpTxt.text = EnemySpawner.Instance.playerHP.ToString(); } // Refresh HP UI after resetting
        if (killTxt != null) { killTxt.text = killNum.ToString(); } // Refresh kill UI after resetting
        if (Remaining != null) { Remaining.text = "0"; } // Reset remaining enemy UI display

        Debug.Log("Game has been reset to its initial state.");
    }
}
