using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : MonoBehaviour
{

    public int baseMaxHealth = 100;     
    public float moveSpeed = 2f;
    [SerializeField] private int damageToPlayer = 1;

    [Header("counterAttack")]
    public float counterAttackChance = 0.2f;
    public int counterAttackDamage = 10;


    private int _maxHealth;            
    private int _currentHealth;      
    private bool _isDead = false;
    private bool _hasReachedEnd = false;


    public bool IsDead => _isDead;
    public int MaxHealth => _maxHealth;
    public int CurrentHealth => _currentHealth;


    private Waypoint _currentWaypoint;
    public float CurrentMoveSpeed { get; private set; }   


    public BaseTower sourceTower;

  
    protected List<Buff> activeBuffs = new List<Buff>();
    private float _moveSpeedMultiplier = 1f;
    private int _maxHealthBonus = 0;       

  
    private void Start()
    {

        _maxHealth = baseMaxHealth;
        _currentHealth = _maxHealth;


        activeBuffs = new List<Buff>();
        Buff[] initialBuffs = GetComponents<Buff>();
        foreach (Buff buff in initialBuffs)
        {
            activeBuffs.Add(buff);
        }


        RecalcStats();


        EnemyManager.Instance?.AddEnemy(this);
    }


    protected virtual void RecalcStats()
    {

        _moveSpeedMultiplier = 1f;
        _maxHealthBonus = 0;


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


        CurrentMoveSpeed = moveSpeed * _moveSpeedMultiplier;
        CurrentMoveSpeed = Mathf.Max(0.01f, CurrentMoveSpeed);


        int newMaxHealth = baseMaxHealth + _maxHealthBonus;
        if (newMaxHealth != _maxHealth)
        {
            int delta = newMaxHealth - _maxHealth;
            _maxHealth = newMaxHealth;


            _currentHealth += delta;
            _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
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


    public void AddBuff(Buff buff)
    {
        if (buff == null) return;


        if (buff.buffType == Buff.BuffType.Heal)
        {
            Heal((int)buff.value);
            Destroy(buff);          
            return;
        }


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


    public Buff ApplyBuff(Buff.BuffType type, float value, float duration = -1f)
    {
        Buff buff = gameObject.AddComponent<Buff>();
        buff.buffType = type;
        buff.value = value;
        buff.Initialize(duration);
        AddBuff(buff);
        return buff;
    }


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


    private void OnReachEnd()
    {
        if (_hasReachedEnd || _isDead) return;
        _hasReachedEnd = true;
        Debug.Log($"{gameObject.name} 到达终点，扣血{damageToPlayer}");
        GameManager.Instance?.TakeDamage(damageToPlayer);
        DestroyEnemy();
    }


    public void Heal(int amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Min(_currentHealth + amount, _maxHealth);
        _currentHealth = Mathf.Max(_currentHealth, 0);

        MonsterHpBar hp = GetComponent<MonsterHpBar>();
        if (hp != null) hp.UpdateHealth(_currentHealth, _maxHealth);  
    }


    public void TakeDamage(int damage, BaseTower source = null)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);
        // Debug.Log($"{gameObject.name} 受到 {damage} 伤害，剩余血量：{_currentHealth}");


        MonsterHpBar hp = GetComponent<MonsterHpBar>();
        if (hp != null) hp.UpdateHealth(_currentHealth, _maxHealth);

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


    protected virtual void Die()
    {
        _isDead = true;
        Debug.Log($"{gameObject.name} 死亡");

        GameManager.Instance?.AddGold(10);
        GameManager.Instance.killNum++;

        DestroyEnemy();
    }


    private void DestroyEnemy()
    {
        EnemyManager.Instance?.RemoveEnemy(this);
        Destroy(gameObject, 1.1f);
    }

    private void OnDestroy()
    {
        EnemyManager.Instance?.RemoveEnemy(this);
    }


    private void Update()
    {
        if (_isDead || _hasReachedEnd || _currentWaypoint == null) return;

       

        // if (Input.GetMouseButtonDown(0))
        // {
        //     ApplyBuff(Buff.BuffType.Heal, 20f, -1f); 
        // }

        UpdateBuffs();     
        MoveToWaypoint();   
    }
}