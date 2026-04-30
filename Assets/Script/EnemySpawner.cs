using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class EnemySpawner : MonoBehaviour
{

    private static EnemySpawner _instance;

    public static EnemySpawner Instance
    {
        get
        {

            if (_instance == null)
            {
                _instance = FindObjectOfType<EnemySpawner>();
                

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

    [Header("生成配置")]
    public Waypoint startWaypoint; 
    public GameObject enemyPrefab; 
    public float spawnInterval = 1f;

    public float enemySpeedMultiplier = 1f;

    public float enemyHealthMultiplier = 1f; 
    public int waveCount = 5; 
    public float waveInterval = 10f; 

    [Header("难度限制")]
    public int maxWaveCount = 20; 
    public float minSpawnInterval = 0.5f; 
    public int maxTotalWaves = 0;

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
        if(progressPanel != null)
        {
            progressPanel.SetActive(true);
        }
        startBtn.enabled = false;


        
        StartSpawnWaves();

        PropSpawner.Instance.ClearAllSpawnedItems();
        PropSpawner.Instance.StartSpawnItems();
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
            Debug.Log($"生成第{currentWave}波敌人");
            waveTxt.text = currentWave+"/"+maxTotalWaves;
            

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
                   
                    waveCountdownTxt.text = Mathf.FloorToInt(remainingTime)+"";
                  
                    yield return new WaitForSeconds(0.1f);
                    remainingTime -= 0.1f;
                }
                
                waveTimePanel.SetActive(false);
            }
            else
            {

                yield return new WaitForSeconds(waveInterval);
            }

            
            if(maxTotalWaves == 0)
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
        Debug.Log($"敌人生成结束，共生成{currentWave - 1}波");
    }


    private void SpawnEnemy()
    {
        if (enemyPrefab == null || startWaypoint == null)
        {
            Debug.LogError("敌人预制体或起始路径点未赋值！");
            return;
        }


        GameObject enemyObj = Instantiate(enemyPrefab, startWaypoint.transform.position, Quaternion.identity);

        BaseEnemy enemy = enemyObj.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.moveSpeed *= enemySpeedMultiplier;
            enemy.baseMaxHealth = Mathf.RoundToInt(enemy.baseMaxHealth * enemyHealthMultiplier);
        
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
        playerHP  = playerHP - num;
        if(playerHP <= 0)
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
        playerHP  = playerHP + num;
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