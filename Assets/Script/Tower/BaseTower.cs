using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BaseTower : MonoBehaviour,IPointerClickHandler
{
    [Header("core citation")]
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



    protected List<Buff> activeBuffs = new List<Buff>();
   
    public float CurrentAttackRate { get; private set; }
    public float CurrentDamage { get; private set; }
    private float _attackSpeedMultiplier = 1f;
    private float _damageMultiplier = 1f;









    protected virtual void Start()
    {
         _currentHealth = maxHealth;
        _turretFirePoints  = turretRoot.gameObject.GetComponent<TurretFirePoints>();
        Debug.Log("_turretFirePoints = "+_turretFirePoints);
        _attackTimer = 0;


        activeBuffs = new List<Buff>();
        Buff[] initialBuffs = GetComponents<Buff>();
        foreach (Buff buff in initialBuffs)
        {
            activeBuffs.Add(buff);
        }

    }

    public void init(TowerData data)
    {
        _currentData = data;


        RecalcStats();
    }


    protected virtual void RecalcStats()
    {
        if (_currentData == null) return;


        _attackSpeedMultiplier = 1f;
        _damageMultiplier = 1f;


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

            }
        }


        CurrentAttackRate = _currentData.attackRate * _attackSpeedMultiplier;
        CurrentDamage = _currentData.damage * _damageMultiplier;


        CurrentAttackRate = Mathf.Max(0.01f, CurrentAttackRate);
        CurrentDamage = Mathf.Max(0, CurrentDamage);
    }


    public void AddBuff(Buff buff)
    {
        if (buff == null) return;



        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff existingBuff = activeBuffs[i];
            if (existingBuff.buffType == buff.buffType)
            {
                activeBuffs.RemoveAt(i);
                Destroy(existingBuff);
            }
        }

        activeBuffs.Add(buff);
        RecalcStats();
    }

    public void RemoveBuff(Buff buff)
    {
        if (activeBuffs.Remove(buff))
        {
            Destroy(buff);
            RecalcStats();
        }
    }


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



    public Buff ApplyBuff(Buff.BuffType type, float value, float duration = -1f)
    {

        Buff buff = gameObject.AddComponent<Buff>();
        buff.buffType = type;
        buff.value = value;
        buff.Initialize(duration);
        

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