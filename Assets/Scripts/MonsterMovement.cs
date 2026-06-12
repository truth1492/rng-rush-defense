using UnityEngine;

public class MonsterMovement : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float yOffset = 0f;  // adjust feet position on path
    public float xOffset = 0f;  // adjust horizontal position on path

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private int endWaypointIndex = -1;

    void Start()
    {
        GameObject pathObject = GameObject.Find("EnemyPath");
        if (pathObject != null && pathObject.transform.childCount > 0)
        {
            int count = pathObject.transform.childCount;
            waypoints = new Transform[count];
            for (int i = 0; i < count; i++)
                waypoints[i] = pathObject.transform.GetChild(i);

            endWaypointIndex = count - 1;
        }

        if (waypoints != null && waypoints.Length > 0)
        {
            currentWaypointIndex = 0;
            transform.position = new Vector3(
                waypoints[0].position.x + xOffset,
                waypoints[0].position.y + yOffset,
                0f);
        }
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypointIndex];
        Vector3 flatTarget = new Vector3(target.position.x + xOffset, target.position.y + yOffset, 0f);
        Vector3 flatPos = new Vector3(transform.position.x, transform.position.y, 0f);

        transform.position = Vector3.MoveTowards(flatPos, flatTarget, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, flatTarget) < 0.05f)
        {
            if (currentWaypointIndex == endWaypointIndex)
            {
                if (WaveManager.Instance != null)
                    WaveManager.Instance.EnemyReachedEnd(gameObject);

                Destroy(gameObject);
                return;
            }

            currentWaypointIndex++;
        }
    }
}