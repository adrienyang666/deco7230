using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    public int score = 0;

    void OnTriggerEnter(Collider other)
    {
        score += 10;

        Debug.Log("Collected: " + other.name);
        Debug.Log("Score: " + score);

        Destroy(other.gameObject);
    }
}