#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class SpatialStoriesIP1Builder
{
    private const string ScenePath = "Assets/SpatialStories/Scenes/SpatialStories_IP1.unity";
    private const string MaterialFolder = "Assets/SpatialStories/Materials";
    private const string TextureFolder = "Assets/SpatialStories/Textures";

    private static Material matBackdrop;
    private static Material matFloor;
    private static Material matYellow;
    private static Material matHover;
    private static Material matPanel;
    private static Material matWhite;
    private static Material matRed;
    private static Material matTextTool;
    private static Material matStickerTool;
    private static Material matTrash;
    private static Material matReset;
    private static Material[] photoMaterials;

    static SpatialStoriesIP1Builder()
    {
        EditorApplication.delayCall += AutoBuildIfNeeded;
    }

    private static void AutoBuildIfNeeded()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            BuildPrototype(false);
    }

    [MenuItem("Tools/Spatial Stories/Build or Rebuild IP1 Prototype")]
    public static void BuildFromMenu()
    {
        BuildPrototype(true);
    }

    [MenuItem("Tools/Spatial Stories/Open IP1 Prototype")]
    public static void OpenPrototype()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            BuildPrototype(false);
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static void BuildPrototype(bool showDialog)
    {
        AssetDatabase.Refresh();
        EnsureAssetFolders();
        EnsureMaterials();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        SetupEnvironment();
        Camera camera = SetupCamera();
        SetupAlbum(camera, out List<PhotoCard> cards);

        GameObject managerObject = new GameObject("StorySessionManager");
        StorySessionManager manager = managerObject.AddComponent<StorySessionManager>();

        Transform editAnchor = CreateEmpty("EditAnchor", new Vector3(0f, 1.75f, -3.75f)).transform;
        Transform publishAnchor = CreateEmpty("PublishAnchor", new Vector3(0f, 1.65f, -3.45f)).transform;
        manager.editAnchor = editAnchor;
        manager.publishAnchor = publishAnchor;

        GameObject toolsRoot = CreateEmpty("EditToolsRoot");
        GameObject stickerPalette = CreateEmpty("StickerPaletteRoot", Vector3.zero, toolsRoot.transform);
        GameObject trashRoot = CreateEmpty("TrashRoot");
        GameObject publishRoot = CreateEmpty("PublishRoot");
        GameObject successRoot = CreateEmpty("SuccessRoot");
        GameObject resetRoot = CreateEmpty("ResetRoot");

        manager.toolsRoot = toolsRoot;
        manager.stickerPaletteRoot = stickerPalette;
        manager.trashRoot = trashRoot;
        manager.publishRoot = publishRoot;
        manager.successRoot = successRoot;
        manager.resetRoot = resetRoot;
        manager.overlaySelectionMaterial = matYellow;
        manager.darkPanelMaterial = matPanel;

        BuildEditTools(toolsRoot, stickerPalette);
        BuildTrash(trashRoot);
        BuildPublish(publishRoot);
        BuildSuccess(successRoot, resetRoot, manager);
        BuildWorldLabels(manager);

        toolsRoot.SetActive(false);
        stickerPalette.SetActive(false);
        trashRoot.SetActive(false);
        publishRoot.SetActive(false);
        successRoot.SetActive(false);
        resetRoot.SetActive(false);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Selection.activeGameObject = managerObject;
        SceneView.lastActiveSceneView?.FrameSelected();

        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Spatial Stories — IP1",
                "The complete IP1 scene has been built.\n\nOpen Assets/SpatialStories/Scenes/SpatialStories_IP1.unity and press Play.\n\nControls:\nLeft click = select / interact\nRight-drag = look\nWASD = move\nQ/E = down/up\nR = reset test session",
                "OK");
        }
    }

    private static void SetupEnvironment()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.48f, 0.49f, 0.53f);

        GameObject environment = CreateEmpty("Environment");

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(environment.transform);
        floor.transform.position = new Vector3(0f, -0.72f, -0.4f);
        floor.transform.localScale = new Vector3(12f, 0.18f, 12f);
        ApplyMaterial(floor, matFloor);

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "AlbumBackdrop";
        wall.transform.SetParent(environment.transform);
        wall.transform.position = new Vector3(0f, 2.0f, 0.65f);
        wall.transform.localScale = new Vector3(10.8f, 6.0f, 0.18f);
        ApplyMaterial(wall, matBackdrop);

        GameObject leftPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftPanel.name = "LeftSpatialPanel";
        leftPanel.transform.SetParent(environment.transform);
        leftPanel.transform.position = new Vector3(-5.0f, 1.8f, -2.0f);
        leftPanel.transform.localScale = new Vector3(0.10f, 4.8f, 5.0f);
        ApplyMaterial(leftPanel, matPanel);

        GameObject rightPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightPanel.name = "RightSpatialPanel";
        rightPanel.transform.SetParent(environment.transform);
        rightPanel.transform.position = new Vector3(5.0f, 1.8f, -2.0f);
        rightPanel.transform.localScale = new Vector3(0.10f, 4.8f, 5.0f);
        ApplyMaterial(rightPanel, matPanel);

        GameObject lightGo = new GameObject("Directional Light");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.15f;
        light.color = new Color(1f, 0.96f, 0.91f);
        lightGo.transform.rotation = Quaternion.Euler(48f, -25f, 0f);

        GameObject fillGo = new GameObject("Soft Fill Light");
        Light fill = fillGo.AddComponent<Light>();
        fill.type = LightType.Point;
        fill.range = 13f;
        fill.intensity = 8f;
        fill.color = new Color(0.70f, 0.79f, 1f);
        fillGo.transform.position = new Vector3(0f, 3.5f, -4.5f);
    }

    private static Camera SetupCamera()
    {
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.055f, 0.060f, 0.072f);
        cam.fieldOfView = 60f;
        cam.nearClipPlane = 0.05f;
        camGo.transform.position = new Vector3(0f, 1.65f, -7.4f);
        camGo.transform.rotation = Quaternion.identity;

        DesktopMRController controller = camGo.AddComponent<DesktopMRController>();
        controller.moveSpeed = 2.6f;
        controller.lookSensitivity = 0.12f;

        SpatialInteractor interactor = camGo.AddComponent<SpatialInteractor>();
        interactor.interactionCamera = cam;
        interactor.maxDistance = 30f;

        AudioListener listener = camGo.AddComponent<AudioListener>();
        return cam;
    }

    private static void SetupAlbum(Camera camera, out List<PhotoCard> cards)
    {
        cards = new List<PhotoCard>();
        GameObject root = CreateEmpty("PhotoWall");

        CreateWorldText("Title", "SPATIAL STORIES", new Vector3(0f, 4.35f, -0.05f), 82, 0.070f, Color.white, TextAnchor.MiddleCenter, root.transform, true);
        CreateWorldText("Subtitle", "Instagram Story creation as spatial direct manipulation", new Vector3(0f, 3.86f, -0.06f), 36, 0.055f, new Color(0.78f,0.80f,0.84f), TextAnchor.MiddleCenter, root.transform, false);

        Vector3[] positions =
        {
            new Vector3(-2.15f, 2.70f, 0.04f),
            new Vector3( 0.00f, 2.70f, 0.00f),
            new Vector3( 2.15f, 2.70f, 0.04f),
            new Vector3(-2.15f, 1.12f, 0.04f),
            new Vector3( 0.00f, 1.12f, 0.00f),
            new Vector3( 2.15f, 1.12f, 0.04f)
        };

        string[] labels = { "Sunset", "Coast", "City", "Beach", "Mountain", "Night" };

        for (int i = 0; i < 6; i++)
        {
            PhotoCard card = CreatePhotoCard(i + 1, labels[i], positions[i], root.transform, photoMaterials[i]);
            cards.Add(card);
        }

        CreateWorldText("BrowseHint", "A floating album wall — choose a memory to turn into a Story", new Vector3(0f, 0.06f, -0.05f), 30, 0.052f, new Color(0.66f,0.69f,0.74f), TextAnchor.MiddleCenter, root.transform, false);
    }

    private static PhotoCard CreatePhotoCard(int index, string label, Vector3 position, Transform parent, Material photoMat)
    {
        GameObject root = new GameObject($"Photo_{index:00}_{label}");
        root.transform.SetParent(parent);
        root.transform.position = position;
        root.transform.rotation = Quaternion.identity;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.78f, 1.30f, 0.12f);

        PhotoCard card = root.AddComponent<PhotoCard>();
        card.photoId = label;

        GameObject hover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hover.name = "HoverFrame";
        hover.transform.SetParent(root.transform, false);
        hover.transform.localPosition = new Vector3(0f, 0f, 0.055f);
        hover.transform.localScale = new Vector3(1.88f, 1.40f, 0.035f);
        Object.DestroyImmediate(hover.GetComponent<Collider>());
        ApplyMaterial(hover, matHover);

        GameObject selected = GameObject.CreatePrimitive(PrimitiveType.Cube);
        selected.name = "SelectedFrame_Yellow";
        selected.transform.SetParent(root.transform, false);
        selected.transform.localPosition = new Vector3(0f, 0f, 0.06f);
        selected.transform.localScale = new Vector3(1.91f, 1.43f, 0.04f);
        Object.DestroyImmediate(selected.GetComponent<Collider>());
        ApplyMaterial(selected, matYellow);

        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Cube);
        surface.name = "PhotoSurface";
        surface.transform.SetParent(root.transform, false);
        surface.transform.localPosition = Vector3.zero;
        surface.transform.localScale = new Vector3(1.78f, 1.30f, 0.055f);
        Object.DestroyImmediate(surface.GetComponent<Collider>());
        ApplyMaterial(surface, photoMat);

        GameObject overlayRoot = CreateEmpty("OverlayRoot", Vector3.zero, root.transform);
        overlayRoot.transform.localPosition = Vector3.zero;

        GameObject labelBack = GameObject.CreatePrimitive(PrimitiveType.Cube);
        labelBack.name = "CaptionPlate";
        labelBack.transform.SetParent(root.transform, false);
        labelBack.transform.localPosition = new Vector3(0f, -0.53f, -0.042f);
        labelBack.transform.localScale = new Vector3(1.55f, 0.18f, 0.025f);
        Object.DestroyImmediate(labelBack.GetComponent<Collider>());
        ApplyMaterial(labelBack, matPanel);

        CreateWorldText("Caption", label, new Vector3(0f, -0.535f, -0.065f), 42, 0.040f, Color.white, TextAnchor.MiddleCenter, root.transform, true, true);

        card.hoverFrame = hover;
        card.selectedFrame = selected;
        card.overlayRoot = overlayRoot.transform;
        hover.SetActive(false);
        selected.SetActive(false);
        return card;
    }

    private static void BuildEditTools(GameObject toolsRoot, GameObject stickerPalette)
    {
        CreateWorldText("ToolsLabel", "SPATIAL TOOLS", new Vector3(2.78f, 3.12f, -3.82f), 34, 0.045f, new Color(0.82f,0.84f,0.88f), TextAnchor.MiddleCenter, toolsRoot.transform, true);

        GameObject textTool = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        textTool.name = "TextTool_T";
        textTool.transform.SetParent(toolsRoot.transform);
        textTool.transform.position = new Vector3(2.82f, 2.42f, -3.78f);
        textTool.transform.localScale = Vector3.one * 0.62f;
        ApplyMaterial(textTool, matTextTool);
        ToolSphere text = textTool.AddComponent<ToolSphere>();
        text.toolType = ToolSphere.ToolType.Text;
        text.visual = textTool.transform;
        CreateWorldText("Label_T", "T", new Vector3(0f, 0f, -0.55f), 78, 0.045f, new Color(0.12f,0.12f,0.15f), TextAnchor.MiddleCenter, textTool.transform, true, true);
        CreateWorldText("TextHint", "TEXT", new Vector3(2.82f, 1.94f, -3.80f), 26, 0.037f, new Color(0.76f,0.78f,0.83f), TextAnchor.MiddleCenter, toolsRoot.transform, true);

        GameObject stickerTool = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        stickerTool.name = "StickerTool_S";
        stickerTool.transform.SetParent(toolsRoot.transform);
        stickerTool.transform.position = new Vector3(2.82f, 1.34f, -3.78f);
        stickerTool.transform.localScale = Vector3.one * 0.62f;
        ApplyMaterial(stickerTool, matStickerTool);
        ToolSphere sticker = stickerTool.AddComponent<ToolSphere>();
        sticker.toolType = ToolSphere.ToolType.Sticker;
        sticker.visual = stickerTool.transform;
        sticker.stickerPaletteRoot = stickerPalette;
        CreateWorldText("Label_S", "S", new Vector3(0f, 0f, -0.55f), 78, 0.045f, Color.white, TextAnchor.MiddleCenter, stickerTool.transform, true, true);
        CreateWorldText("StickerHint", "STICKER", new Vector3(2.82f, 0.86f, -3.80f), 26, 0.037f, new Color(0.76f,0.78f,0.83f), TextAnchor.MiddleCenter, toolsRoot.transform, true);

        BuildStickerChoice(stickerPalette.transform, "Heart", "♥", new Vector3(3.85f, 1.78f, -3.77f), new Color(1f, 0.34f, 0.47f));
        BuildStickerChoice(stickerPalette.transform, "Star", "★", new Vector3(3.85f, 1.34f, -3.77f), new Color(1f, 0.80f, 0.26f));
        BuildStickerChoice(stickerPalette.transform, "Smile", "☺", new Vector3(3.85f, 0.90f, -3.77f), new Color(0.42f, 0.88f, 0.78f));
    }

    private static void BuildStickerChoice(Transform parent, string name, string symbol, Vector3 position, Color color)
    {
        GameObject choice = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        choice.name = "Sticker_" + name;
        choice.transform.SetParent(parent);
        choice.transform.position = position;
        choice.transform.localScale = Vector3.one * 0.34f;
        Material mat = CreateOrUpdateMaterial($"StickerChoice_{name}", color, 0.1f, 0.42f);
        ApplyMaterial(choice, mat);

        StickerChoice sc = choice.AddComponent<StickerChoice>();
        sc.symbol = symbol;
        sc.stickerColor = color;
        sc.visual = choice.transform;
        CreateWorldText("Symbol", symbol, new Vector3(0f, 0f, -0.53f), 70, 0.035f, Color.white, TextAnchor.MiddleCenter, choice.transform, true, true);
    }

    private static void BuildTrash(GameObject trashRoot)
    {
        GameObject bin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bin.name = "TrashBin";
        bin.transform.SetParent(trashRoot.transform);
        bin.transform.position = new Vector3(-3.0f, 0.95f, -3.75f);
        bin.transform.localScale = new Vector3(0.88f, 0.82f, 0.35f);
        ApplyMaterial(bin, matTrash);
        TrashBin trash = bin.AddComponent<TrashBin>();
        trash.visual = bin.transform;

        GameObject lid = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lid.name = "Lid";
        lid.transform.SetParent(bin.transform, false);
        lid.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        lid.transform.localScale = new Vector3(1.12f, 0.15f, 1.12f);
        Object.DestroyImmediate(lid.GetComponent<Collider>());
        ApplyMaterial(lid, matTrash);

        CreateWorldText("TrashIcon", "×", new Vector3(0f, 0f, -0.62f), 76, 0.045f, Color.white, TextAnchor.MiddleCenter, bin.transform, true, true);
        CreateWorldText("TrashLabel", "TRASH", new Vector3(-3.0f, 0.28f, -3.76f), 28, 0.040f, new Color(0.75f,0.77f,0.82f), TextAnchor.MiddleCenter, trashRoot.transform, true);
    }

    private static void BuildPublish(GameObject publishRoot)
    {
        GameObject button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        button.name = "PublishButton";
        button.transform.SetParent(publishRoot.transform);
        button.transform.position = new Vector3(2.82f, 0.20f, -3.78f);
        button.transform.localScale = new Vector3(1.35f, 0.52f, 0.30f);
        ApplyMaterial(button, matRed);
        PublishButton publish = button.AddComponent<PublishButton>();
        publish.visual = button.transform;
        CreateWorldText("PublishText", "PUBLISH", new Vector3(0f, 0f, -0.58f), 48, 0.033f, Color.white, TextAnchor.MiddleCenter, button.transform, true, true);
    }

    private static void BuildSuccess(GameObject successRoot, GameObject resetRoot, StorySessionManager manager)
    {
        GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plate.name = "PublishedPlate";
        plate.transform.SetParent(successRoot.transform);
        plate.transform.position = new Vector3(0f, 3.25f, -3.50f);
        plate.transform.localScale = new Vector3(4.2f, 0.78f, 0.16f);
        ApplyMaterial(plate, matPanel);
        Object.DestroyImmediate(plate.GetComponent<Collider>());

        TextMesh success = CreateWorldText("SuccessText", "Story Published  ✓", new Vector3(0f, 3.25f, -3.62f), 56, 0.047f, Color.white, TextAnchor.MiddleCenter, successRoot.transform, true);
        manager.successText = success;
        CreateWorldText("StoryLabel", "YOUR STORY", new Vector3(0f, 0.90f, -3.52f), 30, 0.040f, new Color(0.78f,0.80f,0.84f), TextAnchor.MiddleCenter, successRoot.transform, true);

        GameObject reset = GameObject.CreatePrimitive(PrimitiveType.Cube);
        reset.name = "ResetButton";
        reset.transform.SetParent(resetRoot.transform);
        reset.transform.position = new Vector3(0f, 0.25f, -3.70f);
        reset.transform.localScale = new Vector3(1.55f, 0.46f, 0.28f);
        ApplyMaterial(reset, matReset);
        ResetButton rb = reset.AddComponent<ResetButton>();
        rb.visual = reset.transform;
        CreateWorldText("ResetText", "RESET TEST", new Vector3(0f, 0f, -0.58f), 43, 0.032f, Color.white, TextAnchor.MiddleCenter, reset.transform, true, true);
    }

    private static void BuildWorldLabels(StorySessionManager manager)
    {
        GameObject hud = CreateEmpty("PrototypeGuidance");

        GameObject controlsPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        controlsPlate.name = "ControlsPlate";
        controlsPlate.transform.SetParent(hud.transform);
        controlsPlate.transform.position = new Vector3(0f, 4.02f, -3.55f);
        controlsPlate.transform.localScale = new Vector3(6.2f, 0.54f, 0.10f);
        ApplyMaterial(controlsPlate, matPanel);
        Object.DestroyImmediate(controlsPlate.GetComponent<Collider>());

        CreateWorldText("Controls", "LEFT CLICK  select / pinch     •     RIGHT-DRAG  look     •     WASD  move     •     R  reset", new Vector3(0f, 4.03f, -3.63f), 30, 0.036f, new Color(0.88f,0.89f,0.92f), TextAnchor.MiddleCenter, hud.transform, false);

        GameObject statusPlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        statusPlate.name = "StatusPlate";
        statusPlate.transform.SetParent(hud.transform);
        statusPlate.transform.position = new Vector3(0f, -0.28f, -3.55f);
        statusPlate.transform.localScale = new Vector3(5.9f, 0.52f, 0.10f);
        ApplyMaterial(statusPlate, matPanel);
        Object.DestroyImmediate(statusPlate.GetComponent<Collider>());

        TextMesh status = CreateWorldText("StatusText", "Browse the album. Left click = pinch/select.", new Vector3(0f, -0.28f, -3.63f), 31, 0.038f, Color.white, TextAnchor.MiddleCenter, hud.transform, false);
        manager.statusText = status;
    }

    private static GameObject CreateEmpty(string name)
    {
        return new GameObject(name);
    }

    private static GameObject CreateEmpty(string name, Vector3 worldPosition)
    {
        GameObject go = new GameObject(name);
        go.transform.position = worldPosition;
        return go;
    }

    private static GameObject CreateEmpty(string name, Vector3 localPosition, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        return go;
    }

    private static TextMesh CreateWorldText(string name, string content, Vector3 position, int fontSize, float characterSize, Color color, TextAnchor anchor, Transform parent, bool bold, bool local = false)
    {
        GameObject go = new GameObject(name);
        if (parent != null)
            go.transform.SetParent(parent, false);
        if (local)
            go.transform.localPosition = position;
        else
            go.transform.position = position;
        go.transform.rotation = Quaternion.identity;

        TextMesh text = go.AddComponent<TextMesh>();
        text.text = content;
        text.fontSize = fontSize;
        text.characterSize = characterSize;
        text.color = color;
        text.anchor = anchor;
        text.alignment = TextAlignment.Center;
        text.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        return text;
    }

    private static void EnsureAssetFolders()
    {
        EnsureFolder("Assets/SpatialStories");
        EnsureFolder("Assets/SpatialStories/Scenes");
        EnsureFolder("Assets/SpatialStories/Materials");
        EnsureFolder("Assets/SpatialStories/Textures");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
        string folder = Path.GetFileName(path);

        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        if (!string.IsNullOrEmpty(parent))
            AssetDatabase.CreateFolder(parent, folder);
    }

    private static void EnsureMaterials()
    {
        matBackdrop = CreateOrUpdateMaterial("Backdrop", new Color(0.095f, 0.105f, 0.125f), 0.0f, 0.28f);
        matFloor = CreateOrUpdateMaterial("Floor", new Color(0.060f, 0.065f, 0.078f), 0.0f, 0.18f);
        matYellow = CreateOrUpdateMaterial("SelectionYellow", new Color(1.0f, 0.82f, 0.10f), 0.0f, 0.55f);
        matHover = CreateOrUpdateMaterial("HoverWhite", new Color(0.92f, 0.94f, 0.98f), 0.0f, 0.42f);
        matPanel = CreateOrUpdateMaterial("PanelDark", new Color(0.14f, 0.15f, 0.18f), 0.0f, 0.40f);
        matWhite = CreateOrUpdateMaterial("SoftWhite", new Color(0.92f, 0.93f, 0.96f), 0.0f, 0.48f);
        matRed = CreateOrUpdateMaterial("PublishRed", new Color(0.92f, 0.15f, 0.19f), 0.0f, 0.50f);
        matTextTool = CreateOrUpdateMaterial("TextTool", new Color(0.92f, 0.91f, 0.86f), 0.0f, 0.62f);
        matStickerTool = CreateOrUpdateMaterial("StickerTool", new Color(0.44f, 0.34f, 0.72f), 0.0f, 0.60f);
        matTrash = CreateOrUpdateMaterial("Trash", new Color(0.24f, 0.25f, 0.29f), 0.0f, 0.32f);
        matReset = CreateOrUpdateMaterial("Reset", new Color(0.18f, 0.47f, 0.67f), 0.0f, 0.45f);

        photoMaterials = new Material[6];
        for (int i = 0; i < 6; i++)
        {
            string texturePath = $"{TextureFolder}/Photo_{i + 1:00}.png";
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            photoMaterials[i] = CreateOrUpdatePhotoMaterial($"PhotoMat_{i + 1:00}", tex);
        }
    }

    private static Material CreateOrUpdateMaterial(string name, Color color, float metallic, float smoothness)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = FindLitShader();
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
        if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
        if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Material CreateOrUpdatePhotoMaterial(string name, Texture2D texture)
    {
        string path = $"{MaterialFolder}/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = FindUnlitTextureShader();
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
        if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
        if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static Shader FindLitShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        return shader;
    }

    private static Shader FindUnlitTextureShader()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (shader == null) shader = FindLitShader();
        return shader;
    }

    private static void ApplyMaterial(GameObject go, Material material)
    {
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null && material != null)
            renderer.sharedMaterial = material;
    }

    private static void AddToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene s in EditorBuildSettings.scenes)
            if (s.path != ScenePath)
                scenes.Add(s);
        scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
#endif
