using UnityEngine;


public class Waypoint : MonoBehaviour
{
    /// Defines a waypoint node for enemy movement paths in the Forest Defense game

    public Waypoint nextWaypoint;
    public bool isLastWaypoint; 


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (nextWaypoint != null)
        {
            // Set gizmo color to yellow for high visibility
            //// Draw a line from current waypoint to next waypoint 
            Gizmos.DrawLine(transform.position, nextWaypoint.transform.position);
        }
    }
}