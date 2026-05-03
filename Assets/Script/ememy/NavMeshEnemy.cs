using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Monsters using NavMeshAgent for pathfinding still move point by point according to the waypoints.
/// </summary>
public class NavMeshEnemy : BaseEnemy
{
    private NavMeshAgent _agent;

    private Animator _enemyAnimator;

  
    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isDeadHash = Animator.StringToHash("IsDead");

    private readonly int _isAttackHash = Animator.StringToHash("IsAttack");

    private bool _isCurrentlyWalking = false;
    private bool _isAttacking = false;

    // Distance threshold for determining whether a waypoint has been reached
    [SerializeField] private float waypointReachedDistance = 0.5f;


    private void Awake()
    {
        _enemyAnimator = GetComponent<Animator>();

        if (_enemyAnimator == null)
        {
            Debug.LogError("_enemyAnimator is  null", this);
        }
    }

    protected override void Start()
    {
        base.Start();


        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
            _agent = gameObject.AddComponent<NavMeshAgent>();

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

    // Overwrite the speed refresh to synchronize the agent speed with the buff system
    protected override void RecalcStats()
    {
        base.RecalcStats();
        if (_agent != null)
            _agent.speed = CurrentMoveSpeed;
    }

    // Rewriting the movement logic, using NavMeshAgent to navigate to the current waypoint
    protected override void MoveToWaypoint()
    {
        if (_isDead || _hasReachedEnd || _currentWaypoint == null)
            return;

        // Set target waypoint
        _agent.SetDestination(_currentWaypoint.transform.position);

        // Check whether the current waypoint has been reached
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
      
            if (_currentWaypoint.isLastWaypoint)
            {
                OnReachEnd();
            }
            else
            {
                // Switch to the next waypoint
                _currentWaypoint = _currentWaypoint.nextWaypoint;
                // Set a new goal immediately to prevent the agent from stopping
                if (_currentWaypoint != null)
                    _agent.SetDestination(_currentWaypoint.transform.position);
            }
        }

         if (_isAttacking) return;
        SetWalkingState(true);
        _isCurrentlyWalking = true;
    }

    // Override the death handling and disable the agent
    protected override void Die()
    {
        if (_agent != null)
            _agent.enabled = false;
        base.Die();
        TriggerDeath();
    }

    // Optional: Clean up the agent in OnDestroy
    protected override void OnDestroy()
    {
        if (_agent != null)
            Destroy(_agent);
        base.OnDestroy();
    }


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

    protected override void PerformCounterAttack()
    {
        // base.PerformCounterAttack();
        SetWalkingState(false);
        SetAttackState(true);

    }

    public void OnAttackAnimationEnd()
    {

        SetAttackState(false);
        SetWalkingState(true);

        if (sourceTower == null) return;
        sourceTower.TakeDamage(counterAttackDamage);
    }


}