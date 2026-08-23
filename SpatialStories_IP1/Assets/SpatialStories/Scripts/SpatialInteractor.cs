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
    private TrashBin activeTrashTarget;

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
        {
            draggedOverlay.Drag(this);
            UpdateTrashDropTarget();
        }

        if (draggedOverlay != null && LeftReleasedThisFrame())
        {
            OverlayItem releasedOverlay = draggedOverlay;
            draggedOverlay = null;

            releasedOverlay.EndDrag(this);
            ClearTrashDropTarget();
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

    private void UpdateTrashDropTarget()
    {
        TrashBin nextTarget = null;
        TryGetTrashUnderPointer(out nextTarget);

        if (nextTarget == activeTrashTarget)
            return;

        if (activeTrashTarget != null)
            activeTrashTarget.SetDropTarget(false);

        activeTrashTarget = nextTarget;

        if (activeTrashTarget != null)
        {
            activeTrashTarget.SetDropTarget(true);

            if (StorySessionManager.Instance != null)
                StorySessionManager.Instance.SetStatus(
                    "Release to delete this text or sticker.");
        }
    }

    private void ClearTrashDropTarget()
    {
        if (activeTrashTarget != null)
            activeTrashTarget.SetDropTarget(false);

        activeTrashTarget = null;
    }

    public Ray GetPointerRay()
    {
        Vector2 pointer = PointerPosition();
        return interactionCamera.ScreenPointToRay(pointer);
    }

    public bool TryGetPointerHit(out RaycastHit hit)
    {
        return Physics.Raycast(
            GetPointerRay(),
            out hit,
            maxDistance,
            interactionMask,
            QueryTriggerInteraction.Ignore);
    }

    public bool TryGetTrashUnderPointer(out TrashBin trash)
    {
        trash = null;

        RaycastHit[] hits = Physics.RaycastAll(
            GetPointerRay(),
            maxDistance,
            interactionMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
            return false;

        System.Array.Sort(
            hits,
            (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            TrashBin candidate =
                hit.collider.GetComponentInParent<TrashBin>();

            if (candidate != null)
            {
                trash = candidate;
                return true;
            }
        }

        return false;
    }

    private Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null
            ? Mouse.current.position.ReadValue()
            : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    private bool LeftPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null &&
               Mouse.current.leftButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    private bool LeftIsPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null &&
               Mouse.current.leftButton.isPressed;
#else
        return Input.GetMouseButton(0);
#endif
    }

    private bool LeftReleasedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null &&
               Mouse.current.leftButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(0);
#endif
    }
}
