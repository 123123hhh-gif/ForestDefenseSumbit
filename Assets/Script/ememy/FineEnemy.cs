using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FineEnemy : BaseEnemy
{
    private Animator _enemyAnimator;

  
    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isDeadHash = Animator.StringToHash("IsDead");

    private readonly int _isAttackHash = Animator.StringToHash("IsAttack");

    private bool _isCurrentlyWalking = false;
     private bool _isAttacking = false;

    private void Awake()
    {
        _enemyAnimator = GetComponent<Animator>();

        if (_enemyAnimator == null)
        {
            Debug.LogError("_enemyAnimator is  null", this);
        }
    }

    // private void Update()
    // {
        
        // if (Input.GetMouseButtonDown(0))
        // {
        //     _isCurrentlyWalking = !_isCurrentlyWalking;
        //     SetWalkingState(_isCurrentlyWalking);
        // }

      
        // if (Input.GetMouseButtonDown(1))
        // {
        //     TriggerDeath();
        // }

        
        // if (Input.GetKeyDown(KeyCode.R))
        // {
        //     ResetAnimationState();
        //     _isCurrentlyWalking = false;

        // }
    // }

    private bool isTure = false;
    protected override void MoveToWaypoint()
    {
        if (_isAttacking) return;

        base.MoveToWaypoint();
        SetWalkingState(true);
        _isCurrentlyWalking = true;
    }

    protected override void Die()
    {
        base.Die();
        TriggerDeath();
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