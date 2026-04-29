using UnityEngine;

public class ObstacleType : MonoBehaviour
{
    public enum ObstacleKind
    {
        JumpOver,
        SlideUnder
    }

    public ObstacleKind obstacleKind;
    public float spawnY = 1f;
    public Vector3 spawnOffset = Vector3.zero;
}