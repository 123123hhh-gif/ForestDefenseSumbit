using UnityEngine;
using System.Collections.Generic;


public class EnemyManager : MonoBehaviour
{

    public static EnemyManager Instance;


    private List<BaseEnemy> _aliveEnemies = new List<BaseEnemy>();

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
    }


    public void AddEnemy(BaseEnemy enemy)
    {
        if (enemy == null || _aliveEnemies.Contains(enemy)) return;
        _aliveEnemies.Add(enemy);
    }


    public void RemoveEnemy(BaseEnemy enemy)
    {
        if (enemy == null || !_aliveEnemies.Contains(enemy)) return;
        _aliveEnemies.Remove(enemy);
    }


    public List<BaseEnemy> GetEnemiesInRange(Vector3 towerPosition, float range)
    {
        List<BaseEnemy> enemiesInRange = new List<BaseEnemy>();
        

        foreach (BaseEnemy enemy in _aliveEnemies)
        {
            if (enemy == null || enemy.IsDead) continue;
            
            float distance = Vector3.Distance(towerPosition, enemy.transform.position);
            if (distance <= range)
            {
                enemiesInRange.Add(enemy);
            }
        }

        return enemiesInRange;
    }


    public List<BaseEnemy> GetAllAliveEnemies()
    {

        _aliveEnemies.RemoveAll(enemy => enemy == null || enemy.IsDead);
        return new List<BaseEnemy>(_aliveEnemies);
    }


    private void OnGUI()
    {
        // GUI.Label(new Rect(10, 50, 200, 30), $"存活敌人：{_aliveEnemies.Count}");
    }
}