using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class DesktopMRController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float verticalSpeed = 2f;

    [Header("Look")]
    public float lookSensitivity = 0.16f;
    public float minPitch = -65f;
    public float maxPitch = 65f;

    private float yaw;
    private float pitch;
    private Vector2 previousMouse;
    private bool wasLooking;

    private void Start()
    {
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x > 180f ? e.x - 360f : e.x;
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        Vector3 move = Vector3.zero;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) move += Vector3.forward;
            if (Keyboard.current.sKey.isPressed) move += Vector3.back;
            if (Keyboard.current.aKey.isPressed) move += Vector3.left;
            if (Keyboard.current.dKey.isPressed) move += Vector3.right;
            if (Keyboard.current.eKey.isPressed) move += Vector3.up;
            if (Keyboard.current.qKey.isPressed) move += Vector3.down;
        }
#else
        move.x = Input.GetAxisRaw("Horizontal");
        move.z = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.E)) move.y += 1f;
        if (Input.GetKey(KeyCode.Q)) move.y -= 1f;
#endif

        Vector3 planarForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 planarRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        Vector3 worldMove = planarForward * move.z + planarRight * move.x + Vector3.up * move.y;

        float speed = Mathf.Abs(move.y) > 0.01f ? verticalSpeed : moveSpeed;
        if (worldMove.sqrMagnitude > 1f)
            worldMove.Normalize();

        transform.position += worldMove * speed * Time.deltaTime;
    }

    private void HandleLook()
    {
        bool looking = RightMousePressed();
        Vector2 mouse = MousePosition();

        if (looking)
        {
            if (!wasLooking)
                previousMouse = mouse;

            Vector2 delta = mouse - previousMouse;
            yaw += delta.x * lookSensitivity;
            pitch -= delta.y * lookSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            previousMouse = mouse;
        }

        wasLooking = looking;
    }

    private Vector2 MousePosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
#endif
        return Input.mousePosition;
    }

    private bool RightMousePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.rightButton.isPressed;
#endif
        return Input.GetMouseButton(1);
    }
}
