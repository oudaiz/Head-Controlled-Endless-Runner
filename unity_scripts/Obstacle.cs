using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float baseMoveSpeed = 15f;
    public float speedIncreaseRate = 0.2f;

    private PlayerMovement player;

    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
    }

    void Update()
    {
        if (player == null) return;
        if (!player.CanStartRunning) return;

        float speedMultiplier = player.gameSpeed;
        float currentSpeed = baseMoveSpeed * speedMultiplier;

        transform.position += Vector3.back * currentSpeed * Time.deltaTime;

        if (transform.position.z < -30f)
        {
            Destroy(gameObject);
        }
    }
}