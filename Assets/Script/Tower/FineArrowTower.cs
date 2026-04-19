using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FineArrowTower : BaseTower
{

    [Header("CONFIG")]
    public GameObject arrowPrefab;
    public float arrowSpeed = 0.5f; 


    

    public Vector3 bulletPosOffset = Vector3.zero;
    public Vector3 bulletRotOffset = new Vector3(0, 0, 0);
    public bool showDebugGizmos = true;

    // public float enemyCenterYOffset = -1.0f;

    protected override void Shoot()
    {

        if (_targetEnemy == null || arrowPrefab == null || _turretFirePoints == null) 
        {
            Debug.LogWarning("弓箭塔射击条件不足：目标/预制体/射击点管理器为空");
            return;
        }

        List<Transform> firePoints = _turretFirePoints.GetAllFirePoints();
        if (firePoints.Count == 0)
        {
            Debug.LogWarning("弓箭塔没有可用的射击点！");
            return;
        }



        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null) continue;


            Vector3 spawnPos = firePoint.TransformPoint(bulletPosOffset);
            Vector3 dirToEnemy = _targetEnemy.position - spawnPos;
            Quaternion targetRot = Quaternion.LookRotation(dirToEnemy);
            targetRot *= Quaternion.Euler(bulletRotOffset);

            GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, targetRot);
            arrowObj.transform.SetParent(null); 


            // ParticleMoverBullet bulletMover = arrowObj.GetComponent<ParticleMoverBullet>();
            ParticleMoverBullet bulletMover = arrowObj.GetComponentInChildren<ParticleMoverBullet>();
            if (bulletMover != null)
            {
                bulletMover.speed = this.arrowSpeed * 30; 
                bulletMover.fatherTower = this;

  
            }

            if(bulletBgm != null)
            {
                 AudioManager.Instance.PlayBattleSFX(bulletBgm);
            }
        }
    }




}

