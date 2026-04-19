using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FineCannonTower : BaseTower
{
    [Header("CONFIG")]
    public GameObject bulletPrefab;
    public float speed = 0.5f; 


    public Vector3 bulletPosOffset = Vector3.zero;

    public Vector3 bulletRotOffset = new Vector3(0, 0, 0);

    public bool showDebugGizmos = true;



    protected override void Shoot()
    {
        if (_targetEnemy == null || bulletPrefab == null || _turretFirePoints == null) 
        {
            Debug.LogWarning("塔射击条件不足：目标/预制体/射击点管理器为空");
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("塔没有可用的射击点！");
            return;
        }

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null) continue;


            Vector3 spawnPos = firePoint.TransformPoint(bulletPosOffset);
            Vector3 dirToEnemy = _targetEnemy.position - spawnPos;
            Quaternion targetRot = Quaternion.LookRotation(dirToEnemy);
            targetRot *= Quaternion.Euler(bulletRotOffset);

            GameObject arrowObj = Instantiate(bulletPrefab, spawnPos, targetRot);
            arrowObj.transform.SetParent(null); 
            
            // ParticleMoverBullet bulletMover = arrowObj.GetComponent<ParticleMoverBullet>();
            ParticleMoverBullet bulletMover = arrowObj.GetComponentInChildren<ParticleMoverBullet>();
            if (bulletMover != null)
            {
                bulletMover.speed = this.speed * 30; 
                bulletMover.fatherTower = this;
                // bulletMover.OnHit += OnBulletHitEnemy;
            }

            if(bulletBgm != null)
            {
                 AudioManager.Instance.PlayBattleSFX(bulletBgm);
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || _turretFirePoints == null || _targetEnemy == null) return;

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0) return;

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null) continue;

            // 绘制原始射击点（白色球）
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);

            // 绘制偏移后的发射位置（红色球）
            Vector3 spawnPos = firePoint.TransformPoint(bulletPosOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnPos, 0.1f);

            // 绘制子弹朝向（红色线，指向敌人）
            Vector3 dirToEnemy = _targetEnemy.position - spawnPos;
            Quaternion targetRot = Quaternion.LookRotation(dirToEnemy) * Quaternion.Euler(bulletRotOffset);
            Gizmos.DrawLine(spawnPos, spawnPos + targetRot * Vector3.forward * 2f);
        }
    }
}