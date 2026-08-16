using UnityEngine;

public class PublishButton : InteractiveObject
{
    public Transform visual;
    public float hoverScale = 1.10f;
    private Vector3 normalScale;

    private void Start()
    {
        if (visual == null) visual = transform;
        normalScale = visual.localScale;
    }

    public override void SetHovered(bool hovered)
    {
        if (visual != null)
            visual.localScale = hovered ? normalScale * hoverScale : normalScale;
    }

    public override void Interact(SpatialInteractor interactor)
    {
        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.PublishStory();
    }
}
