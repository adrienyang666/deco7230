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

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        if (hoverFrame != null) hoverFrame.SetActive(false);
        if (selectedFrame != null) selectedFrame.SetActive(false);
    }

    public override void SetHovered(bool hovered)
    {
        if (hoverFrame != null && !selected && !editing)
            hoverFrame.SetActive(hovered);
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
        if (hoverFrame != null && value)
            hoverFrame.SetActive(false);
    }

    public void MoveToEditAnchor(Transform anchor)
    {
        if (anchor == null) return;
        editing = true;
        SetSelected(true);
        AnimateTo(anchor.position, anchor.rotation, originalScale * editScaleMultiplier);
    }

    public void MoveToPublishAnchor(Transform anchor)
    {
        if (anchor == null) return;
        editing = false;
        AnimateTo(anchor.position, anchor.rotation, originalScale * publishScaleMultiplier);
    }

    public void ReturnToWall()
    {
        editing = false;
        SetSelected(false);
        AnimateTo(originalPosition, originalRotation, originalScale);
    }

    public void ClearOverlays()
    {
        if (overlayRoot == null) return;
        for (int i = overlayRoot.childCount - 1; i >= 0; i--)
            Destroy(overlayRoot.GetChild(i).gameObject);
    }

    private void AnimateTo(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale)
    {
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine(targetPosition, targetRotation, targetScale));
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale)
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, moveDuration));
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
