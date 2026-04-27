// Buff.cs
// 2025-03-16: 新增Buff组件，可挂载至塔或敌人，定义属性修正
using UnityEngine;
using System;

public class Buff : MonoBehaviour
{
    // 修正类型枚举
    public enum BuffType
    {
        None,
        AttackSpeed,    // 攻击速度倍率 (乘算)
        Damage,         // 伤害倍率 (乘算)
        MoveSpeed,      // 移动速度倍率 (乘算)
        MaxHealth,       // 最大生命值增量 (固定值)
        Heal  
    }

    [Header("Buff config")]
    public BuffType buffType;       // 效果类型
    public float value;            // 数值：倍率(1.2=+20%) 或 固定值(如+50生命)
    public float duration = -1f;   // 持续时间, -1 表示永久
    [HideInInspector] public float remainingTime; // 剩余时间

    /// <summary>
    /// 初始化Buff（通常由添加者调用）
    /// </summary>
    public void Initialize(float dur)
    {
        duration = dur;
        remainingTime = duration;
    }

    /// <summary>
    /// 每帧减少持续时间，返回 true 表示Buff仍然有效
    /// </summary>
    public bool Tick(float deltaTime)
    {
        if (duration < 0) return true; // 永久Buff
        remainingTime -= deltaTime;
        return remainingTime > 0;
    }
}