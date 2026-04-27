using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BaseTower : MonoBehaviour,IPointerClickHandler
{
    [Header("核心引用")]
    public Transform baseTransform; 
    public Transform turretRoot; 

    [Header("State")]
    protected TowerData _currentData;
    protected Transform _targetEnemy; 
    private float _attackTimer;
    [HideInInspector]
    public TurretFirePoints _turretFirePoints;

    public AudioClip bulletBgm;


    public TowerData CurrentData => _currentData;

    public int maxHealth = 100;
    private int _currentHealth;
    private bool _isDead = false;
    public bool IsDead => _isDead;

    [HideInInspector]
    public TowerPlace towerPlace;


    // ========== 2025-03-16: 新增 Buff 系统 ==========
    protected List<Buff> activeBuffs = new List<Buff>();
    // 缓存当前计算值
    public float CurrentAttackRate { get; private set; }
    public float CurrentDamage { get; private set; }
    private float _attackSpeedMultiplier = 1f;
    private float _damageMultiplier = 1f;
    // ==============================================








    protected virtual void Start()
    {
         _currentHealth = maxHealth;
        _turretFirePoints  = turretRoot.gameObject.GetComponent<TurretFirePoints>();
        Debug.Log("_turretFirePoints = "+_turretFirePoints);
        _attackTimer = 0;


        // 2025-03-16: 初始化 Buff 列表，收集挂在自身上的所有Buff组件
        activeBuffs = new List<Buff>();
        Buff[] initialBuffs = GetComponents<Buff>();
        foreach (Buff buff in initialBuffs)
        {
            activeBuffs.Add(buff);
        }
        // 注意：此时 _currentData 可能为 null，属性计算延迟到 init 中进行
    }

    public void init(TowerData data)
    {
        _currentData = data;

        // 2025-03-16: 获取到数据后立即重新计算属性
        RecalcStats();
    }

     // 2025-03-16: 重新计算所有受 Buff 影响的属性
    protected virtual void RecalcStats()
    {
        if (_currentData == null) return;

        // 重置乘数
        _attackSpeedMultiplier = 1f;
        _damageMultiplier = 1f;

        // 遍历所有 Buff，根据类型累加效果（乘算）
        foreach (Buff buff in activeBuffs)
        {
            switch (buff.buffType)
            {
                case Buff.BuffType.AttackSpeed:
                    _attackSpeedMultiplier *= buff.value;
                    break;
                case Buff.BuffType.Damage:
                    _damageMultiplier *= buff.value;
                    break;
                // 塔不需要 MoveSpeed / MaxHealth 处理，忽略
            }
        }

        // 应用乘数得到最终属性
        CurrentAttackRate = _currentData.attackRate * _attackSpeedMultiplier;
        CurrentDamage = _currentData.damage * _damageMultiplier;

        // 确保属性不低于合理下限
        CurrentAttackRate = Mathf.Max(0.01f, CurrentAttackRate);
        CurrentDamage = Mathf.Max(0, CurrentDamage);
    }

    // 2025-03-16: 外部添加 Buff（例如技能、其他塔）
    public void AddBuff(Buff buff)
    {
        if (buff == null) return;
        activeBuffs.Add(buff);
        RecalcStats();
    }

    // 2025-03-16: 移除指定 Buff
    public void RemoveBuff(Buff buff)
    {
        if (activeBuffs.Remove(buff))
        {
            Destroy(buff);
            RecalcStats();
        }
    }

    // 2025-03-16: 每帧处理 Buff 过期
    protected virtual void UpdateBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff buff = activeBuffs[i];
            if (!buff.Tick(Time.deltaTime))
            {
                activeBuffs.RemoveAt(i);
                Destroy(buff); 
                RecalcStats();
            }
        }
    }
    // ==============================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            UIManager.Instance.ShowUpgradePanel(this);
        }
    }



    protected virtual void Update()
    {


        // if (Input.GetMouseButtonDown(0))
        // {
        //    ApplyBuff(Buff.BuffType.Damage, 1.5f, -1f);
        // }

         if (_isDead) return;


        // 2025-03-16: 每帧更新 Buff 状态
        UpdateBuffs();
       
        ValidateTarget();

       
        if (!HasTarget())
        {
            FindTarget();
            return;
        }

       
        RotateTurretToTarget();
        AttackTarget();
    }

    protected virtual bool HasTarget()
    {
        if (_targetEnemy == null)
        {
            return false;
        }else
        {
            return true;
        }
    }


    protected virtual void ValidateTarget()
    {
        if (_targetEnemy == null) return;

        BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();
        
        if (enemy == null || enemy.IsDead || !IsTargetInRange())
        {
            _targetEnemy = null;
        }
    }

   
    protected virtual void FindTarget()
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

   
    protected virtual void RotateTurretToTarget()
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

    protected virtual bool IsTurretFacingTarget()
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
        
        // if (_attackTimer < _currentData.attackRate) return;

        Debug.Log($"攻击间隔：{_attackTimer}，当前攻击间隔：{CurrentAttackRate}");
        // 2025-03-16: 使用计算后的攻击间隔 CurrentAttackRate
        if (_attackTimer < CurrentAttackRate) return;

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
               // 2025-03-16: 使用计算后的伤害 CurrentDamage
                Debug.Log($"从{firePoint.name}射击{_targetEnemy.name}，伤害：{CurrentDamage}");
                
                BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();
                // if (enemy != null) enemy.TakeDamage(_currentData.damage);
                if (enemy != null) enemy.TakeDamage((int)CurrentDamage, this); 
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

        // 2025-03-16: 执行升级：替换数据，重新计算属性
        _currentData = _currentData.nextLevelData;
        RecalcStats();

        Debug.Log($"{gameObject.name}升级为{_currentData.towerName}");
        return true;
    }

    
    private bool IsTargetInRange()
    {
        if (_targetEnemy == null) return false;
        return Vector3.Distance(transform.position, _targetEnemy.position) <= _currentData.attackRange;
    }



    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name} 受到 {damage} 伤害，剩余血量：{_currentHealth}");

        TowerHpBar hp = this.GetComponent<TowerHpBar>();
        hp.TakeDamage(damage);
        hp.maxHp = maxHealth;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }


    // 2025-03-16: 动态挂载 Buff 的便捷方法
    /// <param name="type">Buff 类型</param>
    /// <param name="value">数值（倍率/固定值）</param>
    /// <param name="duration">持续时间，-1 为永久</param>
    /// <returns>新创建的 Buff 组件</returns>
    public Buff ApplyBuff(Buff.BuffType type, float value, float duration = -1f)
    {
        // 创建组件
        Buff buff = gameObject.AddComponent<Buff>();
        buff.buffType = type;
        buff.value = value;
        buff.Initialize(duration);
        
        // 添加到有效列表并刷新属性
        AddBuff(buff);
        
        return buff;
    }


    protected virtual void Die()
    {
        _isDead = true;
        Debug.Log($"{gameObject.name} 死亡");

        
        // DestroyEnemy();
        towerPlace.RemoveTower();
    }





    public void DestroyTower(bool isSell = false)
    {

        CleanupTower();
        if (isSell && GameManager.Instance != null)
        {
            int refundGold = Mathf.RoundToInt(_currentData.cost * 0.7f);
            GameManager.Instance.AddGold(refundGold);
            Debug.Log($"出售炮塔{gameObject.name}，返还金币：{refundGold}");
        }

        Destroy(gameObject);
    }


    private void CleanupTower()
    {
        StopAllCoroutines();
        _targetEnemy = null;
        _turretFirePoints = null;
        // foreach (Transform child in transform)
        // {
        //     if (child.CompareTag("Bullet") || child.CompareTag("Effect"))
        //     {
        //         Destroy(child.gameObject);
        //     }
        // }

    }


    private void OnDestroy()
    {

        _targetEnemy = null;
        _currentData = null;
        towerPlace = null;
        Debug.Log($"炮塔{gameObject.name}已被销毁，OnDestroy触发");
    }







}