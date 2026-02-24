using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FineMagicTower : BaseTower
{

    [Header("CONFIG")]
    public GameObject bulletPrefab;

    public bool showDebugGizmos = true;

    protected override void RotateTurretToTarget()
    {
        // base.RotateTurretToTarget();
    }

    protected override bool IsTurretFacingTarget()
    {
        return true;
    }
    protected override void Shoot()
    {
        if (_targetEnemy == null || bulletPrefab == null || _turretFirePoints == null)
        {
            Debug.LogWarning("Tower shooting conditions insufficient: Target/Prefab/Fire Point Manager is null");
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("No available fire points for tower!");
            return;
        }

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null) continue;

            Vector3 spawnPos = firePoint.TransformPoint(CurrentData.bulletPosOffset);
            Vector3 dirToEnemy = _targetEnemy.position - spawnPos;
            Quaternion targetRot = Quaternion.LookRotation(dirToEnemy);
            targetRot *= Quaternion.Euler(CurrentData.bulletRotOffset);

            GameObject obj = Instantiate(bulletPrefab, spawnPos, targetRot);
            obj.transform.SetParent(null);


            ParticleMoverBullet bulletMover = obj.GetComponentInChildren<ParticleMoverBullet>();
            if (bulletMover != null)
            {
                bulletMover.fatherTower = this;
                bulletMover.SetTarget(_targetEnemy);

            }

            if (bulletBgm != null)
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

            // Draw original fire point (white sphere)
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(firePoint.position, 0.1f);

            // Draw offset spawn position (red sphere)
            Vector3 spawnPos = firePoint.TransformPoint(CurrentData.bulletPosOffset);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(spawnPos, 0.1f);

            // Draw bullet direction (red line pointing to enemy)
            Vector3 dirToEnemy = _targetEnemy.position - spawnPos;
            Quaternion targetRot = Quaternion.LookRotation(dirToEnemy) * Quaternion.Euler(CurrentData.bulletRotOffset);
            Gizmos.DrawLine(spawnPos, spawnPos + targetRot * Vector3.forward * 2f);
        }
    }
}
