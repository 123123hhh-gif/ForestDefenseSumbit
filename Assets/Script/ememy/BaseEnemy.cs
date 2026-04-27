using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    // ========== 基础属性 ==========
    public int baseMaxHealth = 100;      // 初始最大生命值（不受Buff影响）
    public float moveSpeed = 2f;
    [SerializeField] private int damageToPlayer = 1;

    [Header("counterAttack")]
    public float counterAttackChance = 0.2f;
    public int counterAttackDamage = 10;

    // ========== 生命值状态（2025-03-16 优化：统一管理） ==========
    private int _maxHealth;            // 当前最大生命值（受Buff加成）
    private int _currentHealth;       // 当前生命值
    private bool _isDead = false;
    private bool _hasReachedEnd = false;

    // 对外只读属性
    public bool IsDead => _isDead;
    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;

    // ========== 移动相关 ==========
    private Waypoint _currentWaypoint;
    public float CurrentMoveSpeed { get; private set; }   // 受Buff影响后的移速

    // ========== 伤害来源（用于反击） ==========
    public BaseTower sourceTower;

    // ========== Buff 系统 ==========
    protected List<Buff> activeBuffs = new List<Buff>();
    private float _moveSpeedMultiplier = 1f;
    private int _maxHealthBonus = 0;        // 最大生命值固定值加成总和

    // ========== 初始化 ==========
    private void Start()
    {
        // 2025-03-16 [优化]：生命值从基础值开始
        _maxHealth = baseMaxHealth;
        _currentHealth = _maxHealth;

        // 收集预制体上挂载的初始Buff
        activeBuffs = new List<Buff>();
        Buff[] initialBuffs = GetComponents<Buff>();
        foreach (Buff buff in initialBuffs)
        {
            activeBuffs.Add(buff);
        }

        // 首次计算受Buff影响的属性
        RecalcStats();

        // 注册到敌人管理器
        EnemyManager.Instance?.AddEnemy(this);
    }

    // ========== Buff 属性重算 ==========
    // 2025-03-16 [优化]：逻辑清晰，生命值同步规则明确
    protected virtual void RecalcStats()
    {
        // 重置修正值
        _moveSpeedMultiplier = 1f;
        _maxHealthBonus = 0;

        // 遍历所有Buff，累加效果
        foreach (Buff buff in activeBuffs)
        {
            switch (buff.buffType)
            {
                case Buff.BuffType.MoveSpeed:
                    _moveSpeedMultiplier *= buff.value;
                    break;
                case Buff.BuffType.MaxHealth:
                    _maxHealthBonus += (int)buff.value;
                    break;
            }
        }

        // ----- 移动速度（乘算）-----
        CurrentMoveSpeed = moveSpeed * _moveSpeedMultiplier;
        CurrentMoveSpeed = Mathf.Max(0.01f, CurrentMoveSpeed);

        // ----- 最大生命值（固定值加成）-----
        int newMaxHealth = baseMaxHealth + _maxHealthBonus;
        if (newMaxHealth != _maxHealth)
        {
            int delta = newMaxHealth - _maxHealth;
            _maxHealth = newMaxHealth;

            // 同步当前生命值：
            // - 若最大生命值增加，当前生命值也增加相同数值（相当于满血附加）
            // - 若最大生命值减少，当前生命值不能超过新最大值
            _currentHealth += delta;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        }
    }

    // ========== Buff 生命周期管理 ==========
    protected virtual void UpdateBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff buff = activeBuffs[i];
            if (!buff.Tick(Time.deltaTime))
            {
                activeBuffs.RemoveAt(i);
                Destroy(buff);          // 销毁组件，防止内存泄漏
                RecalcStats();
            }
        }
    }

    // 外部添加Buff
    public void AddBuff(Buff buff)
    {
        if (buff == null) return;


        if (buff.buffType == Buff.BuffType.Heal)
        {
            Heal((int)buff.value);
            Destroy(buff);          
            return;
        }

        activeBuffs.Add(buff);
        RecalcStats();
    }

    // 外部移除Buff
    public void RemoveBuff(Buff buff)
    {
        if (activeBuffs.Remove(buff))
        {
            Destroy(buff);              // 主动移除时同时销毁组件
            RecalcStats();
        }
    }

    // 动态挂载Buff（便捷方法）
    public Buff ApplyBuff(Buff.BuffType type, float value, float duration = -1f)
    {
        Buff buff = gameObject.AddComponent<Buff>();
        buff.buffType = type;
        buff.value = value;
        buff.Initialize(duration);
        AddBuff(buff);
        return buff;
    }

    // ========== 路径移动 ==========
    public void SetStartWaypoint(Waypoint startWaypoint)
    {
        _currentWaypoint = startWaypoint;
        _hasReachedEnd = false;
    }

    protected virtual void MoveToWaypoint()
    {
        if (_currentWaypoint == null) return;

        Vector3 targetPos = _currentWaypoint.transform.position;
        Vector3 direction = new Vector3(targetPos.x - transform.position.x, 0, targetPos.z - transform.position.z);
        direction.Normalize();

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            transform.rotation = targetRotation;
        }

        // 使用受Buff影响的移速
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            CurrentMoveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPos) < 0.1f)
        {
            if (_currentWaypoint.isLastWaypoint)
            {
                OnReachEnd();
                return;
            }
            _currentWaypoint = _currentWaypoint.nextWaypoint;
        }
    }

    // ========== 终点逻辑 ==========
    private void OnReachEnd()
    {
        if (_hasReachedEnd || _isDead) return;
        _hasReachedEnd = true;
        Debug.Log($"{gameObject.name} 到达终点，扣血{damageToPlayer}");
        GameManager.Instance?.TakeDamage(damageToPlayer);
        DestroyEnemy();
    }

    // ========== 生命值变更（核心） ==========
    // 2025-03-16 [新增]：治疗/增加当前生命值
    public void Heal(int amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        _currentHealth = Mathf.Max(_currentHealth, 0);

        // 更新血条UI（假设MonsterHpBar通过属性读取）
        MonsterHpBar hp = GetComponent<MonsterHpBar>();
        if (hp != null) hp.UpdateHealth(_currentHealth, _maxHealth);  // 需要UI组件适配
    }

    // 受伤逻辑（支持来源塔）
    public void TakeDamage(int damage, BaseTower source = null)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        Debug.Log($"{gameObject.name} 受到 {damage} 伤害，剩余血量：{_currentHealth}");

        // 更新血条UI
        MonsterHpBar hp = GetComponent<MonsterHpBar>();
        if (hp != null) hp.UpdateHealth(_currentHealth, _maxHealth);

        // 反击
        sourceTower = source;
        if (sourceTower != null && _currentHealth > 0)
        {
            TryCounterAttack();
        }

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // ========== 反击 ==========
    private void TryCounterAttack()
    {
        if (Random.Range(0f, 1f) <= counterAttackChance)
        {
            PerformCounterAttack();
        }
    }

    protected virtual void PerformCounterAttack()
    {
        sourceTower?.TakeDamage(counterAttackDamage);
    }

    // ========== 死亡 ==========
    protected virtual void Die()
    {
        _isDead = true;
        Debug.Log($"{gameObject.name} 死亡");

        GameManager.Instance?.AddGold(10);
        GameManager.Instance.killNum++;

        DestroyEnemy();
    }

    // ========== 销毁清理 ==========
    private void DestroyEnemy()
    {
        EnemyManager.Instance?.RemoveEnemy(this);
        Destroy(gameObject, 1.1f);
    }

    private void OnDestroy()
    {
        EnemyManager.Instance?.RemoveEnemy(this);
    }

    // ========== 每帧更新 ==========
    private void Update()
    {
        if (_isDead || _hasReachedEnd || _currentWaypoint == null) return;

       

        // if (Input.GetMouseButtonDown(0))
        // {
        //     ApplyBuff(Buff.BuffType.Heal, 20f, -1f); 
        // }

        UpdateBuffs();      // 处理Buff过期
        MoveToWaypoint();   // 移动
    }
}