using UnityEngine;
using System.Collections.Generic;

public class TurretFirePoints : MonoBehaviour
{
    [Header("Fire Point Settings")]
    public List<Transform> firePoints = new List<Transform>(); // List of all fire points on the turret
    public bool autoCollectChildFirePoints = true; // Automatically collect child objects tagged as FirePoint on awake

    // Tag used to identify fire point objects
    private const string FIRE_POINT_TAG = "FirePoint";

    private void Awake()
    {
        // Auto collect fire points if enabled
        if (autoCollectChildFirePoints)
        {
            CollectFirePointsByTag();
        }
    }

    // Collect all child objects with the FirePoint tag
    private void CollectFirePointsByTag()
    {
        firePoints.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.CompareTag(FIRE_POINT_TAG))
            {
                firePoints.Add(child);
                Debug.Log($"Turret {gameObject.name} auto-collected fire point: {child.name}");
            }
        }
    }

    // Get all valid fire points (remove null references first)
    public List<Transform> GetAllFirePoints()
    {
        // Clean up null fire point references
        firePoints.RemoveAll(fp => fp == null);
        return firePoints;
    }

    // Draw gizmos for fire points in Scene view (only when object is selected)
    private void OnDrawGizmosSelected()
    {
        if (firePoints == null) return;

        Gizmos.color = Color.red;
        foreach (Transform fp in firePoints)
        {
            if (fp != null)
            {
                Gizmos.DrawSphere(fp.position, 0.1f); // Draw a small sphere at fire point position
                Gizmos.DrawRay(fp.position, fp.forward * 0.5f); // Draw a ray to show fire direction
            }
        }
    }
}