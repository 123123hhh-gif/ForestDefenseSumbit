using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PropSpawner : MonoBehaviour
{
    // 单例
    private static PropSpawner _instance;
    public static PropSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PropSpawner>();
            }
            return _instance;
        }
    }

    [Header("道具生成配置")]
    public Waypoint startWaypoint; // 路径起点（和EnemySpawner一致）
    public List<GameObject> itemPrefabs; // 道具预制体列表
    public float minSpawnInterval = 3f;   // 最小生成间隔（秒）
    public float maxSpawnInterval = 8f;   // 最大生成间隔（秒）
    public int maxItemCount = 10;        // 场景中最大道具数量
    public float yOffset = 1.0f;         // 新增：Y轴向上偏移量（可根据需要调整）

    private List<Waypoint> _waypoints = new List<Waypoint>();
    private Coroutine _spawnCoroutine;
    private List<GameObject> _spawnedItems = new List<GameObject>();
    private bool _isSpawning = false; // 新增：标记是否正在生成，控制协程循环

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
        }
    }

    private void Start()
    {
        CollectWaypoints();
    }

    private void CollectWaypoints()
    {
        _waypoints.Clear();
        if (startWaypoint == null)
        {
            Debug.LogError("startWaypoint 未赋值！");
            return;
        }

        Waypoint current = startWaypoint;
        while (current != null)
        {
            _waypoints.Add(current);
            current = current.nextWaypoint;
        }
    }

    /// <summary>
    /// 开始在路径上生成道具
    /// </summary>
    public void StartSpawnItems()
    {
        if (_spawnCoroutine == null && !_isSpawning) // 双重防护：协程引用+状态标记
        {
            _isSpawning = true;
            _spawnCoroutine = StartCoroutine(SpawnItemLoop());
        }
    }

    /// <summary>
    /// 停止生成道具
    /// </summary>
    public void StopSpawnItems()
    {
        _isSpawning = false; // 先终止循环，再停止协程（更安全）
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null; // 清空引用，避免残留
        }
        // 可选：停止后清理场景中所有已生成的道具
        // ClearAllSpawnedItems();
    }

    /// <summary>
    /// 道具生成协程：随机间隔、随机位置、随机道具
    /// </summary>
    private IEnumerator SpawnItemLoop()
    {
        // 用_isSpawning控制循环，而非死循环，停止时更可控
        while (_isSpawning)
        {
            // 先清理已被拾取的道具
            _spawnedItems.RemoveAll(item => item == null);

            if (_spawnedItems.Count < maxItemCount)
            {
                SpawnRandomItemOnPath();
            }

            // 等待随机时间（等待期间也检查状态，避免无效等待）
            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            float elapsedTime = 0f;
            while (elapsedTime < waitTime && _isSpawning)
            {
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }
        }

        // 循环结束后自动清空协程引用
        _spawnCoroutine = null;
        Debug.Log("道具生成协程正常终止");
    }

    private void SpawnRandomItemOnPath()
{
    if (_waypoints.Count < 2)
    {
        Debug.LogWarning("路径点数量不足，无法生成道具！");
        return;
    }

    if (itemPrefabs == null || itemPrefabs.Count == 0)
    {
        Debug.LogError("没有配置道具预制体！");
        return;
    }

    // 1. 随机选择一段路径（两个相邻Waypoint）
    int segmentIndex = Random.Range(0, _waypoints.Count - 1);
    Waypoint wpA = _waypoints[segmentIndex];
    Waypoint wpB = _waypoints[segmentIndex + 1];

    // 2. 在A和B之间随机一个位置
    float t = Random.Range(0f, 1f);
    Vector3 spawnPos = Vector3.Lerp(wpA.transform.position, wpB.transform.position, t);
    
    spawnPos.y += yOffset;

    // 3. 随机选择一个道具预制体
    int prefabIndex = Random.Range(0, itemPrefabs.Count);
    GameObject prefab = itemPrefabs[prefabIndex];

    // 4. 实例化道具
    GameObject itemObj = Instantiate(prefab, spawnPos, Quaternion.identity);
    _spawnedItems.Add(itemObj);

    Debug.Log($"在路径上生成了道具：{prefab.name}，位置：{spawnPos}");
}

    /// <summary>
    /// 可选：清理所有已生成的道具
    /// </summary>
    public void ClearAllSpawnedItems()
    {
        foreach (var item in _spawnedItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        _spawnedItems.Clear();
    }

    private void OnDestroy()
    {
        StopSpawnItems(); // 销毁时停止协程
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnDisable()
    {
        StopSpawnItems(); // 禁用时停止协程
    }

    // 可选：场景切换时停止（如果开启了DontDestroyOnLoad）
    private void OnApplicationQuit()
    {
        StopSpawnItems();
    }
}