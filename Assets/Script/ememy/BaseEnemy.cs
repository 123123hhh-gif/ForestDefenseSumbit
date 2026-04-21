using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("敌人属性")]
    public int maxHealth = 100;


    public float moveSpeed = 2f;

    public int damageToPlayer = 1;

    private int _currentHealth;
    private bool _isDead = false;
    private Waypoint _currentWaypoint; // 当前目标路径点
     private bool _hasReachedEnd = false;

    // 公开属性
    public bool IsDead => _isDead;
    


    private void Start()
    {
        _currentHealth = maxHealth;
        // 生成时自动注册到管理器
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.AddEnemy(this);
        }
    }

    private void Update()
    {
        if (_isDead || _hasReachedEnd || _currentWaypoint == null) return;

        addDebuff();

        MoveToWaypoint();
    }

    // 设置起始路径点
    public void SetStartWaypoint(Waypoint startWaypoint)
    {
        _currentWaypoint = startWaypoint;
         _hasReachedEnd = false;
    }

    protected virtual void addDebuff()
    {

    }

    // 向路径点移动
    protected virtual void MoveToWaypoint()
    {
        if (_currentWaypoint == null) return;

        // 1. 计算方向（锁定Y轴，只在XZ平面移动）
        Vector3 targetPos = _currentWaypoint.transform.position;
        Vector3 direction = new Vector3(targetPos.x - transform.position.x, 0, targetPos.z - transform.position.z);
        direction.Normalize();

        // 2. 仅在水平方向上转向，避免上下旋转
        if (direction.magnitude > 0.1f)
        {
            // 计算目标旋转，强制Y轴为当前值，只修改X和Z轴
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
            
            // 瞬间转向，无缓冲
            transform.rotation = targetRotation;
        }

        // 3. 用 MoveTowards 替代 Translate，移动更精准
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPos, 
            moveSpeed * Time.deltaTime
        );

        // 4. 到达路径点后切换下一个
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

        // 到达终点的核心逻辑（单独抽离，确保只执行一次）
    private void OnReachEnd()
    {
        if (_hasReachedEnd || _isDead) return;
        _hasReachedEnd = true; 
        Debug.Log($"{gameObject.name} 到达终点，扣血{damageToPlayer}");
        
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(damageToPlayer);
        }
        
        DestroyEnemy();
    }

    // 受伤逻辑
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name} 受到 {damage} 伤害，剩余血量：{_currentHealth}");

        MonsterHpBar hp = this.GetComponent<MonsterHpBar>();
        hp.TakeDamage(damage);
        hp.maxHp = maxHealth;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // 死亡逻辑
    protected virtual void Die()
    {
        _isDead = true;
        Debug.Log($"{gameObject.name} 死亡");


        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(10);
            GameManager.Instance.killNum++;
        }

        DestroyEnemy();
    }

    // 统一的敌人销毁逻辑
    private void DestroyEnemy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RemoveEnemy(this);
        }
        Destroy(gameObject, 1.1f);
    }

    // 防止敌人被意外销毁时未移除
    private void OnDestroy()
    {

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RemoveEnemy(this);
        }
    }
}