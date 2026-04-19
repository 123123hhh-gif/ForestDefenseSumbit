using UnityEngine;
using System.Collections.Generic;


public class TurretFirePoints : MonoBehaviour
{
    [Header("射击点配置")]
    public List<Transform> firePoints = new List<Transform>(); 
    public bool autoCollectChildFirePoints = true; 

    
    private const string FIRE_POINT_TAG = "FirePoint";

    private void Awake()
    {
       
        if (autoCollectChildFirePoints)
        {
            CollectFirePointsByTag();
        }
    }


    private void CollectFirePointsByTag()
    {
        firePoints.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>();
        
        foreach (Transform child in allChildren)
        {
            if (child.CompareTag(FIRE_POINT_TAG))
            {
                firePoints.Add(child);
                Debug.Log($"炮塔{gameObject.name}自动收集到射击点：{child.name}");
            }
        }
    }


    public List<Transform> GetAllFirePoints()
    {
       
        firePoints.RemoveAll(fp => fp == null);
        return firePoints;
    }

   
    private void OnDrawGizmosSelected()
    {
        if (firePoints == null) return;
        
        Gizmos.color = Color.red;
        foreach (Transform fp in firePoints)
        {
            if (fp != null)
            {
                Gizmos.DrawSphere(fp.position, 0.1f); 
                Gizmos.DrawRay(fp.position, fp.forward * 0.5f); 
            }
        }
    }
}