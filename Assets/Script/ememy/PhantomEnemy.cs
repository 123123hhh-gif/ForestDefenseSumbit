using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhantomEnemy : BaseEnemy
{
 private Animator _enemyAnimator;

  
    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isDeadHash = Animator.StringToHash("IsDead");

    private bool _isCurrentlyWalking = false;

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
                isTure = !isTure;
        }
        if(isTure)
        {
            return;
        }
        base.MoveToWaypoint();
        SetWalkingState(true);
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

    }

    public void TriggerDeath()
    {
        if (_enemyAnimator == null) return;


        _enemyAnimator.SetTrigger(_isDeadHash);
        _enemyAnimator.SetBool(_isWalkingHash, false);
        _isCurrentlyWalking = false;

    }

    public void ResetAnimationState()
    {
        if (_enemyAnimator == null) return;

        _enemyAnimator.SetBool(_isWalkingHash, false);

        _enemyAnimator.ResetTrigger(_isDeadHash);
    }
}
