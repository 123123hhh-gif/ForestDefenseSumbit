using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

// 定时生成敌人，控制波次 - 单例版本
public class EnemySpawner : MonoBehaviour
{
    // 单例实例（核心）
    private static EnemySpawner _instance;
    // 公共访问属性
    public static EnemySpawner Instance
    {
        get
        {
            // 如果实例为空，尝试在场景中查找
            if (_instance == null)
            {
                _instance = FindObjectOfType<EnemySpawner>();
                
                // 如果查找不到，自动创建一个挂载对象
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
    public Waypoint startWaypoint; // 敌人起始路径点
    public GameObject enemyPrefab; // 敌人预制体
    public float spawnInterval = 1f; // 生成间隔（秒）

    public float enemySpeedMultiplier = 1f; // 敌人速度倍率

    public float enemyHealthMultiplier = 1f; // 敌人生命倍率
    public int waveCount = 5; // 每波生成数量
    public float waveInterval = 10f; // 波次间隔

    [Header("难度限制")]
    public int maxWaveCount = 20; // 每波最大生成数量
    public float minSpawnInterval = 0.5f; // 最小生成间隔
    public int maxTotalWaves = 0; // 最大波数（0表示无限）

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