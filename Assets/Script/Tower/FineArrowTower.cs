using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FineArrowTower : BaseTower
{

    [Header("CONFIG")]
    public GameObject arrowPrefab;


    

    // public Vector3 bulletPosOffset = Vector3.zero;
    // public Vector3 bulletRotOffset = new Vector3(0, 0, 0);
    public bool showDebugGizmos = true;

    // public float enemyCenterYOffset = -1.0f;

    protected override void Shoot()
    {

        if (_targetEnemy == null || arrowPrefab == null || _turretFirePoints == null) 
        {
            Debug.LogWarning("Insufficient shooting conditions for the bow and arrow tower: The target/prefabricated body/shooting point manager is empty");
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("The bow and arrow tower has no available shooting points!");
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


            // ParticleMoverBullet bulletMover = arrowObj.GetComponent<ParticleMoverBullet>();
            ParticleMoverBullet bulletMover = arrowObj.GetComponentInChildren<ParticleMoverBullet>();
            if (bulletMover != null)
            {


                bulletMover.damage = (int)CurrentDamage;

                bulletMover.fatherTower = this;
                bulletMover.SetTarget(_targetEnemy);

  
            }

            if(bulletBgm != null)
            {
                 AudioManager.Instance.PlayBattleSFX(bulletBgm);
            }
        }
    }




}

