using UnityEngine;
using System.Collections.Generic;

public class BaseTower : MonoBehaviour
{
    [Header("核心引用")]
    public Transform baseTransform; 
    public Transform turretRoot; 
    public TowerData initialData; 

    [Header("State")]
    protected TowerData _currentData;
    protected Transform _targetEnemy; 
    private float _attackTimer;
    [HideInInspector]
    public TurretFirePoints _turretFirePoints;

    public AudioClip bulletBgm;


    public TowerData CurrentData => _currentData;

    private void Start()
    {
        
        _currentData = initialData;

        _turretFirePoints  = turretRoot.gameObject.GetComponent<TurretFirePoints>();
        Debug.Log("_turretFirePoints = "+_turretFirePoints);

        
        _attackTimer = 0;
    }

    private void Update()
    {
       
        ValidateTarget();

       
        if (_targetEnemy == null)
        {
            FindTarget();
            return;
        }

       
        RotateTurretToTarget();
        AttackTarget();
    }




    private void ValidateTarget()
    {
        if (_targetEnemy == null) return;

        BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();
        
        if (enemy == null || enemy.IsDead || !IsTargetInRange())
        {
            _targetEnemy = null;
        }
    }

   
    private void FindTarget()
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogWarning("EnemyManager 未找到！");
            return;
        }

       
        List<BaseEnemy> enemiesInRange = EnemyManager.Instance.GetEnemiesInRange(
            transform.position, 
            _currentData.attackRange
        );

        if (enemiesInRange.Count == 0)
        {
            _targetEnemy = null;
            return;
        }

       
        BaseEnemy closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (BaseEnemy enemy in enemiesInRange)
        {
            if (enemy == null || enemy.IsDead) continue;

            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        _targetEnemy = closestEnemy?.transform;
    }

   
    private void RotateTurretToTarget()
    {
        if (_targetEnemy == null || turretRoot == null) return;

        Vector3 direction = _targetEnemy.position - turretRoot.position;
       
        if (direction.magnitude < 0.1f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        turretRoot.rotation = Quaternion.Lerp(
            turretRoot.rotation, 
            targetRotation, 
            Time.deltaTime * _currentData.rotateSpeed
        );
    }

    private bool IsTurretFacingTarget()
    {
        if (_targetEnemy == null || turretRoot == null) return false;
        
       
        Vector3 currentForward = turretRoot.forward;
        Vector3 targetDirection = (_targetEnemy.position - turretRoot.position).normalized;
        
       
        float angle = Vector3.Angle(currentForward, targetDirection);
        return angle < 25f; 
    }

   
    private void AttackTarget()
    {

        if (_targetEnemy == null) return;


       
        if (!IsTurretFacingTarget())
        {
           
            return;
        }

        _attackTimer += Time.deltaTime;
        // Debug.Log("AttackTarge3 _attackTimer = "+ _attackTimer+" _currentData.attackRate = "+ _currentData.attackRate);
        if (_attackTimer < _currentData.attackRate) return;

        _attackTimer = 0;
        Shoot();
    }

    
    protected virtual void Shoot()
    {
        
        
        if (_turretFirePoints == null)
        {
            Debug.LogWarning("射击点管理器未初始化，无法射击！");
            return;
        }

       
        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning($"炮塔{turretRoot.name}没有可用的射击点！");
            return;
        }

       
        foreach (Transform firePoint in firePoints)
        {
            if (firePoint != null && _targetEnemy != null)
            {
                Debug.Log($"从{firePoint.name}射击{_targetEnemy.name}，伤害：{_currentData.damage}");
                // 给敌人扣血
                BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();
                if (enemy != null) enemy.TakeDamage(_currentData.damage);
            }
        }
    }

    
    public bool Upgrade()
    {
        
        if (_currentData.nextLevelData == null)
        {
            Debug.Log($"{_currentData.towerName}已达满级");
            return false;
        }

       
        if (!GameManager.Instance.CheckEnoughGold(_currentData.nextLevelData.cost))
        {
            Debug.Log("金币不足，无法升级");
            return false;
        }

        
        GameManager.Instance.SpendGold(_currentData.nextLevelData.cost);
       
        _currentData = _currentData.nextLevelData;

        Debug.Log($"{gameObject.name}升级为{_currentData.towerName}");
        return true;
    }

    
    private bool IsTargetInRange()
    {
        if (_targetEnemy == null) return false;
        return Vector3.Distance(transform.position, _targetEnemy.position) <= _currentData.attackRange;
    }
}