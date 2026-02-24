using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FineArrowTower : BaseTower
{

    [Header("CONFIG")]
    public GameObject arrowPrefab;


    

    public bool showDebugGizmos = true;

 
    protected override void Shoot()
    {
        if (_targetEnemy == null || arrowPrefab == null || _turretFirePoints == null)
        {
            Debug.LogWarning("Arrow turret shooting conditions insufficient: Target/Prefab/Fire Point Manager is null");
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("No available fire points for arrow turret!");
            return;
        }



        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null) continue;


            Vector3 spawnPos = firePoint.TransformPoint(CurrentData.bulletPosOffset);
            Vector3 dirToEnemy = _targetEnemy.position - spawnPos;
            Quaternion targetRot = Quaternion.LookRotation(dirToEnemy);
            targetRot *= Quaternion.Euler(CurrentData.bulletRotOffset);

            GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, targetRot);
            arrowObj.transform.SetParent(null);



            ParticleMoverBullet bulletMover = arrowObj.GetComponentInChildren<ParticleMoverBullet>();
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




}

