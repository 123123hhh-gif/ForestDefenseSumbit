using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int maxHealth = 100; // Maximum health of the enemy
    public float moveSpeed = 2f; // Movement speed along the path
    public int damageToPlayer = 1; // Damage dealt when reaching the end (reserved for later integration)

    private int _currentHealth; // Current health value
    private bool _isDead = false; // Whether the enemy is dead
    private bool _hasReachedEnd = false; // Whether the enemy has reached the end waypoint
    private Waypoint _currentWaypoint; // Current target waypoint

    public bool IsDead => _isDead; // Public read-only flag for death state

    private void Start()
    {
        _currentHealth = maxHealth; // Initialize health on spawn
        // NOTE: Registration to EnemyManager is intentionally removed for early-stage compilation stability.
    }

    private void Update()
    {
        if (_isDead || _hasReachedEnd || _currentWaypoint == null) return; // Skip if inactive or no path

        ApplyDebuff(); // Hook for derived classes (optional)
        MoveToWaypoint(); // Core movement logic
    }

    public void SetStartWaypoint(Waypoint startWaypoint)
    {
        _currentWaypoint = startWaypoint; // Assign starting waypoint
        _hasReachedEnd = false; // Reset end flag for reuse
    }

    protected virtual void ApplyDebuff()
    {
        // Intentionally empty: override in derived enemies if needed.
    }

    protected virtual void MoveToWaypoint()
    {
        if (_currentWaypoint == null) return; // Safety check

        Vector3 targetPos = _currentWaypoint.transform.position; // Waypoint world position
        Vector3 direction = new Vector3(targetPos.x - transform.position.x, 0f, targetPos.z - transform.position.z); // Lock to XZ plane

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized); // Compute facing direction
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f); // Keep upright rotation
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime); // Move toward waypoint

        if (Vector3.Distance(transform.position, targetPos) < 0.1f) // Arrived at waypoint
        {
            if (_currentWaypoint.isLastWaypoint)
            {
                OnReachEnd(); // Handle reaching the end of the path
                return;
            }

            _currentWaypoint = _currentWaypoint.nextWaypoint; // Advance to next waypoint
        }
    }

    private void OnReachEnd()
    {
        if (_hasReachedEnd || _isDead) return; // Ensure end logic runs only once
        _hasReachedEnd = true; // Mark reached end

        Debug.Log($"{gameObject.name} reached the end. Damage reserved: {damageToPlayer}"); // Placeholder log

        // NOTE: GameManager damage call is intentionally removed for early-stage compilation stability.
        Destroy(gameObject); // Remove enemy at the end for now
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return; // Ignore damage after death

        _currentHealth -= damage; // Apply damage
        Debug.Log($"{gameObject.name} took {damage} damage. HP left: {_currentHealth}"); // Debug log in English

        // NOTE: MonsterHpBar update is intentionally removed for early-stage compilation stability.

        if (_currentHealth <= 0)
        {
            Die(); // Trigger death when HP is depleted
        }
    }

    protected virtual void Die()
    {
        _isDead = true; // Mark dead state
        Debug.Log($"{gameObject.name} died."); // Debug log in English

        // NOTE: Reward / kill count logic is intentionally removed for early-stage compilation stability.
        Destroy(gameObject, 0.2f); // Slight delay to allow effects later if needed
    }
}
