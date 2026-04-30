
using UnityEngine;
using System;

public class Buff : MonoBehaviour
{

    public enum BuffType
    {
        None,
        AttackSpeed,    
        Damage,        
        MoveSpeed,      
        MaxHealth,      
        Heal  
    }

    [Header("Buff config")]
    public BuffType buffType;       
    public float value;            
    public float duration = -1f;   
    [HideInInspector] public float remainingTime; 


    public void Initialize(float dur)
    {
        duration = dur;
        remainingTime = duration;
    }


    public bool Tick(float deltaTime)
    {
        if (duration < 0) return true; 
        remainingTime -= deltaTime;
        return remainingTime > 0;
    }
}