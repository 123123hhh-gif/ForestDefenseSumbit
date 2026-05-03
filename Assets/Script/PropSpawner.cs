using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PropSpawner : MonoBehaviour
{

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

    [Header("Prop generation configuration")]
    public Waypoint startWaypoint; 
    public List<GameObject> itemPrefabs; 
    public float minSpawnInterval = 3f;   
    public float maxSpawnInterval = 8f;   
    public int maxItemCount = 10;        
    public float yOffset = 1.0f;        

    private List<Waypoint> _waypoints = new List<Waypoint>();
    private Coroutine _spawnCoroutine;
    private List<GameObject> _spawnedItems = new List<GameObject>();
    private bool _isSpawning = false;

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
            Debug.LogError("startWaypoint Unassigned!");
            return;
        }

        Waypoint current = startWaypoint;
        while (current != null)
        {
            _waypoints.Add(current);
            current = current.nextWaypoint;
        }
    }


    public void StartSpawnItems()
    {
        if (_spawnCoroutine == null && !_isSpawning) 
        {
            _isSpawning = true;
            _spawnCoroutine = StartCoroutine(SpawnItemLoop());
        }
    }


    public void StopSpawnItems()
    {
        _isSpawning = false; 
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null; 
        }

        // ClearAllSpawnedItems();
    }


    private IEnumerator SpawnItemLoop()
    {

        while (_isSpawning)
        {

            _spawnedItems.RemoveAll(item => item == null);

            if (_spawnedItems.Count < maxItemCount)
            {
                SpawnRandomItemOnPath();
            }


            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            float elapsedTime = 0f;
            while (elapsedTime < waitTime && _isSpawning)
            {
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.1f;
            }
        }


        _spawnCoroutine = null;

    }

    private void SpawnRandomItemOnPath()
{
    if (_waypoints.Count < 2)
    {
        Debug.LogWarning("Insufficient number of path points, unable to generate props!");
        return;
    }

    if (itemPrefabs == null || itemPrefabs.Count == 0)
    {
        Debug.LogError("No prop prefab is configured!");
        return;
    }


    int segmentIndex = Random.Range(0, _waypoints.Count - 1);
    Waypoint wpA = _waypoints[segmentIndex];
    Waypoint wpB = _waypoints[segmentIndex + 1];


    float t = Random.Range(0f, 1f);
    Vector3 spawnPos = Vector3.Lerp(wpA.transform.position, wpB.transform.position, t);
    
    spawnPos.y += yOffset;


    int prefabIndex = Random.Range(0, itemPrefabs.Count);
    GameObject prefab = itemPrefabs[prefabIndex];


    GameObject itemObj = Instantiate(prefab, spawnPos, Quaternion.identity);
    _spawnedItems.Add(itemObj);


}


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
        StopSpawnItems(); 
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private void OnDisable()
    {
        StopSpawnItems(); 
    }


    private void OnApplicationQuit()
    {
        StopSpawnItems();
    }
}