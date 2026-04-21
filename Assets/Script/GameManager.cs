using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance;

    [Header("Basic Configuration")]
    public int startGold = 100;
    public TextMeshProUGUI coinTxt;
    public TextMeshProUGUI hpTxt;
    public TextMeshProUGUI killTxt;
    public TextMeshProUGUI Remaining;

    public AudioClip bgmWarriors;



    public int killNum = 0;


    private int _currentGold;


    private float timer = 0f;
    public float callInterval = 1f;

    private bool isGameOver = false;
    private bool isVictory = false;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        _currentGold = startGold;
        UpdateGoldUI();
    }

    void Start()
    {
        AudioManager.Instance.PlayBGM(bgmWarriors);
    }

    void Update()
    {

        timer += Time.deltaTime;


        if (timer >= callInterval)
        {

            PerSecondMethod();
            CheckVictoryCondition(); 
            timer -= callInterval;
        }
    }


    public void PerSecondMethod()
    {
        List<BaseEnemy> enemies = EnemyManager.Instance.GetAllAliveEnemies();
        Remaining.text = enemies.Count+"";
        killTxt.text = killNum+"";
    }





    private void CheckVictoryCondition()
    {
        if (isGameOver || EnemySpawner.Instance.maxTotalWaves == 0 || EnemySpawner.Instance.isSpawning)
        {
            return;
        }

        bool allWavesSpawned = EnemySpawner.Instance.currentWave >= EnemySpawner.Instance.maxTotalWaves;

        bool noEnemiesAlive = EnemyManager.Instance.GetAllAliveEnemies().Count == 0;

        bool playerAlive = EnemySpawner.Instance.playerHP > 0;


        if (allWavesSpawned && noEnemiesAlive && playerAlive)
        {
            OnVictory();
        }
    }


    private void OnVictory()
    {
        isGameOver = true;
        isVictory = true;
        EnemySpawner.Instance.StopSpawnWaves();
        UIManager.Instance.onOpenVictory();


        Debug.Log("gameVictory!");
    }



    public void TakeDamage(int damageToPlayer)
    {
        if (isGameOver) return;
        
        int num = EnemySpawner.Instance.LoseHP(damageToPlayer);
        Debug.Log("num ="+num);
        hpTxt.text = num+"";
        if(num <= 0)
        {
            isGameOver = true;
           
            UIManager.Instance.onOpenLose();
            EnemySpawner.Instance.StopSpawnWaves();
            Debug.Log("gameLose!");
        }
    }





    public BaseTower PlaceTower(TowerPlace place,TowerData _data)
    {
        SpendGold(_data.cost);


        Vector3 spawnPos = place.PlacePosition;
        spawnPos.y += 0.2f;

        GameObject towerObj = Instantiate(_data.towerPrefab, spawnPos, Quaternion.identity);
        BaseTower tower = towerObj.GetComponent<BaseTower>();
        if (tower != null)
        {
            tower.init(_data);
            tower.towerPlace = place;

            place.SetTower(tower);
        }
        UIManager.Instance.onCloseTowerSelectPanel();
        return tower;
    }


    public bool CheckEnoughGold(int cost)
    {
        return _currentGold >= cost;
    }


    public void SpendGold(int cost)
    {
        _currentGold -= cost;
        UpdateGoldUI();
        Debug.Log($"剩余金币：{_currentGold}");
    }


    public void AddGold(int amount)
    {
        _currentGold += amount;
        UpdateGoldUI();
        Debug.Log($"获得金币：{amount}，剩余：{_currentGold}");
    }


    private void UpdateGoldUI()
    {
        if (coinTxt != null)
        {
            coinTxt.text = _currentGold+"";
        }
    }


public void ResetGame()
{

    _currentGold = startGold; 
    killNum = 0; 
    isGameOver = false; 
    isVictory = false;


    if (EnemySpawner.Instance != null)
    {
      
        EnemySpawner.Instance.StopSpawnWaves();
        
        
        EnemySpawner.Instance.currentWave = 1;
        EnemySpawner.Instance.isSpawning = false;
        EnemySpawner.Instance.spawnCoroutine = null;
        

        EnemySpawner.Instance.waveCount = 5; 
        EnemySpawner.Instance.spawnInterval = 3f; 
        

        EnemySpawner.Instance.playerHP = 10;
        EnemySpawner.Instance.initHp();
        

        if (EnemySpawner.Instance.waveTxt != null)
        {
            EnemySpawner.Instance.waveTxt.text = "1/" + EnemySpawner.Instance.maxTotalWaves;
        }
        if (EnemySpawner.Instance.waveTimePanel != null)
        {
            EnemySpawner.Instance.waveTimePanel.SetActive(false);
        }
        if (EnemySpawner.Instance.progressPanel != null)
        {
            EnemySpawner.Instance.progressPanel.SetActive(false);
        }
        if (EnemySpawner.Instance.startBtn != null)
        {
            EnemySpawner.Instance.startBtn.enabled = true; 
        }
    }


    if (EnemyManager.Instance != null)
    {

        List<BaseEnemy> allEnemies = EnemyManager.Instance.GetAllAliveEnemies();
        foreach (BaseEnemy enemy in allEnemies)
        {
            if (enemy != null && enemy.gameObject != null)
            {
                Destroy(enemy.gameObject);
            }
        }
 
        System.Reflection.FieldInfo field = typeof(EnemyManager).GetField("_aliveEnemies", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            List<BaseEnemy> emptyList = new List<BaseEnemy>();
            field.SetValue(EnemyManager.Instance, emptyList);
        }
    }



    BaseTower[] allTowers = FindObjectsOfType<BaseTower>();
    foreach (BaseTower tower in allTowers)
    {
        if (tower != null && tower.gameObject != null)
        {
            Destroy(tower.gameObject);
        }
    }

    TowerPlace[] allTowerPlaces = FindObjectsOfType<TowerPlace>();
    foreach (TowerPlace place in allTowerPlaces)
    {
        if (place != null)
        {
            place.RemoveTower(); 

            if (place.placeRenderer != null)
            {
                place.placeRenderer.material.color = place.normalColor;
            }
        }
    }


    Arrow[] allArrows = FindObjectsOfType<Arrow>();
    foreach (Arrow arrow in allArrows)
    {
        if (arrow != null && arrow.gameObject != null)
        {
            Destroy(arrow.gameObject);
        }
    }

    Cannon[] allCannons = FindObjectsOfType<Cannon>();
    foreach (Cannon cannon in allCannons)
    {
        if (cannon != null && cannon.gameObject != null)
        {
            Destroy(cannon.gameObject);
        }
    }


    if (UIManager.Instance != null)
    {
       
        UIManager.Instance.onCloseVictory();
        UIManager.Instance.onCloseLose();

        
      
        UIManager.Instance.HideUpgradePanel();
        UIManager.Instance.onCloseTowerSelectPanel();
    }


    UpdateGoldUI(); 
    if (hpTxt != null)
    {
        hpTxt.text = EnemySpawner.Instance.playerHP.ToString(); 
    }
    if (killTxt != null)
    {
        killTxt.text = killNum.ToString(); 
    }
    if (Remaining != null)
    {
        Remaining.text = "0"; 
    }

    Debug.Log("The game has been completely reset to its initial state!");
}



}



