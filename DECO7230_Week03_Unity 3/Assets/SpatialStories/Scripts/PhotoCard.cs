using System.Collections;
using UnityEngine;

public class PhotoCard : InteractiveObject
{
    [Header("Identity")]
    public string photoId = "Photo";

    [Header("Visual Feedback")]
    public GameObject hoverFrame;
    public GameObject selectedFrame;
    public Transform overlayRoot;
    public TextMesh captionText;

    [Header("Browse Feedback")]
    public float hoverScaleMultiplier = 1.03f;
    public float selectedScaleMultiplier = 1.06f;
    public float selectedForwardOffset = 0.24f;
    public float browseFeedbackDuration = 0.18f;
    public Color captionNormalColor = new Color(0.78f, 0.80f, 0.84f);
    public Color captionSelectedColor = Color.white;

    [Header("Animation")]
    public float moveDuration = 0.45f;
    public float editScaleMultiplier = 1.85f;
    public float publishScaleMultiplier = 0.72f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Coroutine moveRoutine;
    private bool selected;
    private bool editing;
    private bool hovered;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        if (hoverFrame != null) hoverFrame.SetActive(false);
        if (selectedFrame != null) selectedFrame.SetActive(false);
        UpdateCaptionVisual();
    }

    public override void SetHovered(bool value)
    {
        hovered = value;

        if (editing || selected)
        {
            if (hoverFrame != null)
                hoverFrame.SetActive(false);
            return;
        }

        if (hoverFrame != null)
            hoverFrame.SetActive(value);

        Vector3 targetScale = value
            ? originalScale * hoverScaleMultiplier
            : originalScale;

        AnimateTo(
            originalPosition,
            originalRotation,
            targetScale,
            browseFeedbackDuration);
    }

    public override void Interact(SpatialInteractor interactor)
    {
        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.OnPhotoClicked(this);
    }

    public void SetSelected(bool value)
    {
        selected = value;

        if (selectedFrame != null)
            selectedFrame.SetActive(value);

        if (hoverFrame != null)
            hoverFrame.SetActive(false);

        UpdateCaptionVisual();

        if (editing)
            return;

        if (value)
        {
            // The selected card moves slightly toward the viewer so selection
            // feels like picking up a physical photo rather than only changing colour.
            Vector3 selectedPosition = originalPosition + Vector3.back * selectedForwardOffset;
            AnimateTo(
                selectedPosition,
                originalRotation,
                originalScale * selectedScaleMultiplier,
                browseFeedbackDuration);
        }
        else
        {
            AnimateTo(
                originalPosition,
                originalRotation,
                originalScale,
                browseFeedbackDuration);
        }
    }

    public void MoveToEditAnchor(Transform anchor)
    {
        if (anchor == null) return;

        editing = true;
        selected = true;

        if (hoverFrame != null)
            hoverFrame.SetActive(false);

        if (selectedFrame != null)
            selectedFrame.SetActive(true);

        UpdateCaptionVisual();

        AnimateTo(
            anchor.position,
            anchor.rotation,
            originalScale * editScaleMultiplier,
            moveDuration);
    }

    public void MoveToPublishAnchor(Transform anchor)
    {
        if (anchor == null) return;

        editing = false;

        if (hoverFrame != null)
            hoverFrame.SetActive(false);

        AnimateTo(
            anchor.position,
            anchor.rotation,
            originalScale * publishScaleMultiplier,
            moveDuration);
    }

    public void ReturnToWall()
    {
        editing = false;
        selected = false;
        hovered = false;

        if (hoverFrame != null)
            hoverFrame.SetActive(false);

        if (selectedFrame != null)
            selectedFrame.SetActive(false);

        UpdateCaptionVisual();

        AnimateTo(
            originalPosition,
            originalRotation,
            originalScale,
            moveDuration);
    }

    public void ClearOverlays()
    {
        if (overlayRoot == null) return;

        for (int i = overlayRoot.childCount - 1; i >= 0; i--)
            Destroy(overlayRoot.GetChild(i).gameObject);
    }

    private void UpdateCaptionVisual()
    {
        if (captionText == null)
            return;

        captionText.color = selected || editing
            ? captionSelectedColor
            : captionNormalColor;
    }

    private void AnimateTo(
        Vector3 targetPosition,
        Quaternion targetRotation,
        Vector3 targetScale,
        float duration)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(
            MoveRoutine(targetPosition, targetRotation, targetScale, duration));
    }

    private IEnumerator MoveRoutine(
        Vector3 targetPosition,
        Quaternion targetRotation,
        Vector3 targetScale,
        float duration)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, smooth);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smooth);
            transform.localScale = Vector3.Lerp(startScale, targetScale, smooth);

            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
        transform.localScale = targetScale;
        moveRoutine = null;
    }
}
