using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1;

    void Update()
    {
        transform.Rotate(0f, 200f * Time.deltaTime, 0f);
    }
}