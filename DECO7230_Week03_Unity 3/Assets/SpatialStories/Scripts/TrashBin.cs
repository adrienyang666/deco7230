using UnityEngine;

public class TrashBin : InteractiveObject
{
    public Transform visual;
    public float hoverScale = 1.12f;
    public float dropTargetScale = 1.28f;

    private Vector3 normalScale;
    private bool hovered;
    private bool dropTarget;

    private void Start()
    {
        if (visual == null)
            visual = transform;

        normalScale = visual.localScale;
        RefreshVisual();
    }

    public override void SetHovered(bool value)
    {
        hovered = value;
        RefreshVisual();
    }

    public void SetDropTarget(bool value)
    {
        dropTarget = value;
        RefreshVisual();
    }

    public override void Interact(SpatialInteractor interactor)
    {
        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.DeleteSelectedOverlay();
    }

    private void RefreshVisual()
    {
        if (visual == null)
            return;

        if (dropTarget)
        {
            visual.localScale = normalScale * dropTargetScale;
            return;
        }

        visual.localScale = hovered
            ? normalScale * hoverScale
            : normalScale;
    }
}
