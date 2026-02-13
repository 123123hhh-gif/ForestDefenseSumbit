using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class BaseTower : MonoBehaviour, IPointerClickHandler
{
    [Header("Core References")]
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

    [HideInInspector]
    public TowerPlace towerPlace;

    protected virtual void Start()
    {
        _turretFirePoints = turretRoot.gameObject.GetComponent<TurretFirePoints>();
        Debug.Log("_turretFirePoints = " + _turretFirePoints);
        _attackTimer = 0;
    }

    public void init(TowerData data)
    {
        _currentData = data;
    }

    // Implement IPointerClickHandler interface to handle click events
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log($"Tower clicked: {eventData.pointerId}");
            // Left click: Open upgrade/detail panel
            UIManager.Instance.ShowUpgradePanel(this);
        }
    }

    protected virtual void Update()
    {
        // Validate if current target is still valid
        ValidateTarget();

        // If no valid target, find a new one
        if (!HasTarget())
        {
            FindTarget();
            return;
        }

        // Rotate turret towards target and attack
        RotateTurretToTarget();
        AttackTarget();
    }

    protected virtual bool HasTarget()
    {
        if (_targetEnemy == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    // Check if current target is still valid (alive, in range, exists)
    protected virtual void ValidateTarget()
    {
        if (_targetEnemy == null) return;

        BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();

        if (enemy == null || enemy.IsDead || !IsTargetInRange())
        {
            _targetEnemy = null;
        }
    }

    // Find the closest valid enemy in attack range
    protected virtual void FindTarget()
    {
        if (EnemyManager.Instance == null)
        {
            Debug.LogWarning("EnemyManager not found!");
            return;
        }

        // Get all enemies within attack range
        List<BaseEnemy> enemiesInRange = EnemyManager.Instance.GetEnemiesInRange(
            transform.position,
            _currentData.attackRange
        );

        if (enemiesInRange.Count == 0)
        {
            _targetEnemy = null;
            return;
        }

        // Find the closest enemy from the list
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

    // Smoothly rotate turret to face target
    protected virtual void RotateTurretToTarget()
    {
        if (_targetEnemy == null || turretRoot == null) return;

        Vector3 direction = _targetEnemy.position - turretRoot.position;
        // Prevent rotation jitter when target is too close
        if (direction.magnitude < 0.1f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        turretRoot.rotation = Quaternion.Lerp(
            turretRoot.rotation,
            targetRotation,
            Time.deltaTime * _currentData.rotateSpeed
        );
    }

    // Check if turret is facing the target within a certain angle threshold
    protected virtual bool IsTurretFacingTarget()
    {
        if (_targetEnemy == null || turretRoot == null) return false;

        // Get current forward direction of turret and normalized direction to target
        Vector3 currentForward = turretRoot.forward;
        Vector3 targetDirection = (_targetEnemy.position - turretRoot.position).normalized;

        // Calculate angle between turret forward and target direction
        float angle = Vector3.Angle(currentForward, targetDirection);
        return angle < 25f; // Attack only when angle is within 25 degrees
    }

    // Handle attack logic (cooldown and shooting)
    private void AttackTarget()
    {
        if (_targetEnemy == null) return;

        // Only attack when turret is facing the target
        if (!IsTurretFacingTarget())
        {
            return;
        }

        _attackTimer += Time.deltaTime;
        // Debug.Log("AttackTarget _attackTimer = " + _attackTimer + " _currentData.attackRate = " + _currentData.attackRate);
        if (_attackTimer < _currentData.attackRate) return;

        _attackTimer = 0;
        Shoot();
    }

    // Perform shooting action (virtual for override in child classes)
    protected virtual void Shoot()
    {
        // Check if fire point manager is initialized
        if (_turretFirePoints == null)
        {
            Debug.LogWarning("Fire point manager not initialized, cannot shoot!");
            return;
        }

        // Get all fire points from the manager
        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning($"Turret {turretRoot.name} has no available fire points!");
            return;
        }

        // Shoot from each fire point
        foreach (Transform firePoint in firePoints)
        {
            if (firePoint != null && _targetEnemy != null)
            {
                Debug.Log($"Shooting at {_targetEnemy.name} from {firePoint.name}, damage: {_currentData.damage}");
                // Deal damage to enemy
                BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();
                if (enemy != null) enemy.TakeDamage(_currentData.damage);
            }
        }
    }

    // Upgrade tower to next level (return success status)
    public bool Upgrade()
    {
        // Check if tower is already at max level
        if (_currentData.nextLevelData == null)
        {
            Debug.Log($"{_currentData.towerName} has reached max level");
            return false;
        }

        // Check if player has enough gold for upgrade
        if (!GameManager.Instance.CheckEnoughGold(_currentData.nextLevelData.cost))
        {
            Debug.Log("Not enough gold to upgrade");
            return false;
        }

        Debug.Log($"{gameObject.name} upgraded to {_currentData.towerName}");
        return true;
    }

    // Check if target is within attack range
    private bool IsTargetInRange()
    {
        if (_targetEnemy == null) return false;
        return Vector3.Distance(transform.position, _targetEnemy.position) <= _currentData.attackRange;
    }

    // Destroy tower (with optional sell refund)
    public void DestroyTower(bool isSell = false)
    {
        // Clean up tower references and coroutines
        CleanupTower();

        // Refund gold if selling
        if (isSell && GameManager.Instance != null)
        {
            int refundGold = Mathf.RoundToInt(_currentData.cost * 0.7f);
            GameManager.Instance.AddGold(refundGold);
            Debug.Log($"Sold tower {gameObject.name}, refunded gold: {refundGold}");
        }

        Destroy(gameObject);
    }

    // Clean up tower resources and references
    private void CleanupTower()
    {
        StopAllCoroutines();
        _targetEnemy = null;
        _turretFirePoints = null;
    }

    // Clean up on destroy
    private void OnDestroy()
    {
        _targetEnemy = null;
        _currentData = null;
        towerPlace = null;
        Debug.Log($"Tower {gameObject.name} destroyed, OnDestroy triggered");
    }
}