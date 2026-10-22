using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SpatialStories.Immersive.Editor
{
    /// <summary>
    /// Creates native editable scene objects inside copies of the user's XR scenes.
    /// Never runs on import and never changes XR, Android, package or build-profile settings.
    /// </summary>
    public static class ImmersiveSceneBuilder
    {
        const string Root = "Assets/SpatialStoriesImmersive";
        const string Generated = Root + "/Generated";
        const string Quest = Root + "/Scenes/SpatialStories_Full_Quest.unity";
        const string Simulator = Root + "/Scenes/SpatialStories_Full_Simulator.unity";
        const string OldQuest = "Assets/SpatialStoriesVR/Scenes/SpatialStories_IP2a_Quest.unity";
        const string OldSimulator = "Assets/SpatialStoriesVR/Scenes/SpatialStories_IP2a_Simulator.unity";
        const string Menu = "Tools/Spatial Stories Full Concept/";
        const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,!?'-";
        static readonly string[] PhotoNames = { "Ekka", "Auckland", "Piha", "Aurora", "Paris", "Shanghai" };
        static readonly string[] ShapeNames = { "Star", "Heart", "Smile", "Sparkle", "Location", "Music" };
        static readonly Color Ink = new Color(.055f, .08f, .13f);
        static readonly Color PanelColor = new Color(.11f, .15f, .23f);
        static readonly Color Mint = new Color(.24f, .87f, .73f);
        static readonly Color Blue = new Color(.13f, .43f, .92f);
        static readonly Color Gold = new Color(1f, .80f, .20f);
        static readonly Color Red = new Color(.94f, .20f, .27f);
        static readonly Color Pink = new Color(1f, .44f, .68f);
        static ImmersiveStory story;
        static Font font;
        static Material ink, panel, white, mint, blue, gold, red, pink;

        [MenuItem(Menu + "1 - Create or open full scenes", false, 10)]
        public static void CreateScenes()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play first", "Exit Play mode before creating the saved scenes.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            try
            {
                PrepareAssets();
                CreateCopyIfMissing(OldQuest, Quest);
                CreateCopyIfMissing(OldSimulator, Simulator);
                EditorSceneManager.OpenScene(Quest, OpenSceneMode.Single);
                SelectAndFrame();
                EditorUtility.DisplayDialog("Full Spatial Stories scenes ready",
                    "The original Quest and Simulator scenes remain intact.\n\n" +
                    "Open the full Simulator scene from this menu for computer testing. " +
                    "All gallery cards, tool spheres and the bin are saved as editable scene objects.\n\n" +
                    "Existing full scenes are opened, never overwritten.", "OK");
            }
            catch (Exception error)
            {
                Debug.LogException(error);
                EditorUtility.DisplayDialog("Scene creation stopped", error.Message +
                    "\n\nYour original scenes and XR settings have not been changed.", "OK");
            }
        }

        [MenuItem(Menu + "2 - Open full Quest scene", false, 20)]
        public static void OpenQuest() => OpenExisting(Quest);

        [MenuItem(Menu + "3 - Open full Simulator scene", false, 30)]
        public static void OpenSimulator() => OpenExisting(Simulator);

        [MenuItem(Menu + "4 - Build full Quest APK", false, 40)]
        public static void BuildQuest()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Stop Play first", "Exit Play mode before building.", "OK");
                return;
            }
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUtility.DisplayDialog("Android build target required",
                    "This build command preserves your settings. Activate your already configured Android build profile, then try again.", "OK");
                return;
            }
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(Quest))
            {
                EditorUtility.DisplayDialog("Create the full scene first", "Run menu item 1 - Create or open full scenes first.", "OK");
                return;
            }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var path = EditorUtility.SaveFilePanel("Build Spatial Stories for Quest", "", "SpatialStories_Full_Quest.apk", "apk");
            if (string.IsNullOrEmpty(path)) return;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Quest },
                target = BuildTarget.Android,
                locationPathName = path,
                options = BuildOptions.None
            });
            if (report.summary.result == BuildResult.Succeeded)
                EditorUtility.DisplayDialog("APK built", "Saved to:\n" + path +
                    "\n\nInstall this APK on Quest using your classroom deployment method. " +
                    "This build contains the full Quest scene only; it does not include the simulator.", "OK");
            else
                EditorUtility.DisplayDialog("Build did not succeed", "See the Console for the Android build errors.", "OK");
        }

        [MenuItem(Menu + "5 - Frame full workspace", false, 50)]
        public static void SelectAndFrame()
        {
            var controller = UnityEngine.Object.FindFirstObjectByType<ImmersiveStory>();
            if (controller) Selection.activeGameObject = controller.gameObject;
            if (SceneView.lastActiveSceneView)
                SceneView.lastActiveSceneView.LookAt(new Vector3(0f, 1.45f, 1.5f), Quaternion.Euler(4f, 0f, 0f), 2.9f, false, true);
        }

        static void OpenExisting(string path)
        {
            if (Application.isPlaying) { EditorUtility.DisplayDialog("Stop Play first", "Exit Play mode before changing scenes.", "OK"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(path))
            {
                EditorUtility.DisplayDialog("Create the full scenes first", "Run menu item 1 - Create or open full scenes first.", "OK");
                return;
            }
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            SelectAndFrame();
        }

        static void PrepareAssets()
        {
            EnsureFolder(Root + "/Scenes");
            EnsureFolder(Generated + "/Materials");
            EnsureFolder(Generated + "/Meshes");
            EnsureFolder(Generated + "/Prefabs");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!font) throw new InvalidOperationException("Unity's LegacyRuntime.ttf font could not be loaded.");
            if (!Shader.Find("SpatialStoriesImmersive/StereoUnlit"))
                throw new InvalidOperationException("The ImmersiveUnlit shader is not imported yet. Wait for Unity import/compilation, then run this menu again.");
            ink = MaterialAsset("Ink", Ink); panel = MaterialAsset("Panel", PanelColor);
            white = MaterialAsset("White", Color.white); mint = MaterialAsset("Mint", Mint);
            blue = MaterialAsset("Blue", Blue); gold = MaterialAsset("Gold", Gold);
            red = MaterialAsset("Red", Red); pink = MaterialAsset("Pink", Pink);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        static Material MaterialAsset(string name, Color color, Texture texture = null)
        {
            var path = Generated + "/Materials/" + name + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing) return existing;
            var material = new Material(Shader.Find("SpatialStoriesImmersive/StereoUnlit")) { name = name };
            material.SetColor("_Color", color);
            if (material.HasProperty("_Shading")) material.SetFloat("_Shading", texture ? 0f : .7f);
            if (texture) material.SetTexture("_MainTex", texture);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static void CreateCopyIfMissing(string source, string destination)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(destination)) return;
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(source))
                throw new InvalidOperationException("The configured source scene is missing: " + source);
            // Verify the six original textures before copying or touching any scene.
            for (var i = 0; i < 6; i++)
                if (!LoadPhoto(i)) throw new InvalidOperationException("Missing original photo texture: Photo_" + (i + 1).ToString("00") + ".png");
            if (!AssetDatabase.CopyAsset(source, destination))
                throw new InvalidOperationException("Unity could not copy the scene to " + destination);
            var scene = EditorSceneManager.OpenScene(destination, OpenSceneMode.Single);
            try
            {
                Camera head = null;
                var oldWorkshops = new List<GameObject>();
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var component in root.GetComponentsInChildren<MonoBehaviour>(true))
                    {
                        if (!component || component.GetType().FullName != "SpatialStories.Quest.VRWorkshop") continue;
                        var property = new SerializedObject(component).FindProperty("head");
                        if (property != null && property.objectReferenceValue is Camera oldHead) head = oldHead;
                        oldWorkshops.Add(component.gameObject);
                    }
                }
                if (!head)
                    foreach (var root in scene.GetRootGameObjects())
                    {
                        var cameras = root.GetComponentsInChildren<Camera>(true);
                        foreach (var candidate in cameras) if (candidate.CompareTag("MainCamera")) head = candidate;
                    }
                if (!head) throw new InvalidOperationException("No XR Main Camera was found in the source scene.");
                // Destruction is strictly inside the new scene copy, never the original asset.
                foreach (var old in oldWorkshops) UnityEngine.Object.DestroyImmediate(old);
                var full = new GameObject("SPATIAL STORIES - FULL FLOATING CONCEPT");
                story = full.AddComponent<ImmersiveStory>();
                story.head = head;
                MakeEnvironment(full.transform);
                story.contentRoot = Empty("Spatial interface - editable", full.transform, Vector3.zero).transform;
                story.focusAnchor = Empty("Photo inspection anchor", story.contentRoot, new Vector3(0f, 1.50f, 1.05f)).transform;
                story.focusAnchor.localScale = Vector3.one * 1.45f;
                story.spawnAnchor = Empty("New object spawn anchor", story.contentRoot, new Vector3(.47f, 1.25f, .90f)).transform;
                story.reviewAnchor = Empty("Preview anchor", story.contentRoot, new Vector3(0f, 1.80f, 2.4f)).transform;
                MakeGallery();
                MakeGuidance();
                MakeTools();
                MakeBin();
                MakeNavigation();
                MakeTextPanel();
                MakeStickerPanel();
                MakeReviewAndSessionPanels();
                story.textTemplate = TextTemplate();
                story.stickerTemplates = new FloatingStoryItem[6];
                for (var i = 0; i < 6; i++) story.stickerTemplates[i] = StickerTemplate(i);
                story.ShowDesign(0);
                EditorUtility.SetDirty(story);
                AssetDatabase.SaveAssets();
                if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Unity could not save the completed full scene.");
            }
            catch
            {
                // Leave the untouched source open and remove only this unfinished new copy.
                EditorSceneManager.OpenScene(source, OpenSceneMode.Single);
                AssetDatabase.DeleteAsset(destination);
                throw;
            }
        }

        static Texture2D LoadPhoto(int index) => AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/SpatialStories/Textures/Photo_" + (index + 1).ToString("00") + ".png");

        static GameObject Empty(string name, Transform parent, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go;
        }

        static GameObject Primitive(string name, PrimitiveType type, Transform parent, Vector3 pos, Vector3 scale, Material mat, bool collision = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, bool collision = false) =>
            Primitive(name, PrimitiveType.Cube, parent, pos, size, mat, collision);

        static void MakeEnvironment(Transform parent)
        {
            var environment = Empty("Quiet gallery environment - editable", parent, Vector3.zero).transform;
            Box("Floor", environment, new Vector3(0, -.045f, 1.5f), new Vector3(12, .08f, 12), ink, true);
            var seam = MaterialAsset("Floor lines", new Color(.10f, .15f, .23f));
            for (var i = -5; i <= 5; i++)
            {
                Box("Subtle floor line X " + i, environment, new Vector3(i, 0f, 1.5f), new Vector3(.007f, .002f, 12), seam);
                Box("Subtle floor line Z " + i, environment, new Vector3(0, 0f, i + 1.5f), new Vector3(12, .002f, .007f), seam);
            }
            // Sparse vertical accents leave the floating album readable without a solid wall.
            for (var i = 0; i < 9; i++)
            {
                var angle = (-80f + i * 20f) * Mathf.Deg2Rad;
                Box("Gallery light " + (i + 1), environment,
                    new Vector3(Mathf.Sin(angle) * 4.5f, 1.35f, Mathf.Cos(angle) * 4.5f),
                    new Vector3(.012f, 2.7f, .012f), seam);
            }
        }

        static void MakeGallery()
        {
            var gallery = story.contentRoot;
            story.mediaCards = new FloatingStoryItem[6]; story.photoButtons = new Button[6];
            story.albumPositions = new Vector3[6]; story.albumRotations = new Quaternion[6]; story.albumScales = new Vector3[6];
            for (var i = 0; i < 6; i++)
            {
                var angle = (i % 3 - 1) * 22f;
                var radians = angle * Mathf.Deg2Rad;
                var position = new Vector3(Mathf.Sin(radians) * 2.1f, 1.89f - (i / 3) * .87f, Mathf.Cos(radians) * 2.1f);
                var card = Empty("Floating photo " + (i + 1).ToString("00") + " - " + PhotoNames[i], gallery, position);
                card.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                var item = card.AddComponent<FloatingStoryItem>();
                item.controller = story; item.isPhoto = true; item.itemId = "photo_" + (i + 1).ToString("00");
                item.content = PhotoNames[i];
                var photo = LoadPhoto(i);
                var aspect = photo.width / (float)photo.height;
                var photoSize = new Vector2(.48f, .72f);
                if (aspect > photoSize.x / photoSize.y) photoSize.y = photoSize.x / aspect;
                else photoSize.x = photoSize.y * aspect;
                item.size = photoSize;
                var surface = MaterialAsset("Photo_" + PhotoNames[i], Color.white, photo);
                Box("Photo backing", card.transform, new Vector3(0f, 0f, .013f), new Vector3(photoSize.x + .025f, photoSize.y + .025f, .016f), panel);
                var photograph = Primitive("Original photograph", PrimitiveType.Quad, card.transform, new Vector3(0f, 0f, -.001f), new Vector3(photoSize.x, photoSize.y, 1f), surface);
                var video = card.AddComponent<MediaVideoSurface>(); video.surface = photograph.GetComponent<Renderer>();
                item.highlight = Frame(card.transform, photoSize + Vector2.one * .022f, gold);
                var collider = card.AddComponent<BoxCollider>(); collider.size = new Vector3(photoSize.x + .03f, photoSize.y + .03f, .026f);
                item.Grab.colliders.Clear(); item.Grab.colliders.Add(collider); item.ConfigureGrab();
                var face = Canvas("Photo selection - ray trigger", card.transform, new Vector3(0f, 0f, -.018f), photoSize * 1000f, false);
                story.photoButtons[i] = ActionButton(face.transform, "", Vector2.zero, photoSize * 1000f, 100 + i, new Color(1, 1, 1, 0));
                // NearFar can route both UI trigger and grip only when its closest
                // 2D and 3D hits identify the SAME GameObject (same2d3dHit).
                var faceCollider = story.photoButtons[i].gameObject.AddComponent<BoxCollider>();
                faceCollider.size = new Vector3(photoSize.x * 1000f, photoSize.y * 1000f, 2f);
                item.Grab.colliders.Add(faceCollider);
                Label("Memory name", card.transform, PhotoNames[i], new Vector3(0f, -photoSize.y / 2f - .052f, -.005f), new Vector2(500, 66), 35, Color.white);
                item.highlight.SetActive(false);
                story.mediaCards[i] = item;
                story.albumPositions[i] = card.transform.localPosition;
                story.albumRotations[i] = card.transform.localRotation;
                story.albumScales[i] = card.transform.localScale;
            }
        }

        static GameObject Frame(Transform parent, Vector2 size, Material material)
        {
            var frame = Empty("Yellow hover and selection outline", parent, Vector3.zero);
            const float width = .013f;
            Box("Left", frame.transform, new Vector3(-size.x / 2, 0, -.012f), new Vector3(width, size.y, .009f), material);
            Box("Right", frame.transform, new Vector3(size.x / 2, 0, -.012f), new Vector3(width, size.y, .009f), material);
            Box("Top", frame.transform, new Vector3(0, size.y / 2, -.012f), new Vector3(size.x, width, .009f), material);
            Box("Bottom", frame.transform, new Vector3(0, -size.y / 2, -.012f), new Vector3(size.x, width, .009f), material);
            return frame;
        }

        static void MakeGuidance()
        {
            var header = Canvas("Stage and contextual guidance", story.contentRoot, new Vector3(0, 2.46f, 2.04f), new Vector2(1650, 250), false);
            story.stageLabel = UIText(header.transform, "SPATIAL STORIES / CHOOSE A MEMORY", new Vector2(0, 65), new Vector2(1650, 75), 48, Mint);
            story.hintLabel = UIText(header.transform, "Point + trigger to select. Grip a photo to bring it closer.", new Vector2(0, -30), new Vector2(1650, 120), 33, Color.white);
            story.welcomePanel = Panel("Welcome - appears at runtime", new Vector3(0, 1.42f, .92f), new Vector2(860, 540));
            UIText(story.welcomePanel.transform, "SPATIAL STORIES", new Vector2(0, 190), new Vector2(760, 70), 45, Mint);
            UIText(story.welcomePanel.transform, "Your memories, within reach.", new Vector2(0, 115), new Vector2(760, 60), 32, Color.white);
            UIText(story.welcomePanel.transform,
                "Point + trigger: choose a photo or a tool\nHold grip: grab, move and rotate\nTwo grips: resize with both hands\nRelease into the left bin: delete; Undo restores",
                new Vector2(0, -5), new Vector2(780, 200), 27, Color.white);
            ActionButton(story.welcomePanel.transform, "BEGIN", new Vector2(0, -185), new Vector2(300, 65), 1, Blue);
            story.welcomePanel.SetActive(false);
        }

        static void MakeTools()
        {
            story.toolGroup = Empty("Floating T and S tool spheres", story.contentRoot, Vector3.zero);
            SphereTool("T - Text sphere", story.toolGroup.transform, new Vector3(.72f, 1.63f, 1.25f), .145f, "T", "TEXT", mint, 10);
            var stickers = SphereTool("S - Sticker sphere", story.toolGroup.transform, new Vector3(.92f, 1.26f, 1.18f), .145f, "S", "STICKERS", gold, 11);
            var preview = Shape("Sticker examples visible before use", 0, stickers.transform, new Vector3(-.085f, -.19f, -.06f), .045f);
            Shape("Heart preview", 1, stickers.transform, new Vector3(.045f, -.19f, -.06f), .045f);
            preview.name = "Visible star preview";
            story.publishObject = SphereTool("Blue physical publish sphere - unlocks after editing", story.contentRoot,
                new Vector3(.95f, .86f, 1.05f), .17f, "PUBLISH", "PREVIEW FIRST", blue, 5);
            story.publishObject.SetActive(false);
        }

        static GameObject SphereTool(string name, Transform parent, Vector3 position, float radius, string symbol, string caption, Material material, int action)
        {
            var go = Empty(name, parent, position);
            var cap = Primitive("Physical sphere", PrimitiveType.Sphere, go.transform, Vector3.zero, Vector3.one * radius * 2f, material);
            var collider = go.AddComponent<SphereCollider>(); collider.radius = radius;
            var xr = go.AddComponent<XRSimpleInteractable>(); xr.colliders.Clear(); xr.colliders.Add(collider);
            var spatial = go.AddComponent<SpatialToolButton>(); spatial.controller = story; spatial.action = action;
            spatial.cap = cap.transform;
            spatial.pressFace = Empty("Physical press plane", go.transform, new Vector3(0, 0, -radius)).transform;
            spatial.pressRadius = radius * .9f;
            var presses = PhysicalControllerInteractors();
            spatial.pressInteractors = presses;
            spatial.pressTips = new Transform[presses.Length];
            for (var i = 0; i < presses.Length; i++) spatial.pressTips[i] = presses[i].transform;
            spatial.highlight = Frame(go.transform, Vector2.one * radius * 2.1f, white);
            spatial.highlight.transform.localPosition = new Vector3(0f, 0f, -radius);
            spatial.highlight.SetActive(false);
            var canvas = Canvas("Trigger and physical poke face", go.transform, new Vector3(0f, 0f, -radius - .01f), Vector2.one * radius * 2000f, false);
            var button = PlainButton(canvas.transform, symbol, Vector2.zero, Vector2.one * radius * 2000f, new Color(1, 1, 1, 0), symbol.Length > 2 ? 36 : 105);
            var faceCollider = button.gameObject.AddComponent<BoxCollider>();
            faceCollider.size = new Vector3(radius * 2000f, radius * 2000f, 2f);
            xr.colliders.Add(faceCollider);
            UnityEventTools.AddPersistentListener(button.onClick, spatial.Press);
            if (symbol.Length == 1) button.GetComponentInChildren<Text>().color = Ink;
            Label("Tool name", go.transform, caption, new Vector3(0, radius + .065f, -.05f), new Vector2(460, 80), 33, Color.white);
            return go;
        }

        static XRBaseInteractor[] PhysicalControllerInteractors()
        {
            var result = new List<XRBaseInteractor>();
            var hands = new HashSet<InteractorHandedness>();
            foreach (var root in story.gameObject.scene.GetRootGameObjects())
                foreach (var interactor in root.GetComponentsInChildren<NearFarInteractor>(true))
                {
                    if (interactor.handedness == InteractorHandedness.None || !hands.Add(interactor.handedness)) continue;
                    // A NearFar attachTransform is a moving far-grab anchor. Its own fixed
                    // controller transform is the physical tip; existing XR rig stays unchanged.
                    result.Add(interactor);
                }
            return result.ToArray();
        }

        static void MakeBin()
        {
            var bin = Empty("LEFT TRASH BIN - open spatial drop target", story.contentRoot, new Vector3(-.82f, .73f, 1.03f));
            var glow = new List<Renderer>();
            var neutral = MaterialAsset("Bin idle", new Color(.28f, .36f, .43f));
            story.trashIdleMaterial = neutral; story.trashActiveMaterial = red;
            Box("Bin base", bin.transform, new Vector3(0, -.12f, 0), new Vector3(.42f, .025f, .38f), ink);
            glow.Add(Box("Left side", bin.transform, new Vector3(-.20f, 0, 0), new Vector3(.025f, .25f, .38f), neutral).GetComponent<Renderer>());
            glow.Add(Box("Right side", bin.transform, new Vector3(.20f, 0, 0), new Vector3(.025f, .25f, .38f), neutral).GetComponent<Renderer>());
            glow.Add(Box("Back", bin.transform, new Vector3(0, 0, .178f), new Vector3(.40f, .25f, .025f), neutral).GetComponent<Renderer>());
            glow.Add(Box("Front", bin.transform, new Vector3(0, -.035f, -.178f), new Vector3(.40f, .18f, .025f), neutral).GetComponent<Renderer>());
            var drop = Empty("Deletion volume - release or throw inside", bin.transform, new Vector3(0, .045f, 0));
            var volume = drop.AddComponent<BoxCollider>(); volume.isTrigger = true; volume.size = new Vector3(.39f, .29f, .35f);
            story.trashVolume = volume; story.trashGlow = glow.ToArray();
            Label("Bin label", bin.transform, "TRASH", new Vector3(0, -.025f, -.20f), new Vector2(330, 70), 42, Color.white);
            Label("Bin hint", bin.transform, "Release inside to delete", new Vector3(0, .22f, -.02f), new Vector2(590, 80), 29, Color.white);
            story.undoObject = Canvas("Undo deletion", bin.transform, new Vector3(0, -.26f, -.01f), new Vector2(430, 95), true).gameObject;
            ActionButton(story.undoObject.transform, "UNDO DELETE", Vector2.zero, new Vector2(420, 80), 3, PanelColor);
            story.undoLabel = UIText(story.undoObject.transform, "", new Vector2(0, -95), new Vector2(620, 100), 25, Mint);
            story.undoObject.SetActive(false);
        }

        static void MakeNavigation()
        {
            story.navigation = Canvas("Photo control and freedom", story.contentRoot, new Vector3(0, .55f, 1.13f), new Vector2(960, 130), true).gameObject;
            ActionButton(story.navigation.transform, "BACK TO ALBUM", new Vector2(-230, 0), new Vector2(400, 85), 2, PanelColor);
            ActionButton(story.navigation.transform, "DESELECT", new Vector2(230, 0), new Vector2(400, 85), 4, PanelColor);
            var session = Canvas("Session control", story.contentRoot, new Vector3(-1.07f, .38f, 1.15f), new Vector2(250, 85), true);
            ActionButton(session.transform, "SESSION", Vector2.zero, new Vector2(250, 80), 21, PanelColor);
        }

        static void MakeTextPanel()
        {
            story.textPanel = Panel("TEXT - editable caption and spatial keyboard", new Vector3(.07f, 1.28f, .73f), new Vector2(1140, 1070));
            var t = story.textPanel.transform;
            UIText(t, "TEXT / WRITE YOUR STORY", new Vector2(-50, 480), new Vector2(950, 65), 38, Mint);
            ActionButton(t, "X", new Vector2(505, 480), new Vector2(70, 60), 9, PanelColor);
            story.draftLabel = UIText(t, "Your caption", new Vector2(0, 370), new Vector2(1040, 130), 42, Color.white);
            var presets = new[] { "Weekend", "Good day", "Wish you were here", "Hello from..." };
            for (var i = 0; i < 4; i++)
                ActionButton(t, presets[i], new Vector2(-402 + i * 268, 260), new Vector2(255, 65), 300 + i, PanelColor, 25);
            var rows = new[] { "QWERTYUIOP", "ASDFGHJKL", "ZXCVBNM", "0123456789", ".,!?'-" };
            for (var row = 0; row < rows.Length; row++)
            {
                var line = rows[row];
                for (var i = 0; i < line.Length; i++)
                    ActionButton(t, line[i].ToString(), new Vector2((i - (line.Length - 1) * .5f) * 99f, 150 - row * 83),
                        new Vector2(89, 70), 1000 + Alphabet.IndexOf(line[i]), PanelColor, 32);
            }
            ActionButton(t, "Aa", new Vector2(-420, -255), new Vector2(125, 60), 16, PanelColor);
            ActionButton(t, "SPACE", new Vector2(-90, -255), new Vector2(500, 60), 17, PanelColor);
            ActionButton(t, "DELETE", new Vector2(345, -255), new Vector2(320, 60), 14, PanelColor);
            var colors = new[] { Color.white, Gold, Mint, Pink, Blue, Color.black };
            for (var i = 0; i < colors.Length; i++)
                ActionButton(t, "", new Vector2(-475 + i * 100, -345), new Vector2(70, 65), 400 + i, colors[i]);
            ActionButton(t, "BOLD", new Vector2(245, -345), new Vector2(190, 65), 500, PanelColor);
            ActionButton(t, "CLEAR", new Vector2(450, -345), new Vector2(170, 65), 15, PanelColor);
            ActionButton(t, "NEW CAPTION", new Vector2(-285, -450), new Vector2(420, 70), 12, PanelColor);
            ActionButton(t, "ADD / APPLY", new Vector2(240, -450), new Vector2(490, 70), 13, Blue);
            story.textPanel.SetActive(false);
        }

        static void MakeStickerPanel()
        {
            story.stickerPanel = Panel("STICKERS - six tangible previews", new Vector3(.18f, 1.38f, .83f), new Vector2(960, 730));
            var t = story.stickerPanel.transform;
            UIText(t, "STICKERS / PICK A SHAPE", new Vector2(-30, 305), new Vector2(800, 60), 38, Gold);
            ActionButton(t, "X", new Vector2(420, 305), new Vector2(70, 60), 9, PanelColor);
            for (var i = 0; i < 6; i++)
            {
                var pos = new Vector2((i % 3 - 1) * 290, 125 - i / 3 * 270);
                ActionButton(t, "", pos, new Vector2(260, 235), 200 + i, PanelColor);
                // Children of this world-space canvas use pixel units; create previews in world metres on a separate anchor.
                var anchor = Empty("Tangible " + ShapeNames[i] + " preview", t, new Vector3(pos.x, pos.y + 20f, -15f));
                anchor.transform.localScale = Vector3.one * 1000f;
                Shape(ShapeNames[i], i, anchor.transform, Vector3.zero, .078f);
                UIText(t, ShapeNames[i].ToUpperInvariant(), new Vector2(pos.x, pos.y - 82), new Vector2(250, 55), 28, Color.white);
            }
            story.stickerPanel.SetActive(false);
        }

        static void MakeReviewAndSessionPanels()
        {
            story.reviewPanel = Panel("Preview and publish confirmation", new Vector3(.83f, 1.35f, .84f), new Vector2(640, 820));
            var r = story.reviewPanel.transform;
            UIText(r, "REVIEW YOUR STORY", new Vector2(0, 345), new Vector2(600, 70), 36, Mint);
            story.reviewLabel = UIText(r, "Check your photo and overlays before publishing.", new Vector2(0, 165), new Vector2(560, 250), 28, Color.white);
            UIText(r, "AUDIENCE", new Vector2(0, -10), new Vector2(550, 50), 28, Mint);
            ActionButton(r, "FRIENDS", new Vector2(-150, -90), new Vector2(270, 65), 18, PanelColor);
            ActionButton(r, "ONLY ME", new Vector2(150, -90), new Vector2(270, 65), 19, PanelColor);
            ActionButton(r, "PUBLISH STORY", new Vector2(0, -205), new Vector2(540, 80), 6, Blue);
            ActionButton(r, "BACK TO EDIT", new Vector2(0, -320), new Vector2(540, 65), 7, PanelColor);
            story.reviewPanel.SetActive(false);
            story.successPanel = Panel("Published successfully", new Vector3(0, 1.45f, .88f), new Vector2(850, 530));
            UIText(story.successPanel.transform, "STORY PUBLISHED", new Vector2(0, 150), new Vector2(780, 80), 46, Mint);
            UIText(story.successPanel.transform, "Prototype confirmation\nYour Story stays inside this test application.", new Vector2(0, 15), new Vector2(780, 160), 30, Color.white);
            ActionButton(story.successPanel.transform, "CREATE ANOTHER STORY", new Vector2(0, -160), new Vector2(620, 75), 8, Blue);
            story.successPanel.SetActive(false);
            story.adminPanel = Panel("Facilitator session tools", new Vector3(0, 1.32f, .70f), new Vector2(980, 840));
            var a = story.adminPanel.transform;
            UIText(a, "SESSION CONTROLS", new Vector2(0, 350), new Vector2(840, 70), 38, Mint);
            story.sessionLabel = UIText(a, "Session data appears during Play.", new Vector2(0, 160), new Vector2(870, 280), 27, Color.white);
            ActionButton(a, "RECORD FACILITATOR PROMPT", new Vector2(0, -40), new Vector2(860, 70), 20, PanelColor);
            ActionButton(a, "RECENTER WORKSPACE", new Vector2(0, -135), new Vector2(860, 70), 23, PanelColor);
            ActionButton(a, "END AS INCOMPLETE", new Vector2(0, -230), new Vector2(860, 70), 22, PanelColor);
            ActionButton(a, "CLOSE", new Vector2(-220, -335), new Vector2(410, 65), 21, PanelColor);
            ActionButton(a, "RESET SESSION", new Vector2(220, -335), new Vector2(410, 65), 8, Blue);
            story.adminPanel.SetActive(false);
        }

        static FloatingStoryItem TextTemplate()
        {
            var path = Generated + "/Prefabs/EditableText.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing) return existing.GetComponent<FloatingStoryItem>();
            var go = new GameObject("Editable spatial caption");
            try
            {
                var item = go.AddComponent<FloatingStoryItem>();
                item.itemId = "caption"; item.isText = true; item.content = "Your story";
                item.size = new Vector2(.38f, .16f); item.textColor = Color.white;
                item.label = Label("Editable caption", go.transform, item.content, new Vector3(0, 0, -.014f), new Vector2(500, 210), 48, Color.white);
                item.label.resizeTextForBestFit = true; item.label.resizeTextMinSize = 22; item.label.resizeTextMaxSize = 48;
                item.label.GetComponentInParent<Canvas>().worldCamera = null;
                item.label.transform.parent.localScale = Vector3.one * .00076f;
                item.highlight = Frame(go.transform, item.size + Vector2.one * .018f, gold);
                item.highlight.SetActive(false);
                var box = go.AddComponent<BoxCollider>(); box.size = new Vector3(.40f, .18f, .035f);
                item.Grab.colliders.Clear(); item.Grab.colliders.Add(box); item.ConfigureGrab();
                return PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<FloatingStoryItem>();
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        static FloatingStoryItem StickerTemplate(int index)
        {
            var path = Generated + "/Prefabs/Sticker_" + ShapeNames[index] + ".prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing) return existing.GetComponent<FloatingStoryItem>();
            var go = new GameObject("Spatial sticker - " + ShapeNames[index]);
            try
            {
                var item = go.AddComponent<FloatingStoryItem>(); item.itemId = "sticker_" + index;
                item.stickerIndex = index; item.content = ShapeNames[index]; item.size = new Vector2(.23f, .23f);
                Shape(ShapeNames[index], index, go.transform, Vector3.zero, .10f);
                item.highlight = Frame(go.transform, new Vector2(.245f, .245f), gold); item.highlight.SetActive(false);
                var box = go.AddComponent<BoxCollider>(); box.size = new Vector3(.25f, .25f, .045f);
                item.Grab.colliders.Clear(); item.Grab.colliders.Add(box); item.ConfigureGrab();
                return PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<FloatingStoryItem>();
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        static GameObject Shape(string name, int index, Transform parent, Vector3 position, float radius)
        {
            var go = Empty(name, parent, position);
            switch (index)
            {
                case 0: Polygon(go.transform, "Five point star", StarPoints(5, .46f), radius, gold); break;
                case 1:
                    var heart = new Vector2[48];
                    for (var i = 0; i < heart.Length; i++)
                    {
                        var t = i * Mathf.PI * 2 / heart.Length;
                        heart[i] = new Vector2(16f * Mathf.Pow(Mathf.Sin(t), 3), 13 * Mathf.Cos(t) - 5 * Mathf.Cos(2 * t) - 2 * Mathf.Cos(3 * t) - Mathf.Cos(4 * t)) / 18f;
                    }
                    Polygon(go.transform, "Heart", heart, radius, pink); break;
                case 2:
                    Polygon(go.transform, "Smile face", CirclePoints(40), radius, gold);
                    Primitive("Left eye", PrimitiveType.Sphere, go.transform, new Vector3(-radius * .34f, radius * .22f, -.013f), Vector3.one * radius * .14f, ink);
                    Primitive("Right eye", PrimitiveType.Sphere, go.transform, new Vector3(radius * .34f, radius * .22f, -.013f), Vector3.one * radius * .14f, ink);
                    for (var i = 0; i < 7; i++)
                    {
                        var x = (i - 3) * .13f;
                        Box("Smile curve " + i, go.transform, new Vector3(x * radius, (-.38f + x * x * .9f) * radius, -.012f), new Vector3(radius * .14f, radius * .07f, .009f), ink);
                    }
                    break;
                case 3: Polygon(go.transform, "Four point sparkle", StarPoints(4, .27f), radius, mint); break;
                case 4:
                    var pin = Empty("Location pin", go.transform, Vector3.zero);
                    Polygon(pin.transform, "Pin body", new[] { new Vector2(0, -1), new Vector2(-.65f, .05f), new Vector2(-.65f, .65f), new Vector2(0, 1), new Vector2(.65f, .65f), new Vector2(.65f, .05f) }, radius, blue);
                    var hole = Empty("Pin centre", go.transform, new Vector3(0, radius * .40f, -.014f));
                    Polygon(hole.transform, "White centre", CirclePoints(24), radius * .24f, white); break;
                default:
                    Box("Music stem", go.transform, new Vector3(radius * .2f, radius * .08f, 0), new Vector3(radius * .14f, radius * 1.5f, .025f), mint);
                    Box("Music flag", go.transform, new Vector3(radius * .46f, radius * .76f, 0), new Vector3(radius * .65f, radius * .17f, .025f), mint);
                    var note = Empty("Music note", go.transform, new Vector3(-radius * .16f, -radius * .63f, -.006f));
                    Polygon(note.transform, "Note head", CirclePoints(30), radius * .43f, mint); break;
            }
            return go;
        }

        static Vector2[] StarPoints(int count, float inner)
        {
            var points = new Vector2[count * 2];
            for (var i = 0; i < points.Length; i++)
            {
                var angle = Mathf.PI / 2 + i * Mathf.PI / count;
                points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (i % 2 == 0 ? 1 : inner);
            }
            return points;
        }

        static Vector2[] CirclePoints(int count)
        {
            var points = new Vector2[count];
            for (var i = 0; i < count; i++) points[i] = new Vector2(Mathf.Cos(i * Mathf.PI * 2 / count), Mathf.Sin(i * Mathf.PI * 2 / count));
            return points;
        }

        static void Polygon(Transform parent, string name, Vector2[] points, float radius, Material material)
        {
            var path = Generated + "/Meshes/" + name.Replace(' ', '_') + ".asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (!mesh)
            {
                var vertices = new Vector3[points.Length + 1];
                var uv = new Vector2[vertices.Length];
                var triangles = new List<int>(); uv[0] = Vector2.one * .5f;
                for (var i = 0; i < points.Length; i++) { vertices[i + 1] = points[i]; uv[i + 1] = points[i] * .5f + Vector2.one * .5f; }
                // Both faces are saved to keep overlays visible during embodied inspection.
                for (var i = 0; i < points.Length; i++)
                {
                    var next = (i + 1) % points.Length + 1;
                    triangles.Add(0); triangles.Add(next); triangles.Add(i + 1);
                    triangles.Add(0); triangles.Add(i + 1); triangles.Add(next);
                }
                mesh = new Mesh { name = name, vertices = vertices, uv = uv, triangles = triangles.ToArray() };
                mesh.RecalculateNormals(); mesh.RecalculateBounds(); AssetDatabase.CreateAsset(mesh, path);
            }
            var go = Empty(name, parent, Vector3.zero); go.transform.localScale = Vector3.one * radius;
            go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        static Canvas Canvas(string name, Transform parent, Vector3 position, Vector2 size, bool raycast)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = Vector3.one * .001f;
            var rect = (RectTransform)go.transform; rect.sizeDelta = size;
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = story ? story.head : null;
            go.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 10;
            if (raycast)
            {
                var raycaster = go.AddComponent<TrackedDeviceGraphicRaycaster>();
                raycaster.checkFor3DOcclusion = false;
            }
            return canvas;
        }

        static GameObject Panel(string name, Vector3 position, Vector2 size)
        {
            var canvas = Canvas(name, story.contentRoot, position, size, true);
            var back = new GameObject("Panel surface", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            back.transform.SetParent(canvas.transform, false); var rect = (RectTransform)back.transform; rect.sizeDelta = size;
            back.GetComponent<Image>().color = new Color(PanelColor.r, PanelColor.g, PanelColor.b, .98f);
            back.GetComponent<Image>().raycastTarget = false;
            return canvas.gameObject;
        }

        static Text Label(string name, Transform parent, string text, Vector3 position, Vector2 size, int fontSize, Color color)
        {
            var canvas = Canvas(name, parent, position, size, false);
            return UIText(canvas.transform, text, Vector2.zero, size, fontSize, color);
        }

        static Text UIText(Transform parent, string value, Vector2 pos, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject("Label - " + (value.Length > 28 ? value.Substring(0, 28) : value), typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform;
            rect.anchoredPosition = pos; rect.sizeDelta = size;
            var text = go.GetComponent<Text>(); text.font = font; text.text = value;
            text.fontSize = fontSize; text.color = color; text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate; text.raycastTarget = false;
            return text;
        }

        static Button PlainButton(Transform parent, string caption, Vector2 pos, Vector2 size, Color color, int fontSize)
        {
            // Each actionable surface has its own registered canvas raycaster, including transparent photo/tool faces.
            var rootCanvas = parent.GetComponentInParent<Canvas>();
            if (rootCanvas)
            {
                var raycaster = rootCanvas.GetComponent<TrackedDeviceGraphicRaycaster>();
                if (!raycaster) raycaster = rootCanvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
                raycaster.checkFor3DOcclusion = false;
            }
            var go = new GameObject("Button - " + caption, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false); var rect = (RectTransform)go.transform; rect.anchoredPosition = pos; rect.sizeDelta = size;
            var image = go.GetComponent<Image>(); image.color = color; image.raycastTarget = true;
            var button = go.GetComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.normalColor = Color.white; colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1);
            colors.selectedColor = Color.white; colors.pressedColor = new Color(.72f, .86f, 1, 1); colors.fadeDuration = .08f; button.colors = colors;
            if (!string.IsNullOrEmpty(caption)) UIText(go.transform, caption, Vector2.zero, size - new Vector2(12, 6), fontSize, Color.white);
            return button;
        }

        static Button ActionButton(Transform parent, string caption, Vector2 pos, Vector2 size, int action, Color color, int fontSize = 30)
        {
            var button = PlainButton(parent, caption, pos, size, color, fontSize);
            UnityEventTools.AddIntPersistentListener(button.onClick, story.Act, action);
            return button;
        }
    }

    [CustomEditor(typeof(ImmersiveStory))]
    public sealed class ImmersiveStoryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var controller = (ImmersiveStory)target;
            EditorGUILayout.HelpBox("Full floating concept: photo wall, T/S spheres, spatial bin and publish control. " +
                "Use these preview buttons outside Play. Runtime starts from the welcome screen.", MessageType.Info);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button("Show full floating album / 悬浮相册")) Preview(controller, 0);
                if (GUILayout.Button("Show close editing / 放大编辑")) Preview(controller, 1);
                if (GUILayout.Button("Show text tools / 文字工具")) Preview(controller, 2);
                if (GUILayout.Button("Show sticker choices / 贴纸选择")) Preview(controller, 3);
            }
            using (new EditorGUI.DisabledScope(Application.isPlaying || controller.editorPreviewMode != 0))
                if (GUILayout.Button("Capture current album positions / 保存相册排列")) CaptureAlbum(controller);
            EditorGUILayout.HelpBox("To customise the photo wall: show the floating album, move its photo objects, " +
                "then capture the album positions and save the scene. Capture is disabled in close editing/tool previews.", MessageType.None);
            if (GUILayout.Button("Frame full workspace / 聚焦场景")) ImmersiveSceneBuilder.SelectAndFrame();
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }

        static void CaptureAlbum(ImmersiveStory controller)
        {
            if (Application.isPlaying || controller.editorPreviewMode != 0 || !controller.contentRoot || controller.mediaCards == null) return;
            var count = controller.mediaCards.Length;
            var positions = new Vector3[count]; var rotations = new Quaternion[count]; var scales = new Vector3[count];
            var parentScale = controller.contentRoot.lossyScale;
            if (Mathf.Abs(parentScale.x) < .0001f || Mathf.Abs(parentScale.y) < .0001f || Mathf.Abs(parentScale.z) < .0001f)
            {
                EditorUtility.DisplayDialog("Cannot capture zero scale", "Restore a nonzero scale on the spatial interface before capturing the album.", "OK");
                return;
            }
            for (var i = 0; i < count; i++)
            {
                var card = controller.mediaCards[i];
                if (!card)
                {
                    EditorUtility.DisplayDialog("Missing photo reference", "A photo is missing from Media Cards. Restore that reference before capturing the album.", "OK");
                    return;
                }
                positions[i] = controller.contentRoot.InverseTransformPoint(card.transform.position);
                rotations[i] = Quaternion.Inverse(controller.contentRoot.rotation) * card.transform.rotation;
                var scale = card.transform.lossyScale;
                scales[i] = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            }
            Undo.RecordObject(controller, "Capture album arrangement");
            controller.albumPositions = positions; controller.albumRotations = rotations; controller.albumScales = scales;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Debug.Log("Captured " + count + " album photo positions. Save this scene to retain them.", controller);
        }

        static void Preview(ImmersiveStory controller, int stage)
        {
            Undo.RegisterFullObjectHierarchyUndo(controller.gameObject, "Preview spatial layout");
            controller.ShowDesign(stage);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            SceneView.RepaintAll();
        }
    }
}
