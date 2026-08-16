using UnityEngine;

public class OverlayItem : InteractiveObject
{
    [HideInInspector] public PhotoCard photo;
    public GameObject selectionFrame;

    [Header("Photo-local drag bounds")]
    public Vector2 minLocal = new Vector2(-0.68f, -0.46f);
    public Vector2 maxLocal = new Vector2(0.68f, 0.46f);
    public float localZ = -0.075f;

    [Header("Direct manipulation")]
    public float minScale = 0.65f;
    public float maxScale = 1.85f;
    public float scaleStep = 0.10f;

    private bool dragging;
    private bool selected;
    private Vector3 initialScale;

    private void Start()
    {
        if (photo == null)
            photo = GetComponentInParent<PhotoCard>();

        initialScale = transform.localScale;
        SetSelected(false);
    }

    public override void SetHovered(bool hovered)
    {
        // Reuse the selection frame as a lightweight hover affordance.
        // A selected overlay keeps the frame visible.
        if (!selected && selectionFrame != null)
            selectionFrame.SetActive(hovered);
    }

    public override void Interact(SpatialInteractor interactor)
    {
        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.SelectOverlay(this);
    }

    public void BeginDrag(SpatialInteractor interactor)
    {
        dragging = true;
        Interact(interactor);
        Drag(interactor);
    }

    public void Drag(SpatialInteractor interactor)
    {
        if (!dragging || photo == null || photo.overlayRoot == null)
            return;

        Vector3 planePoint = photo.transform.TransformPoint(new Vector3(0f, 0f, localZ));
        Plane plane = new Plane(photo.transform.forward, planePoint);
        Ray ray = interactor.GetPointerRay();

        if (!plane.Raycast(ray, out float enter))
            return;

        Vector3 worldPoint = ray.GetPoint(enter);
        Vector3 localPoint = photo.overlayRoot.InverseTransformPoint(worldPoint);

        localPoint.x = Mathf.Clamp(localPoint.x, minLocal.x, maxLocal.x);
        localPoint.y = Mathf.Clamp(localPoint.y, minLocal.y, maxLocal.y);
        localPoint.z = localZ;

        transform.localPosition = localPoint;

        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.MarkEdited("move_overlay", false);
    }

    public void EndDrag(SpatialInteractor interactor)
    {
        dragging = false;

        // Releasing over the spatial trash bin deletes the dragged object.
        // The pointer can reach the bin even though the overlay itself stays
        // constrained to the photo editing plane.
        if (interactor != null &&
            interactor.TryGetTrashUnderPointer(out TrashBin trash))
        {
            trash.SetDropTarget(false);

            if (StorySessionManager.Instance != null)
                StorySessionManager.Instance.DeleteOverlay(this, true);

            return;
        }

        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.MarkEdited("move_overlay", true);
    }

    public void ScaleBySteps(float steps)
    {
        if (Mathf.Approximately(steps, 0f))
            return;

        float currentMultiplier =
            initialScale.x == 0f ? 1f : transform.localScale.x / initialScale.x;

        float nextMultiplier = Mathf.Clamp(
            currentMultiplier + steps * scaleStep,
            minScale,
            maxScale);

        transform.localScale = initialScale * nextMultiplier;

        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.MarkEdited("scale_overlay", false);
    }

    public void RotateBy(float degrees)
    {
        if (Mathf.Approximately(degrees, 0f))
            return;

        transform.Rotate(0f, 0f, degrees, Space.Self);

        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.MarkEdited("rotate_overlay", false);
    }

    public void SetSelected(bool value)
    {
        selected = value;

        if (selectionFrame != null)
            selectionFrame.SetActive(value);
    }
}
