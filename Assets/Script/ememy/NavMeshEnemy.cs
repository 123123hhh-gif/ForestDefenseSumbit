using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemies using NavMeshAgent for pathfinding move sequentially according to waypoints.
/// Pauses movement during attacks to ensure complete animation playback, and handles agent 
/// disabling when the enemy dies.
/// </summary>
public class NavMeshEnemy : BaseEnemy
{
    private NavMeshAgent _agent;
    private Animator _enemyAnimator;

    // Animation parameter hashes
    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isDeadHash = Animator.StringToHash("IsDead");
    private readonly int _isAttackHash = Animator.StringToHash("IsAttack");

    private bool _isCurrentlyWalking = false;
    private bool _isAttacking = false;

    // Distance threshold for reaching a waypoint
    [SerializeField] private float waypointReachedDistance = 0.5f;

    private void Awake()
    {
        _enemyAnimator = GetComponent<Animator>();
        if (_enemyAnimator == null)
        {
            Debug.LogError("_enemyAnimator is null", this);
        }
    }

    protected override void Start()
    {
        base.Start();

        // Get or add NavMeshAgent component
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
            _agent = gameObject.AddComponent<NavMeshAgent>();

        // Configure agent parameters
        _agent.speed = CurrentMoveSpeed;
        _agent.angularSpeed = 360f;
        _agent.acceleration = 8f;
        _agent.stoppingDistance = waypointReachedDistance;
        _agent.autoBraking = true;
        _agent.radius = 0.5f;
        _agent.height = 2f;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        _agent.updateRotation = true;
        _agent.updateUpAxis = false;
    }

    // Override speed refresh to sync agent speed with buff system
    protected override void RecalcStats()
    {
        base.RecalcStats();
        if (_agent != null && _agent.enabled)
            _agent.speed = CurrentMoveSpeed;
    }

    // Override movement logic: stop moving during attacks, and always set walking animation (unless attacking)
    protected override void MoveToWaypoint()
    {
        // Skip movement if dead, reached end, no current waypoint, or attacking
        if (_isDead || _hasReachedEnd || _currentWaypoint == null || _isAttacking)
            return;

        // Ensure agent exists and is enabled
        if (_agent == null || !_agent.enabled) return;

        // Set target waypoint
        _agent.SetDestination(_currentWaypoint.transform.position);

        // Check if current waypoint is reached
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            if (_currentWaypoint.isLastWaypoint)
            {
                OnReachEnd();
            }
            else
            {
                // Switch to next waypoint
                _currentWaypoint = _currentWaypoint.nextWaypoint;
                if (_currentWaypoint != null)
                    _agent.SetDestination(_currentWaypoint.transform.position);
            }
        }

        // Ensure walking animation is enabled (not in attack state)
        if (!_isAttacking)
        {
            SetWalkingState(true);
        }
    }

    // Override death handling
    protected override void Die()
    {
        // Disable agent first to prevent subsequent movement operations
        if (_agent != null)
            _agent.enabled = false;

        // Call base death logic (may trigger gold reward, etc.)
        base.Die();

        // Trigger death animation (agent is disabled so SetAttackState won't attempt to operate it)
        TriggerDeath();
    }

    // Clean up agent component
    protected override void OnDestroy()
    {
        if (_agent != null)
            Destroy(_agent);
        base.OnDestroy();
    }

    // Animation state control
    public void SetWalkingState(bool isWalking)
    {
        if (_enemyAnimator == null) return;
        _enemyAnimator.SetBool(_isWalkingHash, isWalking);
        _isCurrentlyWalking = isWalking;
    }

    public void SetAttackState(bool isAttack)
    {
        if (_enemyAnimator == null) return;
        _enemyAnimator.SetBool(_isAttackHash, isAttack);
        _isAttacking = isAttack;

        // Pause agent movement during attack to ensure complete animation playback
        // Note: Do not operate if agent is disabled
        if (_agent != null && _agent.enabled)
        {
            _agent.isStopped = isAttack;
        }
    }

    public void TriggerDeath()
    {
        if (_enemyAnimator == null) return;

        _enemyAnimator.SetTrigger(_isDeadHash);
        _enemyAnimator.SetBool(_isWalkingHash, false);
        SetWalkingState(false);
        SetAttackState(false);
    }

    public void ResetAnimationState()
    {
        if (_enemyAnimator == null) return;
        SetWalkingState(false);
        SetAttackState(false);
        _enemyAnimator.ResetTrigger(_isDeadHash);
    }

    // Override counter attack logic: enter attack state and pause movement
    protected override void PerformCounterAttack()
    {
        SetWalkingState(false);
        SetAttackState(true);
        // Stop agent movement during attack (already set isStopped in SetAttackState)
    }

    // Animation Event: Called when attack animation finishes playing
    public void OnAttackAnimationEnd()
    {
        // Do not resume movement if already dead
        if (_isDead) return;

        // Resume agent movement first (before ending attack state)
        SetAttackState(false);
        SetWalkingState(true);

        // Reset current waypoint to ensure agent continues moving forward
        // (since isStopped was just set to false but target may have changed)
        if (_agent != null && _agent.enabled && !_isDead && _currentWaypoint != null && !_hasReachedEnd)
        {
            _agent.SetDestination(_currentWaypoint.transform.position);
        }

        // Inflict damage (if sourceTower exists)
        if (sourceTower != null)
        {
            sourceTower.TakeDamage(counterAttackDamage);
        }
    }
}