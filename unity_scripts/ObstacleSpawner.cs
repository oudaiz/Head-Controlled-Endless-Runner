using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public float startSpawnInterval = 3f;
    public float minSpawnInterval = 0.8f;
    public float spawnRateIncrease = 0.2f;
    public float[] lanePositions;

    private float timer;
    private PlayerMovement player;

    void Start()
    {
        if (lanePositions == null || lanePositions.Length == 0)
        {
            lanePositions = new float[] { -3f, 0f, 3f };
        }

        player = FindAnyObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if (player == null) return;
        if (!player.CanStartRunning) return;

        timer += Time.deltaTime;

        float currentSpawnInterval = Mathf.Max(
            minSpawnInterval,
            startSpawnInterval - Time.timeSinceLevelLoad * spawnRateIncrease
        );

        if (timer >= currentSpawnInterval)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }

    void SpawnObstacle()
    {
        int randomLane = Random.Range(0, lanePositions.Length);
        int randomObstacle = Random.Range(0, obstaclePrefabs.Length);

        GameObject selectedPrefab = obstaclePrefabs[randomObstacle];
        ObstacleType obstacleType = selectedPrefab.GetComponent<ObstacleType>();

        float yPos = 1f;
        Vector3 offset = Vector3.zero;

        if (obstacleType != null)
        {
            yPos = obstacleType.spawnY;
            offset = obstacleType.spawnOffset;
        }

        Vector3 spawnPos = new Vector3(
            lanePositions[randomLane],
            yPos,
            transform.position.z + 30f
        ) + offset;

        Instantiate(selectedPrefab, spawnPos, selectedPrefab.transform.rotation);
    }
}