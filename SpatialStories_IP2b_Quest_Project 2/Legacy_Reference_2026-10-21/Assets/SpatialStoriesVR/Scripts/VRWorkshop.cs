using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SpatialStories.Quest
{
    public sealed class VRWorkshop : MonoBehaviour
    {
        [Tooltip("Existing IP1 photographs, in Ekka/Auckland/Piha/Aurora/Paris/Shanghai order.")]
        public Texture2D[] photographs = new Texture2D[6];
        public Camera head;
        public VRSessionLog Log { get; private set; }
        public string StageName => flow.Stage.ToString();
        readonly StoryFlow flow = new StoryFlow();
        readonly List<VROverlay> overlays = new List<VROverlay>();
        readonly Stack<VROverlay> deleted = new Stack<VROverlay>();
        readonly string[] names = { "Ekka", "Auckland", "Piha", "Aurora", "Paris", "Shanghai" };
        Vector3 boardPosition = new Vector3(0, 1.36f, .94f);
        const float BoardWidth = .495f, BoardHeight = .88f;
        [SerializeField, HideInInspector] Transform workspace, board, overlayRoot;
        [SerializeField, HideInInspector] GameObject welcome, album, editTools, previewPanel, success, changeDialog, adminPanel, trash;
        [SerializeField, HideInInspector] GameObject photoSurface, frame, tray;
        [SerializeField, HideInInspector] Text status, adminStatus, boardTitle, trashText, progress;
        [SerializeField, HideInInspector] Button previewButton, undoButton;
        [SerializeField, HideInInspector] Material ink, panel, blue, mint, white, red;
        Material trashMaterial;
        public bool HasSavedLayout => workspace && board && overlayRoot && welcome && album && editTools &&
            previewPanel && success && changeDialog && adminPanel && trash && photoSurface && frame && tray &&
            status && adminStatus && boardTitle && trashText && progress && previewButton && undoButton && editControls;
        int serial;
        bool entering, modal;

        public bool Holding => overlays.Any(o => o && o.gameObject.activeSelf && o.Busy);
        int ActiveCount => overlays.Count(o => o && o.gameObject.activeSelf);
        bool AllPlaced => overlays.Where(o => o && o.gameObject.activeSelf).All(o => o.Snapped);

        // Normalize photo sampling after importing older scene serialization.
        // Only album RawImages are touched; textures and layout stay unchanged.
        public void RestoreAlbumPhotoUVs()
        {
            if (!album) return;
            foreach (var image in album.GetComponentsInChildren<RawImage>(true))
                image.uvRect = new Rect(0f, 0f, 1f, 1f);
        }

        void Start()
        {
            head = head ? head : Camera.main;
            Log = gameObject.AddComponent<VRSessionLog>();
            if (!ink) ink = VRVisuals.Material(VRVisuals.Ink);
            if (!panel) panel = VRVisuals.Material(VRVisuals.Panel);
            if (!blue) blue = VRVisuals.Material(VRVisuals.Blue);
            if (!mint) mint = VRVisuals.Material(VRVisuals.Mint);
            if (!white) white = VRVisuals.Material(Color.white);
            if (!red) red = VRVisuals.Material(VRVisuals.Red);
            EnsureEventSystem();
            if (HasSavedLayout)
            {
                // Use the actual saved scene objects, including the user's transform edits.
                RestoreAlbumPhotoUVs();
                boardPosition = board.localPosition;
                var binRenderer = trash.GetComponentInChildren<Renderer>(true);
                trashMaterial = new Material(binRenderer ? binRenderer.sharedMaterial : panel);
                foreach (var renderer in trash.GetComponentsInChildren<MeshRenderer>(true))
                    renderer.sharedMaterial = trashMaterial;
                foreach (var canvas in workspace.GetComponentsInChildren<Canvas>(true)) canvas.worldCamera = head;
            }
            else if (workspace || transform.childCount > 0)
            {
                Debug.LogError("Spatial Stories: saved layout references are incomplete. Reimport the complete scene patch; no duplicate workspace was created.", this);
                enabled = false; return;
            }
            else
            {
                // Compatibility only for untouched scenes from the first source release.
                trashMaterial = new Material(panel);
                workspace = VRVisuals.Root("Story workspace", transform, Vector3.zero).transform;
                BuildRoom(); BuildStatus(); BuildWelcome(); BuildAlbum(); BuildEditor(); BuildAdmin();
            }
            adminPanel.SetActive(false); modal = false; entering = false;
            Refresh();
            Hint("A small space for your stories.\nPoint at BEGIN and press the trigger.");
            StartCoroutine(AlignWelcome());
        }

        IEnumerator AlignWelcome()
        {
            // Wait for the tracked camera/simulator to publish its first pose.
            yield return new WaitForSecondsRealtime(.5f);
            if (flow.Stage == StoryStage.Welcome && !modal) PositionDesk();
        }

        void EnsureEventSystem()
        {
            var events = FindFirstObjectByType<EventSystem>();
            if (!events) events = new GameObject("EventSystem").AddComponent<EventSystem>();
            foreach (var module in events.GetComponents<BaseInputModule>())
                if (!(module is XRUIInputModule)) { module.enabled = false; Destroy(module); }
            if (!events.GetComponent<XRUIInputModule>()) events.gameObject.AddComponent<XRUIInputModule>();
        }

        void BuildRoom()
        {
            QualitySettings.antiAliasing = 4;
            QualitySettings.shadows = ShadowQuality.Disable;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.65f, .65f, .65f);
            VRVisuals.Box("Floor", transform, new Vector3(0, -.04f, 0), new Vector3(14, .08f, 14), ink);
            VRVisuals.Box("Rear wall", transform, new Vector3(0, 2, 4), new Vector3(10, 4, .08f), panel);
            // Stable peripheral references, no artificial camera movement or automatic locomotion.
            for (int i = -3; i <= 3; i++)
                VRVisuals.Box("Wall seam", transform, new Vector3(i, 2, 3.94f), new Vector3(.01f, 4, .01f), ink);
            VRVisuals.Box("Desk", workspace, new Vector3(0, .67f, 1), new Vector3(1.65f, .045f, .7f), panel);
        }

        void BuildStatus()
        {
            progress = VRVisuals.Label("Stage", workspace, "SPATIAL STORIES", new Vector3(0, 2.02f, 1.65f), .035f, VRVisuals.Mint);
            var canvas = VRVisuals.Canvas("Contextual guidance", workspace, new Vector3(0, 1.91f, 1.58f), new Vector2(1200, 180), head);
            status = VRVisuals.UIText(canvas.transform, "", Vector2.zero, new Vector2(1160, 170), 30, Color.white);
        }

        void BuildWelcome()
        {
            var canvas = VRVisuals.Canvas("Welcome", workspace, new Vector3(0, 1.35f, 1.45f), new Vector2(900, 650), head);
            welcome = canvas.gameObject;
            VRVisuals.UIText(canvas.transform, "SPATIAL STORIES\nYour memories, within reach.", new Vector2(0, 190), new Vector2(860, 140), 38, Color.white);
            VRVisuals.UIText(canvas.transform,
                "Point + trigger: buttons and photo cards\nHold grip: pick up text and stickers\nTwo grips: hold with both hands to resize\nRelease: place on the photo or in the bin",
                new Vector2(0, 25), new Vector2(860, 160), 26, Color.white);
            VRVisuals.UIText(canvas.transform, "Stand or sit comfortably. You can stay in one place.", new Vector2(0, -110), new Vector2(860, 55), 23, VRVisuals.Mint);
            VRVisuals.Button(canvas.transform, "BEGIN", new Vector2(0, -220), new Vector2(300, 75), Begin, VRVisuals.Blue);
        }

        void BuildAlbum()
        {
            album = VRVisuals.Root("Six-photo album", workspace, Vector3.zero);
            for (int i = 0; i < 6; i++)
            {
                int index = i;
                var card = VRVisuals.Canvas(names[i], album.transform,
                    new Vector3((i % 3 - 1) * .39f, 1.54f - (i / 3) * .47f, 1.63f), new Vector2(265, 425), head);
                var button = VRVisuals.Button(card.transform, "", Vector2.zero, new Vector2(265, 425), () => SelectPhoto(index), Color.white);
                var colors = button.colors; colors.normalColor = VRVisuals.Panel;
                colors.highlightedColor = VRVisuals.Mint; colors.pressedColor = VRVisuals.Blue; button.colors = colors;
                var image = new GameObject("Photo", typeof(RectTransform)); image.transform.SetParent(button.transform, false);
                ((RectTransform)image.transform).sizeDelta = new Vector2(225, 400);
                var raw = image.AddComponent<RawImage>(); raw.texture = photographs[i]; raw.raycastTarget = false;
                VRVisuals.UIText(card.transform, names[i], new Vector2(0, -238), new Vector2(300, 48), 26, Color.white);
            }
        }

        Material PhotoMaterial(int index)
        {
            if (index < photographs.Length && photographs[index]) return VRVisuals.Material(Color.white, photographs[index]);
            Debug.LogWarning("Missing photo " + index + ". Run Tools > Spatial Stories > Create Quest VR Scene again.");
            return panel;
        }

        void BuildEditor()
        {
            board = VRVisuals.Root("Photo editing board", workspace, boardPosition).transform;
            frame = VRVisuals.Box("Placement border", board, new Vector3(0, 0, .016f), new Vector3(BoardWidth + .025f, BoardHeight + .025f, .02f), mint);
            photoSurface = VRVisuals.Quad("Selected photograph", board, Vector3.zero, new Vector2(BoardWidth, BoardHeight), white);
            boardTitle = VRVisuals.Label("Photo name", board, "", new Vector3(0, .50f, 0), .026f, Color.white);
            overlayRoot = VRVisuals.Root("Editable objects", workspace, Vector3.zero).transform;
            tray = VRVisuals.Root("New item tray", workspace, new Vector3(.38f, .91f, .69f));
            VRVisuals.Box("Tray", tray.transform, Vector3.zero, new Vector3(.30f, .015f, .21f), panel);
            VRVisuals.Label("Tray label", tray.transform, "PICK UP", new Vector3(0, -.045f, -.11f), .020f, VRVisuals.Mint);

            var tools = VRVisuals.Canvas("Text and sticker tools", workspace, new Vector3(.64f, 1.36f, 1.18f), new Vector2(460, 680), head);
            editTools = tools.gameObject;
            VRVisuals.UIText(tools.transform, "TEXT", new Vector2(0, 275), new Vector2(440, 50), 32, VRVisuals.Mint);
            VRVisuals.Button(tools.transform, "Weekend", new Vector2(0, 210), new Vector2(320, 60), () => AddOverlay("Weekend", false), VRVisuals.Panel);
            VRVisuals.Button(tools.transform, "Good day", new Vector2(0, 140), new Vector2(320, 60), () => AddOverlay("Good day", false), VRVisuals.Panel);
            VRVisuals.UIText(tools.transform, "STICKERS", new Vector2(0, 60), new Vector2(440, 50), 32, VRVisuals.Mint);
            VRVisuals.Button(tools.transform, "Star", new Vector2(-105, -15), new Vector2(190, 65), () => AddOverlay("Star", true), VRVisuals.Panel);
            VRVisuals.Button(tools.transform, "Heart", new Vector2(105, -15), new Vector2(190, 65), () => AddOverlay("Heart", true), VRVisuals.Panel);
            VRVisuals.Button(tools.transform, "Smile", new Vector2(0, -90), new Vector2(190, 65), () => AddOverlay("Smile", true), VRVisuals.Panel);
            VRVisuals.UIText(tools.transform, "New items appear on the tray.\nGrip, move, then release on your photo.", new Vector2(0, -220), new Vector2(450, 110), 24, Color.white);
            // Permanent previews, matching the IP1 request to see stickers before choosing them.
            foreach (var name in new[] { "Star", "Heart", "Smile" })
                BuildSticker(name, VRVisuals.Root(name + " preview", editTools.transform, Vector3.zero).transform);
            // Previews above use canvas scale, so position/scale them in UI units.
            var previews = editTools.transform.Cast<Transform>().Where(t => t.name.EndsWith(" preview")).ToArray();
            for (int i = 0; i < previews.Length; i++) { previews[i].localPosition = new Vector3((i - 1) * 120, -155, 0); previews[i].localScale = Vector3.one * 340; }

            trash = VRVisuals.Root("Release-to-delete bin", workspace, new Vector3(-.60f, 1.04f, .80f));
            VRVisuals.Box("Bin base", trash.transform, new Vector3(0, -.12f, 0), new Vector3(.32f, .03f, .29f), trashMaterial);
            VRVisuals.Box("Left rim", trash.transform, new Vector3(-.15f, 0, 0), new Vector3(.025f, .24f, .29f), trashMaterial);
            VRVisuals.Box("Right rim", trash.transform, new Vector3(.15f, 0, 0), new Vector3(.025f, .24f, .29f), trashMaterial);
            VRVisuals.Box("Back rim", trash.transform, new Vector3(0, 0, .14f), new Vector3(.30f, .24f, .025f), trashMaterial);
            trashText = VRVisuals.Label("Drop cue", trash.transform, "DELETE\nRelease inside", new Vector3(0, -.23f, -.02f), .027f, Color.white);

            var controls = VRVisuals.Canvas("Editing actions", workspace, new Vector3(0, .73f, 1.17f), new Vector2(1260, 160), head);
            VRVisuals.Button(controls.transform, "Change photo", new Vector2(-390, 0), new Vector2(270, 70), RequestChangePhoto, VRVisuals.Panel);
            undoButton = VRVisuals.Button(controls.transform, "Undo delete", new Vector2(-70, 0), new Vector2(270, 70), UndoDelete, VRVisuals.Panel);
            previewButton = VRVisuals.Button(controls.transform, "Preview", new Vector2(270, 0), new Vector2(300, 70), Preview, VRVisuals.Blue);
            controls.gameObject.name = "Edit controls";
            editControls = controls.gameObject;

            var preview = VRVisuals.Canvas("Review and publish", workspace, new Vector3(.64f, 1.36f, 1.18f), new Vector2(460, 550), head);
            previewPanel = preview.gameObject;
            VRVisuals.UIText(preview.transform, "READY TO SHARE?\nCheck your composition.", new Vector2(0, 130), new Vector2(440, 150), 30, Color.white);
            VRVisuals.Button(preview.transform, "Confirm publish", new Vector2(0, -10), new Vector2(370, 75), Publish, VRVisuals.Blue);
            VRVisuals.Button(preview.transform, "Keep editing", new Vector2(0, -110), new Vector2(370, 70), ContinueEditing, VRVisuals.Panel);
            VRVisuals.UIText(preview.transform, "Prototype only: nothing is uploaded.", new Vector2(0, -210), new Vector2(440, 65), 21, Color.white);

            var completed = VRVisuals.Canvas("Published", workspace, new Vector3(.64f, 1.36f, 1.18f), new Vector2(460, 500), head);
            success = completed.gameObject;
            VRVisuals.UIText(completed.transform, "STORY COMPLETE\nYour composition is ready.", new Vector2(0, 130), new Vector2(440, 170), 31, VRVisuals.Mint);
            VRVisuals.UIText(completed.transform, "Simulated publication\nNo Instagram connection", new Vector2(0, -10), new Vector2(440, 90), 23, Color.white);
            VRVisuals.Button(completed.transform, "Next participant", new Vector2(0, -130), new Vector2(360, 75), ResetSession, VRVisuals.Blue);

            var change = VRVisuals.Canvas("Change photo confirmation", workspace, new Vector3(0, 1.35f, .68f), new Vector2(660, 360), head);
            changeDialog = change.gameObject;
            var bg = change.gameObject.AddComponent<Image>(); bg.color = VRVisuals.Ink;
            VRVisuals.UIText(change.transform, "Change photo?\nThis clears the current text and stickers.", new Vector2(0, 80), new Vector2(640, 110), 27, Color.white);
            VRVisuals.Button(change.transform, "Keep editing", new Vector2(-155, -70), new Vector2(285, 75), CancelChange, VRVisuals.Blue);
            VRVisuals.Button(change.transform, "Clear and change", new Vector2(155, -70), new Vector2(285, 75), ConfirmChange, VRVisuals.Red);
        }
        [SerializeField, HideInInspector] GameObject editControls;

        // Int arguments are persistent UnityEvent bindings stored in the .unity files.
        public void HandleButton(int action)
        {
            if (!Application.isPlaying || !enabled || !Log) return;
            if (action >= 10 && action <= 15) { SelectPhoto(action - 10); return; }
            switch (action)
            {
                case 1: Begin(); break;
                case 20: AddOverlay("Weekend", false); break;
                case 21: AddOverlay("Good day", false); break;
                case 22: AddOverlay("Star", true); break;
                case 23: AddOverlay("Heart", true); break;
                case 24: AddOverlay("Smile", true); break;
                case 30: RequestChangePhoto(); break;
                case 31: UndoDelete(); break;
                case 32: Preview(); break;
                case 33: Publish(); break;
                case 34: ContinueEditing(); break;
                case 35: CancelChange(); break;
                case 36: ConfirmChange(); break;
                case 37: ResetSession(); break;
                case 40: ToggleAdmin(); break;
                case 41: RecordPrompt(); break;
                case 42: PositionDesk(); break;
                case 43: EndIncomplete(); break;
            }
        }

        public void ShowLayoutInEditor(int stage)
        {
            if (Application.isPlaying || !HasSavedLayout) return;
            welcome.SetActive(stage == 0); album.SetActive(stage == 1);
            bool edit = stage == 2;
            board.gameObject.SetActive(edit); editTools.SetActive(edit); editControls.SetActive(edit);
            trash.SetActive(edit); tray.SetActive(edit); overlayRoot.gameObject.SetActive(edit);
            previewPanel.SetActive(false); success.SetActive(false);
            changeDialog.SetActive(false); adminPanel.SetActive(false);
            progress.text = stage == 0 ? "SPATIAL STORIES" : stage == 1 ? "01 / CHOOSE A MEMORY" : "02 / MAKE IT YOURS";
            status.text = stage == 0 ? "A small space for your stories." : stage == 1 ? "Choose a memory." : "Add text or a sticker from the tools.";
        }

        void BuildAdmin()
        {
            var toggle = VRVisuals.Canvas("Session menu", workspace, new Vector3(-.67f, .72f, 1.15f), new Vector2(150, 80), head);
            VRVisuals.Button(toggle.transform, "Session", Vector2.zero, new Vector2(150, 60), ToggleAdmin, VRVisuals.Panel);
            var menu = VRVisuals.Canvas("Facilitator controls", workspace, new Vector3(0, 1.35f, .60f), new Vector2(760, 660), head);
            adminPanel = menu.gameObject;
            menu.gameObject.AddComponent<Image>().color = VRVisuals.Ink;
            adminStatus = VRVisuals.UIText(menu.transform, "", new Vector2(0, 235), new Vector2(710, 100), 27, Color.white);
            VRVisuals.Button(menu.transform, "Record facilitator prompt", new Vector2(0, 130), new Vector2(540, 65), RecordPrompt, VRVisuals.Panel);
            VRVisuals.Button(menu.transform, "Position desk in front of me", new Vector2(0, 45), new Vector2(540, 65), PositionDesk, VRVisuals.Panel);
            VRVisuals.Button(menu.transform, "End as incomplete", new Vector2(0, -40), new Vector2(540, 65), EndIncomplete, VRVisuals.Red);
            VRVisuals.Button(menu.transform, "New session", new Vector2(0, -125), new Vector2(540, 65), ResetSession, VRVisuals.Panel);
            VRVisuals.Button(menu.transform, "Close", new Vector2(0, -220), new Vector2(320, 65), ToggleAdmin, VRVisuals.Blue);
            adminPanel.SetActive(false);
        }

        public void Hint(string message) { status.text = message; }
        public void Begin()
        {
            if (flow.Stage != StoryStage.Welcome) return;
            PositionDesk(); flow.Begin(); Log.Begin(); Refresh();
            Hint("Choose a memory.\nPoint at a photo and press the trigger.");
        }
        public void SelectPhoto(int index)
        {
            if (modal || !flow.SelectPhoto(index)) return;
            Log.Event("select_photo", StageName, names[index]);
            photoSurface.GetComponent<Renderer>().sharedMaterial = PhotoMaterial(index);
            boardTitle.text = names[index];
            StartCoroutine(EnterEditor());
        }
        IEnumerator EnterEditor()
        {
            entering = true; Refresh();
            Hint("Your photo is coming closer.");
            Vector3 finish = boardPosition, start = boardPosition + Vector3.forward * .30f;
            float elapsed = 0;
            while (elapsed < .4f)
            {
                elapsed += Time.deltaTime; board.localPosition = Vector3.Lerp(start, finish, Mathf.SmoothStep(0, 1, elapsed / .4f));
                yield return null;
            }
            board.localPosition = finish;
            yield return new WaitForSeconds(.15f);
            entering = false; Refresh();
            Hint("Add text or a sticker from the tools.\nPick it up from the tray and place it on your photo.");
            Log.Event("editor_ready", StageName, names[flow.Photo]);
        }

        void AddOverlay(string value, bool sticker)
        {
            if (flow.Stage != StoryStage.Edit || entering || modal || Holding) return;
            if (ActiveCount >= 6) { Hint("Six items is the limit. Remove an item before adding another."); return; }
            if (overlays.Any(o => o && o.gameObject.activeSelf && !o.Snapped))
            { Hint("Place the item on the tray onto your photo first."); return; }
            var spawn = overlayRoot.InverseTransformPoint(tray.transform.TransformPoint(new Vector3(0, .11f, 0)));
            var root = VRVisuals.Root(value + " " + (++serial), overlayRoot, spawn);
            root.SetActive(false);
            var collider = root.AddComponent<BoxCollider>();
            Vector2 size = sticker ? new Vector2(.145f, .145f) : new Vector2(.235f, .10f);
            collider.size = new Vector3(size.x, size.y, .045f);
            var body = root.AddComponent<Rigidbody>(); body.useGravity = false; body.isKinematic = true;
            var grab = root.AddComponent<XRGrabInteractable>();
            grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
            grab.throwOnDetach = false; grab.useDynamicAttach = true;
            grab.matchAttachPosition = true; grab.matchAttachRotation = true;
            grab.selectMode = InteractableSelectMode.Multiple;
            grab.colliders.Add(collider);
            var transformer = root.AddComponent<XRGeneralGrabTransformer>();
            transformer.allowTwoHandedScaling = true; transformer.allowOneHandedScaling = false;
            transformer.clampScaling = true;
            transformer.minimumScaleRatio = .65f; transformer.maximumScaleRatio = 1.7f;
            grab.addDefaultGrabTransformers = false;
            grab.AddSingleGrabTransformer(transformer); grab.AddMultipleGrabTransformer(transformer);
            var item = root.AddComponent<VROverlay>(); item.workshop = this; item.size = size;
            item.itemId = (sticker ? "sticker_" : "text_") + value + "_" + serial;
            item.outline = VRVisuals.Box("Selection outline", root.transform, new Vector3(0, 0, .018f), new Vector3(size.x + .018f, size.y + .018f, .014f), mint);
            item.outline.SetActive(false);
            VRVisuals.Box("Backing", root.transform, new Vector3(0, 0, .008f), new Vector3(size.x, size.y, .01f), sticker ? panel : ink);
            if (sticker) BuildSticker(value, root.transform);
            else VRVisuals.Label("Text", root.transform, value, new Vector3(0, 0, -.012f), .026f, Color.white);
            overlays.Add(item); root.SetActive(true); item.SavePlacement(false);
            Log.Event(sticker ? "add_sticker" : "add_text", StageName, item.itemId);
            Hint("Your new item is on the tray.\nHold grip to move it; release close to the photo to attach it.");
            Refresh();
        }

        // Shapes are meshes, so sticker appearance does not depend on emoji font support.
        void BuildSticker(string type, Transform parent)
        {
            var points = new List<Vector2>();
            if (type == "Star")
                for (int i = 0; i < 10; i++) { float a = (90 + i * 36) * Mathf.Deg2Rad; points.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (i % 2 == 0 ? .062f : .027f)); }
            else if (type == "Heart")
                for (int i = 0; i < 48; i++)
                { float t = i * Mathf.PI * 2 / 48; points.Add(new Vector2(16 * Mathf.Pow(Mathf.Sin(t), 3), 13 * Mathf.Cos(t) - 5 * Mathf.Cos(2*t) - 2 * Mathf.Cos(3*t) - Mathf.Cos(4*t)) * .0035f); }
            else
                for (int i = 0; i < 40; i++) { float a = i * Mathf.PI * 2 / 40; points.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .059f); }
            Polygon(parent, points, type == "Heart" ? red : VRVisuals.Material(new Color(1, .77f, .24f)), -.013f);
            if (type == "Smile")
            {
                VRVisuals.Box("Eye", parent, new Vector3(-.021f, .016f, -.017f), new Vector3(.009f, .014f, .003f), ink);
                VRVisuals.Box("Eye", parent, new Vector3(.021f, .016f, -.017f), new Vector3(.009f, .014f, .003f), ink);
                for (int i = 0; i < 9; i++)
                { float a = (210 + i * 15) * Mathf.Deg2Rad; VRVisuals.Box("Smile", parent, new Vector3(Mathf.Cos(a) * .03f, Mathf.Sin(a) * .03f, -.017f), new Vector3(.009f, .005f, .003f), ink); }
            }
        }
        void Polygon(Transform parent, List<Vector2> points, Material material, float z)
        {
            var vertices = new Vector3[points.Count + 1]; var triangles = new List<int>();
            for (int i = 0; i < points.Count; i++)
            {
                vertices[i + 1] = points[i]; int next = (i + 1) % points.Count + 1;
                triangles.AddRange(new[] { 0, i + 1, next, 0, next, i + 1 });
            }
            var mesh = new Mesh { vertices = vertices, triangles = triangles.ToArray() }; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            var go = VRVisuals.Root("Sticker shape", parent, new Vector3(0, 0, z));
            go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = material;
            go.AddComponent<VRMeshLifetime>().mesh = mesh;
        }

        public bool InsideTrash(Vector3 point)
        {
            Vector3 p = trash.transform.InverseTransformPoint(point);
            return Mathf.Abs(p.x) <= .17f && p.y >= -.10f && p.y <= .22f && Mathf.Abs(p.z) <= .17f;
        }
        public void ResolveDrop(VROverlay item)
        {
            if (flow.Stage != StoryStage.Edit) { item.RestorePlacement(); return; }
            if (InsideTrash(item.transform.position))
            {
                Log.Event("delete", StageName, item.itemId);
                item.gameObject.SetActive(false); deleted.Push(item);
                Hint("Item removed.\nUse Undo delete if you change your mind."); Refresh(); return;
            }
            Vector3 local = board.InverseTransformPoint(item.transform.position);
            if (Mathf.Abs(local.x) < BoardWidth / 2 + .10f && Mathf.Abs(local.y) < BoardHeight / 2 + .10f && Mathf.Abs(local.z) < .23f)
            {
                Vector3 upOnPhoto = Vector3.ProjectOnPlane(item.transform.up, board.forward);
                float angle = upOnPhoto.sqrMagnitude > .001f ? Vector3.SignedAngle(board.up, upOnPhoto, board.forward) : 0;
                float radians = angle * Mathf.Deg2Rad;
                Vector2 half = item.size * item.transform.localScale.x * .5f;
                float halfX = Mathf.Abs(Mathf.Cos(radians)) * half.x + Mathf.Abs(Mathf.Sin(radians)) * half.y;
                float halfY = Mathf.Abs(Mathf.Sin(radians)) * half.x + Mathf.Abs(Mathf.Cos(radians)) * half.y;
                local.x = Mathf.Clamp(local.x, -BoardWidth / 2 + halfX, BoardWidth / 2 - halfX);
                local.y = Mathf.Clamp(local.y, -BoardHeight / 2 + halfY, BoardHeight / 2 - halfY);
                // Unique depth per item prevents text/stickers from fighting for the same plane.
                local.z = -.018f - overlays.IndexOf(item) * .002f;
                item.transform.position = board.TransformPoint(local);
                item.transform.rotation = board.rotation * Quaternion.Euler(0, 0, angle);
                item.SavePlacement(true); Log.Event("place_on_photo", StageName, item.itemId);
                Hint("Attached. Grab again to adjust it.\nPreview your composition when you are ready.");
            }
            else
            {
                item.RestorePlacement(); Log.Event("invalid_drop_return", StageName, item.itemId);
                Hint("Item returned to its last position.\nRelease it close to the photo, or inside the bin.");
            }
            Refresh();
        }
        void UndoDelete()
        {
            if (flow.Stage != StoryStage.Edit || Holding || modal || deleted.Count == 0) return;
            if (ActiveCount >= 6) { Hint("Remove an item first; six items is the limit."); return; }
            if (!deleted.Peek().Snapped && overlays.Any(o => o && o.gameObject.activeSelf && !o.Snapped))
            { Hint("Place the current tray item onto the photo before restoring another tray item."); return; }
            var item = deleted.Pop(); item.RestorePlacement(); item.gameObject.SetActive(true);
            Log.Event("undo_delete", StageName, item.itemId); Hint("Item restored."); Refresh();
        }
        void Preview()
        {
            if (modal || entering) return;
            if (!AllPlaced) { Hint("Place each item onto the photo before previewing."); return; }
            if (!flow.Preview(ActiveCount, Holding)) return;
            Log.Event("preview", StageName, ""); Refresh(); Hint("Review your Story.\nConfirm publish, or keep editing.");
        }
        void Publish()
        {
            if (modal || !flow.Publish()) return;
            Log.Event("publish", StageName, names[flow.Photo]);
            Log.End("published", "task-level success must be coded from observation");
            Refresh(); Hint("Story complete.\nTell the facilitator how the interaction felt.");
        }
        void ContinueEditing()
        {
            if (modal || !flow.ContinueEditing()) return;
            Log.Event("return_to_edit", StageName, ""); Refresh(); Hint("Continue editing your composition.");
        }
        void RequestChangePhoto()
        {
            if (flow.Stage != StoryStage.Edit || entering || Holding || modal) return;
            modal = true; changeDialog.SetActive(true); SetGrabbing(false); editTools.SetActive(false); editControls.SetActive(false);
        }
        void CancelChange() { modal = false; Refresh(); }
        void ConfirmChange()
        {
            if (!flow.ChangePhoto(Holding)) return;
            ClearOverlays(); modal = false; Log.Event("change_photo", StageName, "confirmed discard"); Refresh();
            Hint("Choose another photo.");
        }
        void ClearOverlays()
        {
            foreach (var item in overlays) if (item) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
            overlays.Clear(); deleted.Clear(); serial = 0;
        }
        void ResetSession()
        {
            if (Holding || entering) { Hint("Release the held item first."); return; }
            Log.End("incomplete", "new_session_requested");
            ClearOverlays(); modal = false; adminPanel.SetActive(false); flow.Reset(); Refresh();
            Hint("Ready for the next participant.\nPress BEGIN when the participant and facilitator are ready.");
        }
        void EndIncomplete()
        {
            if (Holding || entering) { Hint("Release the held item first."); return; }
            Log.End("incomplete", StageName); adminStatus.text = "Session ended as incomplete.\nChoose New session for the next participant.";
            // The menu stays open; no task can continue with logging silently stopped.
        }
        void RecordPrompt() { Log.Prompt(StageName); UpdateAdminStatus(); }
        void ToggleAdmin()
        {
            if (Holding || entering || (modal && !adminPanel.activeSelf)) return;
            if (adminPanel.activeSelf && !Log.Active && flow.Stage != StoryStage.Published && flow.Stage != StoryStage.Welcome)
            { adminStatus.text = "Session ended. Choose New session to continue."; return; }
            bool show = !adminPanel.activeSelf; adminPanel.SetActive(show); modal = show;
            Log.Event(show ? "session_menu_open" : "session_menu_close", StageName, "timing includes menu use");
            Refresh(); UpdateAdminStatus();
        }
        void UpdateAdminStatus()
        {
            adminStatus.text = "FACILITATOR\nSession: " + (Log.SessionId ?? "not started") + " | Prompts: " + Log.Prompts;
            if (!string.IsNullOrEmpty(Log.LastError)) adminStatus.text = Log.LastError;
        }
        void PositionDesk()
        {
            if (!head || Holding || entering) return;
            float yaw = head.transform.eulerAngles.y;
            workspace.position = new Vector3(head.transform.position.x, head.transform.position.y - 1.6f, head.transform.position.z);
            workspace.rotation = Quaternion.Euler(0, yaw, 0);
            Log?.Event("position_desk", StageName, "workspace moved; headset pose unchanged");
        }
        void SetGrabbing(bool enabled)
        {
            foreach (var item in overlays) if (item && item.Grab) item.Grab.enabled = enabled;
        }
        void Refresh()
        {
            bool edit = flow.Stage == StoryStage.Edit;
            welcome.SetActive(flow.Stage == StoryStage.Welcome && !modal);
            album.SetActive(flow.Stage == StoryStage.Browse && !modal);
            board.gameObject.SetActive(flow.Photo >= 0);
            editTools.SetActive(edit && !entering && !modal); editControls.SetActive(edit && !entering && !modal);
            trash.SetActive(edit && !entering && !modal);
            tray.SetActive(edit && !entering && !modal);
            overlayRoot.gameObject.SetActive(flow.Photo >= 0);
            previewPanel.SetActive(flow.Stage == StoryStage.Preview && !modal);
            success.SetActive(flow.Stage == StoryStage.Published && !modal);
            changeDialog.SetActive(modal && !adminPanel.activeSelf);
            SetGrabbing(edit && !modal && !entering);
            previewButton.interactable = ActiveCount > 0 && AllPlaced && !Holding;
            undoButton.interactable = deleted.Count > 0 && !Holding;
            progress.text = flow.Stage == StoryStage.Welcome ? "SPATIAL STORIES" :
                flow.Stage == StoryStage.Browse ? "01 / CHOOSE A MEMORY" :
                flow.Stage == StoryStage.Edit ? "02 / MAKE IT YOURS" :
                flow.Stage == StoryStage.Preview ? "03 / REVIEW YOUR STORY" : "STORY COMPLETE";
        }
        void Update()
        {
            if (!Log) return;
            if (Keyboard.current != null && Keyboard.current.f8Key.wasPressedThisFrame) RecordPrompt();
            if (flow.Stage != StoryStage.Edit || modal) return;
            bool overBin = overlays.Any(o => o && o.gameObject.activeSelf && o.Held && InsideTrash(o.transform.position));
            trashMaterial.color = overBin ? VRVisuals.Red : VRVisuals.Panel;
            trashText.text = overBin ? "RELEASE TO DELETE" : "DELETE\nRelease inside";
            previewButton.interactable = !entering && !Holding && ActiveCount > 0 && AllPlaced;
            undoButton.interactable = deleted.Count > 0 && !Holding;
            bool overPhoto = overlays.Any(o => o && o.gameObject.activeSelf && o.Held && NearPhoto(o.transform.position));
            frame.GetComponent<Renderer>().sharedMaterial = overPhoto ? blue : mint;
        }
        bool NearPhoto(Vector3 point)
        {
            Vector3 p = board.InverseTransformPoint(point);
            return Mathf.Abs(p.x) < BoardWidth / 2 + .1f && Mathf.Abs(p.y) < BoardHeight / 2 + .1f && Mathf.Abs(p.z) < .23f;
        }
        void OnDestroy()
        {
            if (Log) Log.End("incomplete", "scene_closed");
            if (trashMaterial) Destroy(trashMaterial);
            VRVisuals.ReleaseMaterials();
        }
    }
}
