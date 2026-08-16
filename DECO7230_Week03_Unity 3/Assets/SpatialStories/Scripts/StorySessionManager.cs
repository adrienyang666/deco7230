using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class StorySessionManager : MonoBehaviour
{
    public static StorySessionManager Instance { get; private set; }

    [Header("Scene References")]
    public Transform editAnchor;
    public Transform publishAnchor;
    public GameObject toolsRoot;
    public GameObject stickerPaletteRoot;
    public GameObject trashRoot;
    public GameObject publishRoot;
    public GameObject successRoot;
    public GameObject resetRoot;
    public TextMesh statusText;
    public TextMesh successText;

    [Header("Overlay Visuals")]
    public Material overlaySelectionMaterial;
    public Material darkPanelMaterial;

    [Header("Prototype Settings")]
    public string defaultStoryText = "Weekend";

    [Header("Edit Transition")]
    public float editToolsRevealDelay = 0.46f;

    [Header("Overlay Manipulation")]
    public float overlayRotateSpeed = 90f;

    [HideInInspector] public PhotoCard selectedPhoto;
    [HideInInspector] public PhotoCard editingPhoto;
    [HideInInspector] public OverlayItem selectedOverlay;

    private readonly List<PhotoCard> photoCards = new List<PhotoCard>();
    private bool hasEdited;
    private float sessionStart;
    private int textAdds;
    private int stickerAdds;
    private int deletes;
    private int photoSelections;
    private Coroutine editTransitionRoutine;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        photoCards.AddRange(FindObjectsByType<PhotoCard>(FindObjectsSortMode.None));
        sessionStart = Time.time;
        SetActiveSafe(toolsRoot, false);
        SetActiveSafe(trashRoot, false);
        SetActiveSafe(publishRoot, false);
        SetActiveSafe(successRoot, false);
        SetActiveSafe(resetRoot, false);
        SetStatus("Browse the album. Left click = pinch/select.");
    }

    private void Update()
    {
        if (ResetKeyPressed())
            ResetPrototype();

        HandleSelectedOverlayManipulation();
    }

    private void HandleSelectedOverlayManipulation()
    {
        if (selectedOverlay == null || editingPhoto == null)
            return;

        float scroll = ReadScrollSteps();
        if (!Mathf.Approximately(scroll, 0f))
        {
            selectedOverlay.ScaleBySteps(scroll);
            SetStatus("Overlay selected: drag to move • scroll to scale • Z / X to rotate.");
        }

        float rotateDirection = ReadOverlayRotationDirection();
        if (!Mathf.Approximately(rotateDirection, 0f))
        {
            selectedOverlay.RotateBy(
                rotateDirection * overlayRotateSpeed * Time.deltaTime);
            SetStatus("Overlay selected: drag to move • scroll to scale • Z / X to rotate.");
        }
    }

    public void OnPhotoClicked(PhotoCard photo)
    {
        if (photo == null || successRoot != null && successRoot.activeSelf)
            return;

        if (editingPhoto != null)
            return;

        if (selectedPhoto == photo)
        {
            EnterEditMode(photo);
            return;
        }

        if (selectedPhoto != null)
            selectedPhoto.SetSelected(false);

        selectedPhoto = photo;
        selectedPhoto.SetSelected(true);
        photoSelections++;
        SetStatus("Photo selected. Select it again to bring it forward.");
        LogEvent("select_photo", photo.photoId);
    }

    private void EnterEditMode(PhotoCard photo)
    {
        if (editTransitionRoutine != null)
            StopCoroutine(editTransitionRoutine);

        editTransitionRoutine = StartCoroutine(EnterEditModeRoutine(photo));
    }

    private IEnumerator EnterEditModeRoutine(PhotoCard photo)
    {
        editingPhoto = photo;
        selectedPhoto = photo;
        selectedOverlay = null;
        hasEdited = false;

        SetAlbumDimmed(true, photo);
        SetAlbumCaptionsVisible(false);
        SetActiveSafe(toolsRoot, false);
        SetActiveSafe(trashRoot, false);
        SetActiveSafe(publishRoot, false);
        SetActiveSafe(stickerPaletteRoot, false);

        SetStatus("Bringing your photo into the editing space...");
        photo.MoveToEditAnchor(editAnchor);
        LogEvent("enter_edit", photo.photoId);

        yield return new WaitForSeconds(editToolsRevealDelay);

        SetActiveSafe(toolsRoot, true);
        SetActiveSafe(trashRoot, true);
        SetStatus("Edit mode: use T for text or S for stickers.");

        editTransitionRoutine = null;
    }

    private void SetAlbumDimmed(bool dimmed, PhotoCard focusPhoto = null)
    {
        foreach (PhotoCard card in photoCards)
        {
            if (card == null)
                continue;

            bool shouldDim = dimmed && card != focusPhoto;
            card.SetDimmed(shouldDim);
        }
    }

    private void SetAlbumCaptionsVisible(bool visible)
    {
        // Hide the full browsing chrome during editing, not just the captions.
        // This avoids 3D TextMesh objects rendering over the foreground photo.
        foreach (PhotoCard card in photoCards)
        {
            if (card == null)
                continue;

            card.SetBrowseCaptionVisible(visible);
        }

        GameObject photoWall = GameObject.Find("PhotoWall");
        if (photoWall == null)
            return;

        SetNamedChildActive(photoWall.transform, "Title", visible);
        SetNamedChildActive(photoWall.transform, "Subtitle", visible);
        SetNamedChildActive(photoWall.transform, "BrowseHint", visible);
    }

    private static void SetNamedChildActive(Transform parent, string childName, bool active)
    {
        if (parent == null)
            return;

        Transform child = parent.Find(childName);
        if (child != null)
            child.gameObject.SetActive(active);
    }

    public void AddTextOverlay()
    {
        if (editingPhoto == null || editingPhoto.overlayRoot == null)
            return;

        GameObject root = new GameObject("TextOverlay");
        root.transform.SetParent(editingPhoto.overlayRoot, false);
        root.transform.localPosition = new Vector3(0f, 0.26f, -0.075f);

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.9f, 0.26f, 0.06f);

        OverlayItem item = root.AddComponent<OverlayItem>();
        item.photo = editingPhoto;
        item.localZ = -0.075f;

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "SelectionFrame";
        frame.transform.SetParent(root.transform, false);
        frame.transform.localPosition = new Vector3(0f, 0f, 0.035f);
        frame.transform.localScale = new Vector3(0.94f, 0.30f, 0.025f);
        Destroy(frame.GetComponent<Collider>());
        if (overlaySelectionMaterial != null)
            frame.GetComponent<Renderer>().sharedMaterial = overlaySelectionMaterial;
        item.selectionFrame = frame;

        GameObject label = new GameObject("Text");
        label.transform.SetParent(root.transform, false);
        label.transform.localPosition = new Vector3(0f, 0f, -0.02f);
        TextMesh tm = label.AddComponent<TextMesh>();
        tm.text = defaultStoryText;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 64;
        tm.characterSize = 0.075f;
        tm.color = Color.white;
        tm.fontStyle = FontStyle.Bold;

        textAdds++;
        SelectOverlay(item);
        MarkEdited("add_text");
        SetStatus("Text added: drag to move • scroll to scale • Z / X to rotate.");
    }

    public void AddStickerOverlay(string symbol, Color color)
    {
        if (editingPhoto == null || editingPhoto.overlayRoot == null)
            return;

        GameObject root = new GameObject("Sticker_" + symbol);
        root.transform.SetParent(editingPhoto.overlayRoot, false);
        root.transform.localPosition = new Vector3(0.34f, -0.12f, -0.075f);

        BoxCollider col = root.AddComponent<BoxCollider>();
        col.size = new Vector3(0.40f, 0.40f, 0.06f);

        OverlayItem item = root.AddComponent<OverlayItem>();
        item.photo = editingPhoto;
        item.localZ = -0.075f;

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "SelectionFrame";
        frame.transform.SetParent(root.transform, false);
        frame.transform.localPosition = new Vector3(0f, 0f, 0.035f);
        frame.transform.localScale = new Vector3(0.42f, 0.42f, 0.025f);
        Destroy(frame.GetComponent<Collider>());
        if (overlaySelectionMaterial != null)
            frame.GetComponent<Renderer>().sharedMaterial = overlaySelectionMaterial;
        item.selectionFrame = frame;

        GameObject label = new GameObject("StickerSymbol");
        label.transform.SetParent(root.transform, false);
        label.transform.localPosition = new Vector3(0f, -0.005f, -0.02f);
        TextMesh tm = label.AddComponent<TextMesh>();
        tm.text = symbol;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontSize = 80;
        tm.characterSize = 0.095f;
        tm.color = color;
        tm.fontStyle = FontStyle.Bold;

        stickerAdds++;
        SelectOverlay(item);
        MarkEdited("add_sticker:" + symbol);
        SetActiveSafe(stickerPaletteRoot, false);
        SetStatus("Sticker added: drag to move • scroll to scale • Z / X to rotate.");
    }

    public void SelectOverlay(OverlayItem item)
    {
        if (selectedOverlay != null && selectedOverlay != item)
            selectedOverlay.SetSelected(false);
        selectedOverlay = item;
        if (selectedOverlay != null)
        {
            selectedOverlay.SetSelected(true);
            SetStatus("Overlay selected: drag to move • scroll to scale • Z / X to rotate.");
        }
    }

    public void DeleteSelectedOverlay()
    {
        if (selectedOverlay == null)
        {
            SetStatus("Select a text or sticker first, then use the trash bin.");
            return;
        }

        OverlayItem doomed = selectedOverlay;
        selectedOverlay = null;
        Destroy(doomed.gameObject);
        deletes++;
        MarkEdited("delete_overlay");
        SetStatus("Overlay removed.");
    }

    public void MarkEdited(string eventName, bool logEvent = true)
    {
        hasEdited = true;
        SetActiveSafe(publishRoot, true);
        if (logEvent)
            LogEvent(eventName, editingPhoto != null ? editingPhoto.photoId : "");
    }

    public void PublishStory()
    {
        if (editingPhoto == null || !hasEdited)
        {
            SetStatus("Add at least one edit before publishing.");
            return;
        }

        editingPhoto.MoveToPublishAnchor(publishAnchor);
        SetAlbumDimmed(false);
        SetActiveSafe(toolsRoot, false);
        SetActiveSafe(stickerPaletteRoot, false);
        SetActiveSafe(trashRoot, false);
        SetActiveSafe(publishRoot, false);
        SetActiveSafe(successRoot, true);
        SetActiveSafe(resetRoot, true);
        if (successText != null)
            successText.text = "Story Published  ✓\n" + editingPhoto.photoId;
        SetStatus("Published. Press R or select RESET for the next participant.");
        LogEvent("publish", editingPhoto.photoId);
        SaveSessionRow();
    }

    public void ResetPrototype()
    {
        if (editTransitionRoutine != null)
        {
            StopCoroutine(editTransitionRoutine);
            editTransitionRoutine = null;
        }

        SetAlbumDimmed(false);
        SetAlbumCaptionsVisible(true);

        foreach (PhotoCard card in photoCards)
        {
            card.ClearOverlays();
            card.ReturnToWall();
        }

        selectedPhoto = null;
        editingPhoto = null;
        selectedOverlay = null;
        hasEdited = false;
        textAdds = 0;
        stickerAdds = 0;
        deletes = 0;
        photoSelections = 0;
        sessionStart = Time.time;

        SetActiveSafe(toolsRoot, false);
        SetActiveSafe(stickerPaletteRoot, false);
        SetActiveSafe(trashRoot, false);
        SetActiveSafe(publishRoot, false);
        SetActiveSafe(successRoot, false);
        SetActiveSafe(resetRoot, false);
        SetStatus("New session. Browse the album and choose a photo.");
        LogEvent("reset", "");
    }

    public void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private void LogEvent(string eventName, string detail)
    {
        Debug.Log($"[SpatialStories Test] {Time.time - sessionStart:F2}s | {eventName} | {detail}");
    }

    private void SaveSessionRow()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, "SpatialStories_TestLog.csv");
            bool newFile = !File.Exists(path);
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                if (newFile)
                    writer.WriteLine("timestamp,photo,selections,textAdds,stickerAdds,deletes,durationSeconds");
                string photo = editingPhoto != null ? editingPhoto.photoId : "";
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss},{photo},{photoSelections},{textAdds},{stickerAdds},{deletes},{Time.time - sessionStart:F2}");
            }
            Debug.Log("[SpatialStories Test] CSV saved to: " + path);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Could not save Spatial Stories test log: " + ex.Message);
        }
    }

    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private float ReadScrollSteps()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current != null)
        {
            float y = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Abs(y) > 0.01f)
                return Mathf.Sign(y);
        }
#else
        float y = Input.mouseScrollDelta.y;
        if (Mathf.Abs(y) > 0.01f)
            return Mathf.Sign(y);
#endif
        return 0f;
    }

    private float ReadOverlayRotationDirection()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            float direction = 0f;
            if (Keyboard.current.zKey.isPressed) direction += 1f;
            if (Keyboard.current.xKey.isPressed) direction -= 1f;
            return direction;
        }
#else
        float direction = 0f;
        if (Input.GetKey(KeyCode.Z)) direction += 1f;
        if (Input.GetKey(KeyCode.X)) direction -= 1f;
        return direction;
#endif
        return 0f;
    }

    private bool ResetKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
            return Keyboard.current.rKey.wasPressedThisFrame;
#endif
        return Input.GetKeyDown(KeyCode.R);
    }
}
