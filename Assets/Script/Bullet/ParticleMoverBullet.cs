using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleMoverBullet : MonoBehaviour
{
    public float hitOffset = 0f;
    public bool UseFirePointRotation;
    public GameObject hit;
    public GameObject flash;


    [HideInInspector]
    public int damage = 0;


    public float maxFlyDistance = 50f;
    public float maxLifeTime = 5f;


    public Buff.BuffType buffType;  
    public float buffValue;   
    public float buffDuration;
    private float rotateSpeed = 720f;
    private Transform _targetEnemy;

    [HideInInspector]
    public BaseTower fatherTower;   
    private Vector3 _startPos;
    private float speed = 0f;




    void Start()
    {
        _startPos = transform.position;
        StartCoroutine(TimeoutDestroy());

       
        if (fatherTower != null && fatherTower.CurrentData != null)
        {
            speed = fatherTower.CurrentData.bulletSpeed;
        }
        else
        {
            speed = 15f;
        }


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

                Vector3 dirToTarget = (_targetEnemy.position - transform.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(dirToTarget);

                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
                transform.position += transform.forward * (speed * Time.deltaTime);
            }
            else
            {

                transform.position += transform.forward * (speed * Time.deltaTime);
            }
        }


        float currentDistance = Vector3.Distance(transform.position, _startPos);
        if (currentDistance >= maxFlyDistance)
        {
            DestroyBullet();
            return;
        }
    }

    void OnCollisionEnter(Collision collision)
    {

        if (collision.gameObject.CompareTag("Enemy") == false)
            return;

        speed = 0;
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point + contact.normal * hitOffset;


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


        BaseEnemy enemy = collision.collider.GetComponentInParent<BaseEnemy>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.ApplyBuff(buffType, buffValue, buffDuration); 

            enemy.TakeDamage(damage, fatherTower); 
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