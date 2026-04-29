using UnityEngine;

public class EndlessGround : MonoBehaviour
{
    public float groundSpeed = 15f;
    public float groundLength = 100f;

    public float overlap = 0.001f;

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
        transform.Translate(Vector3.back * groundSpeed * speedMultiplier * Time.deltaTime, Space.World);

        // نفس منطقك الأصلي تقريبًا، لكن مع overlap بسيط
        if (transform.position.z < -groundLength)
        {
            transform.position += Vector3.forward * ((groundLength * 2f) - overlap);
        }
    }
}