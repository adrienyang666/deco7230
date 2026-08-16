using UnityEngine;

public class OverlayItem : InteractiveObject
{
    [HideInInspector] public PhotoCard photo;
    public GameObject selectionFrame;

    [Header("Photo-local drag bounds")]
    public Vector2 minLocal = new Vector2(-0.68f, -0.46f);
    public Vector2 maxLocal = new Vector2(0.68f, 0.46f);
    public float localZ = -0.075f;

    private bool dragging;

    private void Start()
    {
        if (photo == null)
            photo = GetComponentInParent<PhotoCard>();
        SetSelected(false);
    }

    public override void SetHovered(bool hovered) { }

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
        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.MarkEdited("move_overlay", true);
    }

    public void SetSelected(bool value)
    {
        if (selectionFrame != null)
            selectionFrame.SetActive(value);
    }
}
