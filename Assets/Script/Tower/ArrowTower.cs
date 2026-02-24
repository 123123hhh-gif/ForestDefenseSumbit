using UnityEngine;
using System.Collections.Generic;

public class ArrowTower : BaseTower
{
    [Header("config")]
    public GameObject arrowPrefab; 
    public float arrowSpeed = 0.5f; 
    protected override void Shoot()
    {

        if (_targetEnemy == null || arrowPrefab == null || _turretFirePoints == null) 
        {
            Debug.LogWarning("Insufficient shooting conditions for the bow and arrow tower: The target/ prefab/ shooting point manager is empty");
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("There are no available shooting points on the bow and arrow tower!");
            return;
        }

        foreach (Transform firePoint in firePoints)
        {
                if (firePoint == null) continue;

                
                GameObject arrowObj = Instantiate(arrowPrefab);
                arrowObj.transform.SetParent(null); 
                arrowObj.transform.position = firePoint.position; 
                arrowObj.transform.LookAt(_targetEnemy); 

                
                Arrow arrow = arrowObj.GetComponent<Arrow>();
                if (arrow == null) arrow = arrowObj.AddComponent<Arrow>();
                arrow.SetTarget(_targetEnemy,this);

            if(bulletBgm != null)
            {
                AudioManager.Instance.PlayBattleSFX(bulletBgm);
            }
        }
    }
}



public class Arrow : MonoBehaviour
{
    private Transform _target;
    private float _speed;
    private int _damage;

    private float _lifeTime = 5f;
    private float _lifeTimer = 0f;

    private BaseTower fatherTower;

    public void SetTarget(Transform target, BaseTower tower)
    {
         Debug.Log("Shoot 4 ");
        _target = target;
        fatherTower = tower;
        
        _speed = fatherTower.CurrentData.bulletSpeed;
        _damage = fatherTower.CurrentData.damage;
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