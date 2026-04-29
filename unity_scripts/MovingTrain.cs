using UnityEngine;

public class MovingTrain : MonoBehaviour
{
    public float moveSpeed = 15f;
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
        transform.Translate(Vector3.back * moveSpeed * speedMultiplier * Time.deltaTime);

        if (transform.position.z < -120f)
        {
            Destroy(gameObject);
        }
    }
}