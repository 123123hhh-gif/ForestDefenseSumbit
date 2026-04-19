using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CannonTower : BaseTower
{

    [Header("cannonConfig")]
    public GameObject bulletPrefab; 
    public float speed = 0.5f; 




    protected override void Shoot()
    {

        if (_targetEnemy == null || bulletPrefab == null || _turretFirePoints == null) 
        {
            Debug.LogWarning("射击条件不足：目标 = "+_targetEnemy+"  预制体 = "+bulletPrefab+"  射击点管理器 ="+_turretFirePoints);
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("没有可用的射击点！");
            return;
        }

        foreach (Transform firePoint in firePoints)
        {
                if (firePoint == null) continue;

               
                GameObject obj = Instantiate(bulletPrefab);
                obj.transform.SetParent(null);
                obj.transform.position = firePoint.position; 
                obj.transform.LookAt(_targetEnemy); 

               
                Cannon arrow = obj.GetComponent<Cannon>();
                if (arrow == null) arrow = obj.AddComponent<Cannon>();
                arrow.SetTarget(_targetEnemy, speed, _currentData.damage);
        }

            if(bulletBgm != null)
            {
                 AudioManager.Instance.PlayBattleSFX(bulletBgm);
            }
    }
}




public class Cannon : MonoBehaviour
{
    private Transform _target;
    private float _speed;
    private int _damage;

    private float _lifeTime = 5f;
    private float _lifeTimer = 0f;

    public void SetTarget(Transform target, float speed, int damage)
    {
         Debug.Log("Shoot 4 ");
        _target = target;
        _speed = speed;
        _damage = damage;
        _lifeTimer = 0;
    }

    private void Update()
        {
            
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= _lifeTime || _target == null)
            {
                Destroy(gameObject);
                return;
            }

           
            if (_target == null)
            {
                Destroy(gameObject);
                return;
            }

            
            transform.position = Vector3.MoveTowards(
                transform.position, 
                _target.position, 
                _speed * Time.deltaTime
            );

           
            if (Vector3.Distance(transform.position, _target.position) < 0.1f)
            {
                BaseEnemy enemy = _target.GetComponent<BaseEnemy>();
                if (enemy != null && !enemy.IsDead) enemy.TakeDamage(_damage);
                Destroy(gameObject);
            }
        }
}