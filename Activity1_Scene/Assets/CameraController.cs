using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    public float sensitivity = 0.1f;

    private InputAction lookAction;

    private float horizontalRotation = 0f;
    private float verticalRotation = 15f;

    void Start()
    {
        // Find the Look action from InputSystem_Actions
        lookAction = InputSystem.actions.FindAction("Look");

        // Lock the cursor inside the Game window
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (player == null || lookAction == null)
        {
            return;
        }

        // Read mouse or controller input
        Vector2 lookInput = lookAction.ReadValue<Vector2>();

        // Left and right rotation
        horizontalRotation += lookInput.x * sensitivity;

        // Up and down rotation
        verticalRotation -= lookInput.y * sensitivity;

        // Stop the camera from turning upside down
        verticalRotation = Mathf.Clamp(
            verticalRotation,
            -30f,
            70f
        );

        // Follow the Player
        transform.position = player.position + targetOffset;

        // Rotate the CameraHolder
        transform.rotation = Quaternion.Euler(
            verticalRotation,
            horizontalRotation,
            0f
        );

        // Press Escape to release the mouse
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}