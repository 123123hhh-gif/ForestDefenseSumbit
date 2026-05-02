using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 使用 NavMeshAgent 进行寻路的怪物，仍按路点逐点移动。
/// 需要场景中已烘焙 NavMesh。
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

    // 用于判断是否到达路点的距离阈值
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

        // 获取或添加 NavMeshAgent 组件
        _agent = GetComponent<NavMeshAgent>();
        if (_agent == null)
            _agent = gameObject.AddComponent<NavMeshAgent>();

        // 配置 agent 参数（可根据需要调整）
        _agent.speed = CurrentMoveSpeed;
        _agent.angularSpeed = 360f;
        _agent.acceleration = 8f;
        _agent.stoppingDistance = waypointReachedDistance;
        _agent.autoBraking = true;
        _agent.radius = 0.5f;
        _agent.height = 2f;
        _agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // 让 agent 自动更新旋转，我们不需要自己控制旋转
        _agent.updateRotation = true;
        _agent.updateUpAxis = false; // 如果希望保持 Y 轴向上
    }

    // 重写速度刷新，使 agent 速度与 buff 系统同步
    protected override void RecalcStats()
    {
        base.RecalcStats();
        if (_agent != null)
            _agent.speed = CurrentMoveSpeed;
    }

    // 重写移动逻辑，使用 NavMeshAgent 前往当前路点
    protected override void MoveToWaypoint()
    {
        if (_isDead || _hasReachedEnd || _currentWaypoint == null)
            return;

        // 设置目标路点
        _agent.SetDestination(_currentWaypoint.transform.position);

        // 检查是否到达当前路点
        // 需要满足：没有正在计算的路径，且剩余距离小于停止距离
        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance)
        {
            // 到达路点
            if (_currentWaypoint.isLastWaypoint)
            {
                OnReachEnd();
            }
            else
            {
                // 切换到下一个路点
                _currentWaypoint = _currentWaypoint.nextWaypoint;
                // 立即设置新目标，防止 agent 停止
                if (_currentWaypoint != null)
                    _agent.SetDestination(_currentWaypoint.transform.position);
            }
        }

         if (_isAttacking) return;
        SetWalkingState(true);
        _isCurrentlyWalking = true;
    }

    // 重写死亡处理，禁用 agent
    protected override void Die()
    {
        if (_agent != null)
            _agent.enabled = false;
        base.Die();
        TriggerDeath();
    }

    // 可选：在 OnDestroy 中清理 agent
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
        sourceTower.TakeDamage(counterAttackDamage);
        SetAttackState(false);
        SetWalkingState(true);
    }


}