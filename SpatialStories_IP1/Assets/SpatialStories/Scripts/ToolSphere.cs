using UnityEngine;

public class ToolSphere : InteractiveObject
{
    public enum ToolType { Text, Sticker }

    public ToolType toolType;
    public Transform visual;
    public float hoverScale = 1.15f;
    public GameObject stickerPaletteRoot;

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
        if (StorySessionManager.Instance == null)
            return;

        if (toolType == ToolType.Text)
        {
            StorySessionManager.Instance.AddTextOverlay();
        }
        else if (stickerPaletteRoot != null)
        {
            stickerPaletteRoot.SetActive(!stickerPaletteRoot.activeSelf);
            StorySessionManager.Instance.SetStatus(stickerPaletteRoot.activeSelf ? "Choose a sticker" : "Sticker palette closed");
        }
    }
}
