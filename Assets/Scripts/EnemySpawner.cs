using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Monster Blueprint")]
    public GameObject enemyPrefab;

    [Header("Wave Settings")]
    public float timeBetweenSpawns = 2f;
    private float spawnTimer;

    // We need to look up where the path starts
    private Transform spawnPoint;

    void Start()
    {
        // Find the EnemyPath folder, and grab its very first child (WayPoint_1)
        GameObject pathObject = GameObject.Find("EnemyPath");
        if (pathObject != null && pathObject.transform.childCount > 0)
        {
            spawnPoint = pathObject.transform.GetChild(0);
        }
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= timeBetweenSpawns)
        {
            SpawnMonster();
            spawnTimer = 0f;
        }
    }

    void SpawnMonster()
    {
        // Block spawning if assets aren't linked up cleanly
        if (enemyPrefab == null || spawnPoint == null) return;

        // FIXED: Spawn the monster directly at WayPoint_1's coordinates instead of Vector3.zero
        Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
    }
}