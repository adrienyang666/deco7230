using System.Collections;
using UnityEngine;

public class PublishButton : InteractiveObject
{
    [Header("Visual feedback")]
    public Transform visual;
    public float hoverScale = 1.10f;
    public float readyStartScale = 0.82f;
    public float readyDuration = 0.22f;
    public float pressScale = 0.92f;
    public float pressDuration = 0.07f;

    private Vector3 normalScale;
    private Coroutine feedbackRoutine;
    private bool locked;

    private void Awake()
    {
        if (visual == null)
            visual = transform;

        normalScale = visual.localScale;
    }

    private void OnEnable()
    {
        locked = false;

        if (visual == null)
            visual = transform;

        if (normalScale == Vector3.zero)
            normalScale = visual.localScale;

        StartFeedback(ReadyRoutine());
    }

    private void OnDisable()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        if (visual != null && normalScale != Vector3.zero)
            visual.localScale = normalScale;
    }

    public override void SetHovered(bool hovered)
    {
        if (locked || visual == null || feedbackRoutine != null)
            return;

        visual.localScale = hovered
            ? normalScale * hoverScale
            : normalScale;
    }

    public override void Interact(SpatialInteractor interactor)
    {
        if (locked)
            return;

        locked = true;
        StartFeedback(PressRoutine());
    }

    private void StartFeedback(IEnumerator routine)
    {
        if (feedbackRoutine != null)
            StopCoroutine(feedbackRoutine);

        feedbackRoutine = StartCoroutine(routine);
    }

    private IEnumerator ReadyRoutine()
    {
        Vector3 start = normalScale * readyStartScale;
        visual.localScale = start;

        float elapsed = 0f;

        while (elapsed < readyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, readyDuration));
            float smooth = Mathf.SmoothStep(0f, 1f, t);

            visual.localScale = Vector3.Lerp(start, normalScale, smooth);
            yield return null;
        }

        visual.localScale = normalScale;
        feedbackRoutine = null;
    }

    private IEnumerator PressRoutine()
    {
        Vector3 start = visual.localScale;
        Vector3 target = normalScale * pressScale;
        float elapsed = 0f;

        while (elapsed < pressDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, pressDuration));
            visual.localScale = Vector3.Lerp(start, target, t);
            yield return null;
        }

        visual.localScale = target;

        if (StorySessionManager.Instance != null)
            StorySessionManager.Instance.PublishStory();

        feedbackRoutine = null;
    }
}
