using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Attributes")]
    public int maxHealth = 100;


    public float moveSpeed = 2f;

    public int damageToPlayer = 1;

    private int _currentHealth;
    private bool _isDead = false;
    private Waypoint _currentWaypoint; // Current target waypoint
    private bool _hasReachedEnd = false;

    // Public properties
    public bool IsDead => _isDead;



    private void Start()
    {
        _currentHealth = maxHealth;
        // Automatically register to the manager when spawned
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

    // Set the starting waypoint
    public void SetStartWaypoint(Waypoint startWaypoint)
    {
        _currentWaypoint = startWaypoint;
        _hasReachedEnd = false;
    }

    protected virtual void addDebuff()
    {

    }

    // Move to the target waypoint
    protected virtual void MoveToWaypoint()
    {
        if (_currentWaypoint == null) return;

        // 1. Calculate direction (Lock Y-axis, only move on XZ plane)
        Vector3 targetPos = _currentWaypoint.transform.position;
        Vector3 direction = new Vector3(targetPos.x - transform.position.x, 0, targetPos.z - transform.position.z);
        direction.Normalize();

        // 2. Rotate only horizontally to avoid up/down rotation
        if (direction.magnitude > 0.1f)
        {
            // Calculate target rotation, force Y-axis to current value, only modify X and Z axes
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

            // Instant rotation with no easing
            transform.rotation = targetRotation;
        }

        // 3. Use MoveTowards instead of Translate for more precise movement
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // 4. Switch to the next waypoint after reaching the current one
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

    // Core logic for reaching the end (extracted separately to ensure it is executed only once)
    private void OnReachEnd()
    {

        if (_hasReachedEnd || _isDead) return;
        _hasReachedEnd = true;
        Debug.Log($"{gameObject.name} reached the end, deduct {damageToPlayer} HP from player");
        // Add this log: Check if GameManager is null
        Debug.Log("GameManager Instance: " + (GameManager.Instance == null ? "Null" : "Exist"));


        if (GameManager.Instance != null)
        {
            GameManager.Instance.TakeDamage(damageToPlayer);
        }

        DestroyEnemy();
    }

    // Take damage logic
    public void TakeDamage(int damage)
    {
        Debug.Log("TakeDamage called!");
        if (_isDead) return;

        _currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage, remaining HP: {_currentHealth}");

        MonsterHpBar hp = this.GetComponent<MonsterHpBar>();
        hp.TakeDamage(damage);
        hp.maxHp = maxHealth;

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    // Death logic
    protected virtual void Die()
    {
        _isDead = true;
        Debug.Log($"{gameObject.name} died");


        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(10);
            GameManager.Instance.killNum++;
        }

        DestroyEnemy();
    }

    // Unified enemy destruction logic
    private void DestroyEnemy()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RemoveEnemy(this);
        }
        Destroy(gameObject, 1.1f);
    }

    // Prevent the enemy from not being removed when accidentally destroyed
    private void OnDestroy()
    {

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RemoveEnemy(this);
        }
    }
}