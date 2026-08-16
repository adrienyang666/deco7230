using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class SpatialInteractor : MonoBehaviour
{
    [Header("Raycast")]
    public Camera interactionCamera;
    public float maxDistance = 30f;
    public LayerMask interactionMask = ~0;

    private InteractiveObject hoveredObject;
    private OverlayItem draggedOverlay;

    private void Awake()
    {
        if (interactionCamera == null)
            interactionCamera = Camera.main;
    }

    private void Update()
    {
        if (interactionCamera == null)
            return;

        UpdateHover();

        if (LeftPressedThisFrame())
        {
            if (hoveredObject is OverlayItem overlay)
            {
                draggedOverlay = overlay;
                draggedOverlay.BeginDrag(this);
            }
            else if (hoveredObject != null)
            {
                hoveredObject.Interact(this);
            }
        }

        if (draggedOverlay != null && LeftIsPressed())
            draggedOverlay.Drag(this);

        if (draggedOverlay != null && LeftReleasedThisFrame())
        {
            draggedOverlay.EndDrag(this);
            draggedOverlay = null;
        }
    }

    private void UpdateHover()
    {
        InteractiveObject current = null;
        if (TryGetPointerHit(out RaycastHit hit))
            current = hit.collider.GetComponentInParent<InteractiveObject>();

        if (current == hoveredObject)
            return;

        if (hoveredObject != null)
            hoveredObject.SetHovered(false);

        hoveredObject = current;

        if (hoveredObject != null)
            hoveredObject.SetHovered(true);
    }

    public Ray GetPointerRay()
    {
        Vector2 pointer = PointerPosition();
        return interactionCamera.ScreenPointToRay(pointer);
    }

    public bool TryGetPointerHit(out RaycastHit hit)
    {
        return Physics.Raycast(GetPointerRay(), out hit, maxDistance, interactionMask, QueryTriggerInteraction.Ignore);
    }

    private Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();
#endif
        return Input.mousePosition;
    }

    private bool LeftPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasPressedThisFrame;
#endif
        return Input.GetMouseButtonDown(0);
    }

    private bool LeftIsPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.leftButton.isPressed;
#endif
        return Input.GetMouseButton(0);
    }

    private bool LeftReleasedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
            return Mouse.current.leftButton.wasReleasedThisFrame;
#endif
        return Input.GetMouseButtonUp(0);
    }
}
