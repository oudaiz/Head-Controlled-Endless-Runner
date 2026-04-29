using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    public GameObject[] trainPrefabs;
    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    public float spawnZ = 300f;
    public float minSpawnTime = 3f;
    public float maxSpawnTime = 5f;

    public float minGapSameLane = 130f;

    private float spawnTimer;
    private PlayerMovement player;

    private GameObject lastLeftTrain;
    private GameObject lastRightTrain;

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        ResetSpawnTimer();
    }

    void Update()
    {
        if (player == null) return;
        if (!player.CanStartRunning) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            SpawnTrain();
            ResetSpawnTimer();
        }
    }

    void SpawnTrain()
    {
        if (trainPrefabs == null || trainPrefabs.Length == 0) return;
        if (leftSpawnPoint == null || rightSpawnPoint == null) return;

        bool spawnLeft = Random.value < 0.5f;

        Transform chosenPoint = spawnLeft ? leftSpawnPoint : rightSpawnPoint;
        GameObject lastTrain = spawnLeft ? lastLeftTrain : lastRightTrain;

        if (lastTrain != null)
        {
            float distanceZ = Mathf.Abs(spawnZ - lastTrain.transform.position.z);
            if (distanceZ < minGapSameLane)
                return;
        }

        int prefabIndex = Random.Range(0, trainPrefabs.Length);

        Vector3 spawnPos = chosenPoint.position;
        spawnPos.z = spawnZ;

        GameObject newTrain = Instantiate(trainPrefabs[prefabIndex], spawnPos, chosenPoint.rotation);

        if (spawnLeft)
            lastLeftTrain = newTrain;
        else
            lastRightTrain = newTrain;
    }

    void ResetSpawnTimer()
    {
        spawnTimer = Random.Range(minSpawnTime, maxSpawnTime);
    }
}
