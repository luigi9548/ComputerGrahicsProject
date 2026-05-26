using UnityEngine;

public class WalkPath : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 1.5f;
    private int currentWaypoint = 0;

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime);

        transform.LookAt(new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z));

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }
}