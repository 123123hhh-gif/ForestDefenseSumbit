using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FineMultipleTower : BaseTower
{
    [Header("CONFIG")]
    public GameObject bulletPrefab;

    public bool showDebugGizmos = true;

    public int attackNumber = 3;


    private float _targetRefreshInterval = 0.2f; 
    private float _lastTargetRefreshTime;
    private float _cleanupInterval = 0.5f;
    private float _lastCleanupTime;


    private List<BaseEnemy> _nearestEnemies = new List<BaseEnemy>();

    protected override void Start()
    {
        base.Start();

        MultipleTowerData tmpData = CurrentData as MultipleTowerData;
        attackNumber = tmpData.AttackNumber;
    }

    protected override bool HasTarget()
    {
        return _nearestEnemies.Count > 0;
    }

    protected override void Update()
    {
        base.Update();


        if (Time.time - _lastCleanupTime >= _cleanupInterval)
        {
            CleanUpInvalidEnemies();
            _lastCleanupTime = Time.time;
        }

        if (Time.time - _lastTargetRefreshTime >= _targetRefreshInterval)
        {
            FindTarget(); 
            _lastTargetRefreshTime = Time.time;
        }

    }



    private void CleanUpInvalidEnemies()
    {

        for (int i = _nearestEnemies.Count - 1; i >= 0; i--)
        {
            BaseEnemy enemy = _nearestEnemies[i];      
            if (enemy == null)
            {
                _nearestEnemies.RemoveAt(i);
                continue; 
            }

            if (enemy.IsDead)
            {
                _nearestEnemies.RemoveAt(i);
            }
        }
    }

    protected override void ValidateTarget()
    {
        base.ValidateTarget();
        // Debug.Log("ValidateTarget in FineMultipleTower called");
        
        // if (_targetEnemy == null) return;

        // BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();
        
        // if (enemy == null || enemy.IsDead || !IsTargetInRange())
        // {
        //     _targetEnemy = null;
        // }
    }

    protected override void FindTarget()
    {

        if (EnemyManager.Instance == null)
        {
            Debug.LogWarning("EnemyManager 未找到！");

            _nearestEnemies.Clear();
            return;
        }


        List<BaseEnemy> enemiesInRange = EnemyManager.Instance.GetEnemiesInRange(
            transform.position, 
            _currentData.attackRange
        );


        _nearestEnemies.Clear();


        if (enemiesInRange.Count == 0)
        {
            return;
        }


        List<BaseEnemy> aliveEnemies = new List<BaseEnemy>();
        foreach (BaseEnemy enemy in enemiesInRange)
        {

            if (enemy != null && !enemy.IsDead)
            {
                aliveEnemies.Add(enemy);
            }
        }


        if (aliveEnemies.Count == 0)
        {
            return;
        }


        aliveEnemies.Sort((a, b) => 
        {

            float distanceA = Vector3.Distance(transform.position, a.transform.position);
            float distanceB = Vector3.Distance(transform.position, b.transform.position);

            return distanceA.CompareTo(distanceB);
        });

        int takeCount = Mathf.Min(aliveEnemies.Count, attackNumber);
        for (int i = 0; i < takeCount; i++)
        {
            _nearestEnemies.Add(aliveEnemies[i]);
        }
        
        if(_nearestEnemies.Count == 1)
        {
            Debug.Log("找到1个最近的存活敌人");
        }

        Debug.Log($"找到{_nearestEnemies.Count}个最近的存活敌人，攻击范围：{_currentData.attackRange}");
    }


    protected override void RotateTurretToTarget()
    {
        if(_nearestEnemies.Count == 1)
        {
            Debug.Log("找到1个最近的存活敌人");
        }
        if (_nearestEnemies.Count == 0 || turretRoot == null) return;
        BaseEnemy firstEnemy = _nearestEnemies[0];
        if (firstEnemy == null) return;

        _targetEnemy = firstEnemy.transform;

        Vector3 direction = _targetEnemy.position - turretRoot.position;
       
        if (direction.magnitude < 0.1f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        turretRoot.rotation = Quaternion.Lerp(
            turretRoot.rotation, 
            targetRotation, 
            Time.deltaTime * _currentData.rotateSpeed
        );
    }


    protected override void Shoot()
    {
        if(_nearestEnemies.Count == 1)
        {
            Debug.Log("找到1个最近的存活敌人");
        }
        if (_targetEnemy == null || bulletPrefab == null || _turretFirePoints == null) 
        {
            Debug.LogWarning("塔射击条件不足：目标/预制体/射击点管理器为空");
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("塔没有可用的射击点！");
            return;
        }

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null) continue;

            for (int i = 0; i < _nearestEnemies.Count; i++)
            {
                BaseEnemy enemy = _nearestEnemies[i];
                // SubscribeEnemyEvents(enemy);
                GenerateBullet(firePoint, enemy.transform);
            }

            if(bulletBgm != null)
            {
                 AudioManager.Instance.PlayBattleSFX(bulletBgm);
            }
        }
    }



    private void RemoveEnemyFromList(BaseEnemy enemy)
    {
        if (enemy == null) return;

        if (_nearestEnemies.Contains(enemy))
        {
            _nearestEnemies.Remove(enemy);
            Debug.Log($"从炮塔目标列表移除敌人：{enemy.gameObject.name}，剩余目标数：{_nearestEnemies.Count}");
        }
    }


    private void GenerateBullet(Transform firePoint, Transform target)
    {

        Vector3 spawnPos = firePoint.TransformPoint(CurrentData.bulletPosOffset);

        Vector3 dirToEnemy = target.position - spawnPos;
        Quaternion targetRot = Quaternion.LookRotation(dirToEnemy);
        targetRot *= Quaternion.Euler(CurrentData.bulletRotOffset);


        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, targetRot);
        bulletObj.transform.SetParent(null);

        ParticleMoverBullet bulletMover = bulletObj.GetComponentInChildren<ParticleMoverBullet>();
        if (bulletMover != null)
        {


            bulletMover.damage = (int)CurrentDamage;

            bulletMover.fatherTower = this;
            bulletMover.SetTarget(target);
        }
        else
        {
            Debug.LogWarning($"子弹{bulletObj.name}缺少ParticleMoverBullet组件！");
        }
    }


    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || _turretFirePoints == null || _targetEnemy == null) return;

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0) return;

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null) continue;


            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);


            Vector3 spawnPos = firePoint.TransformPoint(CurrentData.bulletPosOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnPos, 0.1f);


            Vector3 dirToEnemy = _targetEnemy.position - spawnPos;
            Quaternion targetRot = Quaternion.LookRotation(dirToEnemy) * Quaternion.Euler(CurrentData.bulletRotOffset);
            Gizmos.DrawLine(spawnPos, spawnPos + targetRot * Vector3.forward * 2f);
        }
    }
}
