using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    public GameObject coinPrefab;
    public float spawnInterval = 2f;
    public float[] lanePositions;
    public float minGapFromObstacle = 6f;
    public float coinSpacing = 2f;
    public float jumpObstacleExtraGap = 10f;

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

        if (timer >= spawnInterval)
        {
            SpawnCoin();
            timer = 0f;
        }
    }

    void SpawnCoin()
    {
        int randomLane = Random.Range(0, lanePositions.Length);
        int coinCount = Random.Range(3, 7);

        float startZ = transform.position.z + 30f;
        float endZ = startZ + (coinCount * coinSpacing);

        if (!IsCoinChainPositionSafe(lanePositions[randomLane], startZ, endZ))
            return;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                lanePositions[randomLane],
                1f,
                startZ + (i * coinSpacing)
            );

            Instantiate(coinPrefab, spawnPos, Quaternion.identity);
        }
    }

    bool IsCoinChainPositionSafe(float laneX, float chainStartZ, float chainEndZ)
    {
        ObstacleType[] obstacles = FindObjectsByType<ObstacleType>();

        foreach (ObstacleType obstacle in obstacles)
        {
            if (Mathf.Abs(obstacle.transform.position.x - laneX) > 0.1f)
                continue;

            float obstacleZ = obstacle.transform.position.z;
            float currentGap = minGapFromObstacle;

            if (obstacle.obstacleKind == ObstacleType.ObstacleKind.JumpOver)
            {
                currentGap += jumpObstacleExtraGap;
            }

            bool tooClose =
                obstacleZ >= chainStartZ - currentGap &&
                obstacleZ <= chainEndZ + currentGap;

            if (tooClose)
                return false;
        }

        return true;
    }
}