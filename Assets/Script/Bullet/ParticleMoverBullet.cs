using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMoverBullet : MonoBehaviour
{
    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public GameObject hit;
    public GameObject flash;

    // ========== 2025-03-16: 新增伤害字段，由发射塔传入 ==========
    [HideInInspector]
    public int damage = 0;
    // =========================================================

    public float maxFlyDistance = 50f;
    public float maxLifeTime = 5f;


    public Buff.BuffType buffType;  
    public float buffValue;   
    public float buffDuration;
    private float rotateSpeed = 720f;
    private Transform _targetEnemy;

    [HideInInspector]
    public BaseTower fatherTower;   // 保留引用，用于可能的特殊逻辑（如反伤溯源），但不再依赖其数据计算伤害

    private Vector3 _startPos;
    private float speed = 0f;

    // 2025-03-16: 缓存初始旋转偏移（如果需要），但目前我们只使用发射时的初始旋转
    // private Vector3 _bulletRotOffset;

    void Start()
    {
        _startPos = transform.position;
        StartCoroutine(TimeoutDestroy());

        // 2025-03-16: 子弹速度从父塔获取，若父塔已销毁则使用默认值
        if (fatherTower != null && fatherTower.CurrentData != null)
        {
            speed = fatherTower.CurrentData.bulletSpeed;
        }
        else
        {
            speed = 15f; // 默认速度
        }

        // 发射特效（保持原有逻辑）
        if (flash != null)
        {
            var flashInstance = Instantiate(flash, transform.position, transform.rotation);
            ParticleSystem flashPs = flashInstance.GetComponent<ParticleSystem>();
            if (flashPs != null)
            {
                Destroy(flashInstance, flashPs.main.duration);
            }
            else
            {
                var flashPsParts = flashInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                if (flashPsParts != null)
                    Destroy(flashInstance, flashPsParts.main.duration);
                else
                    Destroy(flashInstance, 1f);
            }
        }
    }

    public void SetTarget(Transform target)
    {
        _targetEnemy = target;
    }

    void FixedUpdate()
    {
        // 2025-03-16: 重构追踪逻辑
        // 1. 如果目标无效（null/已死亡），则直线飞行直至销毁
        bool targetValid = _targetEnemy != null;
        if (targetValid)
        {
            BaseEnemy enemy = _targetEnemy.GetComponent<BaseEnemy>();
            if (enemy == null || enemy.IsDead)
                targetValid = false;
        }

        if (speed != 0)
        {
            if (targetValid)
            {
                // 追踪目标：平滑转向并移动
                Vector3 dirToTarget = (_targetEnemy.position - transform.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget);
                // 不再依赖父塔的旋转偏移，子弹发射时已包含初始偏移，追踪时不重复叠加
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
                transform.position += transform.forward * (speed * Time.deltaTime);
            }
            else
            {
                // 目标丢失：沿当前方向直线飞行
                transform.position += transform.forward * (speed * Time.deltaTime);
            }
        }

        // 超出最大飞行距离则销毁
        float currentDistance = Vector3.Distance(transform.position, _startPos);
        if (currentDistance >= maxFlyDistance)
        {
            DestroyBullet();
            return;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // 只与敌人碰撞
        if (collision.gameObject.CompareTag("Enemy") == false)
            return;

        speed = 0;
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point + contact.normal * hitOffset;

        // 生成击中特效（保留原有逻辑）
        if (hit != null)
        {
            var hitInstance = Instantiate(hit, pos, rot);
            if (UseFirePointRotation)
            {
                hitInstance.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0);
            }
            else
            {
                hitInstance.transform.rotation = rot;
            }

            ParticleSystem hitPs = hitInstance.GetComponent<ParticleSystem>();
            if (hitPs != null)
            {
                Destroy(hitInstance, hitPs.main.duration);
            }
            else
            {
                var hitPsParts = hitInstance.transform.GetChild(0).GetComponent<ParticleSystem>();
                if (hitPsParts != null)
                    Destroy(hitInstance, hitPsParts.main.duration);
                else
                    Destroy(hitInstance, 1f);
            }
        }

        // 2025-03-16: 使用传入的 damage 值，不再从父塔获取
        BaseEnemy enemy = collision.collider.GetComponentInParent<BaseEnemy>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.ApplyBuff(buffType, buffValue, buffDuration); // 先应用Buff，再造成伤害
            
            enemy.TakeDamage(damage, fatherTower); // 传入父塔作为伤害来源，用于反伤等逻辑
        }

        DestroyBullet();
    }

    private void DestroyBullet()
    {
        speed = 0;
        Destroy(gameObject);
    }

    private IEnumerator TimeoutDestroy()
    {
        yield return new WaitForSeconds(maxLifeTime);
        DestroyBullet();
    }
}