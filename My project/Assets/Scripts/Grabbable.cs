using UnityEngine;

public class Grabbable : MonoBehaviour
{
    public bool isGrabbed = false;

    void Start()
    {
        if (GetComponent<Rigidbody>() == null)
        {
            gameObject.AddComponent<Rigidbody>();
        }
    }
}