using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SpatialStories.Immersive
{
    public sealed partial class ImmersiveStory
    {
        [Tooltip("Enabled only in the new IP2b scene copies.")]
        public bool enableCollage;
        readonly List<FloatingStoryItem> sourceHandles = new List<FloatingStoryItem>();
        readonly List<Material> collageMaterials = new List<Material>();
        readonly Dictionary<Button, int> collageButtons = new Dictionary<Button, int>();
        FloatingStoryItem activeLayer;
        GameObject sourcePanel, layerPanel, flatPreview;
        Text layerLabel;
        Shader collageShader, photoShader;
        Material textDepthMaterial, outlineMaterial;
        bool sourcesOpen, flatPreviewReady;
        RenderTexture previewTexture;
        Camera previewCamera;
        Renderer previewRenderer;

        sealed class Subject
        {
            public int photoIndex;
            public string resource, title;
            public Rect detail, subject, textureUV;
            public Vector3 position;
            public Vector2 cardSize;
        }
        Subject[] subjects;

        void InitializeCollage()
        {
            if (!enableCollage) return;
            collageShader = Resources.Load<Shader>("CollageDepth");
            photoShader = Resources.Load<Shader>("ImmersiveUnlit");
            if (!collageShader || !photoShader)
            {
                Debug.LogError("IP2b shaders are missing from Runtime/Resources.", this);
                enabled = false;
                return;
            }
            textDepthMaterial = NewMaterial(collageShader, Color.white);
            textDepthMaterial.SetFloat("_Cutoff", .15f);
            outlineMaterial = NewMaterial(photoShader, new Color(.35f, 1f, .75f));
            foreach (var card in mediaCards) if (card) card.EnableStableTwoHandScaling();
            subjects = new[]
            {
                new Subject { photoIndex=2, resource="IP2b/Surfer", title="PIHA / SURFER",
                    detail=new Rect(.463f,.365f,.17f,.12f), subject=new Rect(.498f,.389f,.115f,.069f),
                    textureUV=new Rect(.21484375f,.07421875f,.595703125f,.875976563f),
                    position=new Vector3(-.67f,1.53f,1.04f) },
                new Subject { photoIndex=4, resource="IP2b/Eiffel", title="PARIS / TOWER",
                    detail=new Rect(.27f,.21f,.50f,.69f), subject=new Rect(.321f,.237f,.388f,.633f),
                    textureUV=new Rect(.1484375f,.009765625f,.704101563f,.979817708f),
                    position=new Vector3(-1.03f,1.53f,1.04f) }
            };
            if (stickerPanel)
                CollageButton(stickerPanel.transform, "FROM PHOTO", new Vector2(0,-310), new Vector2(620,64), 600);
            sourcePanel = new GameObject("IP2b - floating source photographs");
            sourcePanel.transform.SetParent(contentRoot, false);
            var heading = CollageCanvas("Source guidance", sourcePanel.transform, new Vector3(-.85f,1.91f,1.03f), new Vector2(680,150));
            CollageText(heading.transform, "FROM PHOTO\nGrip a highlighted subject and move it onto your Story.",
                Vector2.zero, new Vector2(660,140), 25);
            foreach (var subject in subjects)
            {
                var original = mediaCards[subject.photoIndex].transform.Find("Original photograph");
                var texture = original ? original.GetComponent<Renderer>().sharedMaterial.mainTexture : null;
                if (!texture) { Debug.LogError("Missing source photograph: " + subject.title); continue; }
                subject.cardSize = new Vector2(.42f * texture.width / texture.height * subject.detail.width / subject.detail.height, .42f);
                var material = NewMaterial(photoShader, Color.white, texture);
                material.mainTextureScale = subject.detail.size;
                material.mainTextureOffset = subject.detail.position;
                Quad(subject.title + " - detail", sourcePanel.transform, subject.position, subject.cardSize, material);
                var caption = CollageCanvas(subject.title, sourcePanel.transform, subject.position + new Vector3(0,-.27f,0), new Vector2(350,70));
                CollageText(caption.transform, subject.title + "\nSubject detail", Vector2.zero, new Vector2(340,70), 22);
            }
            sourcePanel.SetActive(false);
            // Between the focused photo and navigation, away from the right-hand Publish sphere.
            layerPanel = CollageCanvas("IP2b - layer controls", contentRoot, new Vector3(0,.83f,1.05f), new Vector2(720,220));
            var background = layerPanel.AddComponent<Image>(); background.color = new Color(.035f,.065f,.10f,.96f); background.raycastTarget = false;
            layerLabel = CollageText(layerPanel.transform, "LAYERS\nPlace an element first", new Vector2(0,58), new Vector2(690,80), 23);
            AddLayerIcon(CollageButton(layerPanel.transform, "BACKWARD", new Vector2(-235,-45), new Vector2(215,62), 601), false);
            AddLayerIcon(CollageButton(layerPanel.transform, "FORWARD", new Vector2(235,-45), new Vector2(215,62), 602), true);
            CollageButton(layerPanel.transform, "NEXT LAYER", new Vector2(0,-45), new Vector2(225,62), 603);
            layerPanel.SetActive(false);
            // Retry is only enabled after an unsuccessful capture.
            if (reviewPanel) CollageButton(reviewPanel.transform, "RETRY PREVIEW", new Vector2(0,-380), new Vector2(540,40), 604);
            InitializeCutoutDiscovery();
            InitializePlacementFeedback();
        }

        bool CollageActionAllowed(int action)
        {
            if (!enableCollage || !collageShader) return false;
            if (action == 604) return stage == Stage.Review && !flatPreviewReady;
            if (stage != Stage.Edit || !selected) return false;
            if (action == 600) return stickerPanel && stickerPanel.activeSelf;
            var layers = Layers(selected);
            if (action == 603) return layers.Count > 0;
            if (!activeLayer || activeLayer.deleted || !activeLayer.placed || !IsOnSelected(activeLayer)) return false;
            int index = layers.IndexOf(activeLayer);
            return action == 601 ? index > 0 : action == 602 && index >= 0 && index < layers.Count - 1;
        }

        void CollageAction(int action)
        {
            switch (action)
            {
                case 600:
                    ClosePanels(); sourcesOpen = true; Set(sourcePanel, true); EnsureSourceHandles(); Refresh();
                    Hint("Grip the outlined surfer or tower in a source photo.\nMove it onto your Story and release."); Record("open_photo_sources"); break;
                case 601: MoveLayer(-1); break;
                case 602: MoveLayer(1); break;
                case 603:
                    var layers = Layers(selected);
                    if (layers.Count > 0) SetActiveLayer(layers[(layers.IndexOf(activeLayer)+1) % layers.Count]);
                    RefreshCollage(); break;
                case 604: CaptureFlatPreview(); break;
            }
        }

        void EnsureSourceHandles()
        {
            if (!enableCollage || !sourcesOpen || subjects == null) return;
            foreach (var subject in subjects)
            {
                var source = mediaCards[subject.photoIndex];
                if (!source || sourceHandles.Any(i => i && !i.isDiscoveryHandle && i.sourcePhotoId == source.itemId)) continue;
                var center = subject.subject.center;
                var offset = new Vector2((center.x-subject.detail.center.x)/subject.detail.width*subject.cardSize.x,
                    (center.y-subject.detail.center.y)/subject.detail.height*subject.cardSize.y);
                float h = subject.subject.height / subject.detail.height * subject.cardSize.y;
                CreateSourceHandle(subject, subject.position + new Vector3(offset.x,offset.y,-.026f), h, false);
            }
        }

        FloatingStoryItem CreateSourceHandle(Subject subject, Vector3 position, float height, bool discovery)
        {
            var texture = Resources.Load<Texture2D>(subject.resource);
            if (!texture) { Hint("A prepared subject is missing. Check the IP2b Resources folder."); return null; }
            var go = new GameObject((discovery ? "Direct cutout - " : "Lift subject - ") + subject.title, typeof(RectTransform));
            go.SetActive(false); go.transform.SetParent(contentRoot, false); go.transform.localPosition=position;
            var dimensions = SubjectDimensions(subject, texture, height);
            Quad("Prepared transparent subject", go.transform, Vector3.zero, dimensions, SubjectMaterial(subject,texture));
            var item = go.AddComponent<FloatingStoryItem>();
            item.controller=this; item.isCutout=true; item.isSourceHandle=true; item.isDiscoveryHandle=discovery;
            item.sourcePhotoId=mediaCards[subject.photoIndex].itemId; item.content=subject.title; item.size=dimensions;
            item.itemId=(discovery ? "direct_" : "source_")+item.sourcePhotoId;
            var collider=go.AddComponent<BoxCollider>(); collider.size=new Vector3(dimensions.x,dimensions.y,.022f);
            item.Grab.colliders.Clear(); item.Grab.colliders.Add(collider);
            EnsureCutoutClickFace(item);
            item.highlight=CollageFrame(go.transform,dimensions); item.persistentHighlight=true;
            sourceHandles.Add(item);
            go.SetActive(true); item.ConfigureGrab(); item.EnableStableTwoHandScaling(); item.SavePose();
            return item;
        }

        static Vector2 SubjectDimensions(Subject subject, Texture texture, float height) =>
            new Vector2(height*texture.width*subject.textureUV.width/(texture.height*subject.textureUV.height),height);

        Material SubjectMaterial(Subject subject, Texture texture)
        {
            var material=NewMaterial(collageShader,Color.white,texture);
            material.mainTextureScale=subject.textureUV.size; material.mainTextureOffset=subject.textureUV.position;
            return material;
        }

        bool LiftSubject(FloatingStoryItem item)
        {
            if (!selected || overlays.Any(i => i && !i.deleted && IsOnSelected(i) && !i.placed) ||
                overlays.Count(i => i && !i.deleted && IsOnSelected(i)) >= maxOverlaysPerPhoto)
            {
                if (item.Grab.interactionManager) item.Grab.interactionManager.CancelInteractableSelection((IXRSelectInteractable)item.Grab);
                Hint("Place or remove the floating element before taking another subject.");
                return false;
            }
            sourceHandles.Remove(item);
            bool fromDiscovery=item.isDiscoveryHandle;
            if (fromDiscovery) discoveryHandle=null;
            item.isDiscoveryHandle=false;
            item.isSourceHandle = false; item.justLifted = true; item.placed = false;
            item.itemId = "cutout_" + (++serial); item.ownerPhoto = selected;
            item.collageLayer = NextLayer(selected);
            overlays.Add(item);
            // Keep the same parent until XRI has completed detach. The photo remains intact.
            Record("lift_photo_subject", item.itemId + " | source=" + item.sourcePhotoId);
            Record("cutout_entry_used", (fromDiscovery ? "direct_beside_s" : "source_menu") + " | " + item.itemId);
            return true;
        }

        void ReturnUnusedSubject(FloatingStoryItem item, string reason)
        {
            Record(reason, item.itemId + " | source=" + item.sourcePhotoId);
            overlays.Remove(item); sourceHandles.Remove(item);
            if (activeLayer == item) activeLayer = null;
            ForgetItem(item);
            item.gameObject.SetActive(false); Destroy(item.gameObject);
            EnsureSourceHandles(); Refresh();
            Hint("The subject returned to its source.\nGrip it again and release over a photo.");
        }

        void PrepareCollageOverlay(FloatingStoryItem item)
        {
            item.ownerPhoto = selected; item.collageLayer = NextLayer(selected);
            item.EnableStableTwoHandScaling();
            // Existing stickers contain shallow 3D decoration. Compress only their visual children
            // so each logical layer occupies a narrow depth band; root colliders remain usable.
            foreach (Transform child in item.transform)
            {
                var p = child.localPosition; p.z *= .04f; child.localPosition = p;
                var scale = child.localScale; scale.z *= .04f; child.localScale = scale;
            }
            if (item.label)
            {
                item.label.material = textDepthMaterial;
                var canvas = item.label.GetComponentInParent<Canvas>();
                if (canvas) canvas.worldCamera = head;
            }
            SetActiveLayer(item);
        }

        // Shared by the visual prediction and the actual drop. No pose or ownership changes here.
        bool TryFindCollagePlacement(FloatingStoryItem item, out FloatingStoryItem target,
            out CollagePlacement.Placement result)
        {
            target = null;
            result = default;
            if (!item || item.isPhoto || item.deleted || !item.gameObject.activeInHierarchy ||
                InsideBin(item.transform.position) || mediaCards == null) return false;
            float best = float.MaxValue;
            foreach (var photo in mediaCards)
            {
                if (!CollagePlacement.TryProject(item, photo, out var placement, out var score)) continue;
                int count = 0;
                foreach (var overlay in overlays)
                    if (overlay && overlay != item && !overlay.deleted && overlay.ownerPhoto == photo) count++;
                if (count >= maxOverlaysPerPhoto) continue;
                if (score < best) { best = score; target = photo; result = placement; }
            }
            if (!target) return false;
            int depthIndex = 0;
            foreach (var overlay in overlays)
                if (overlay && overlay != item && !overlay.deleted && overlay.placed && overlay.ownerPhoto == target &&
                    (item.ownerPhoto != target || overlay.collageLayer < item.collageLayer)) depthIndex++;
            result.position.z = LayerDepth(depthIndex);
            return true;
        }

        static float LayerDepth(int index) => -.045f - .014f * index;

        void PlaceCollageItem(FloatingStoryItem item)
        {
            if (!TryFindCollagePlacement(item, out var target, out var result))
            {
                if (item.justLifted) { ReturnUnusedSubject(item, "invalid_subject_drop"); return; }
                item.RestorePose(); Record("invalid_drop_return", item.itemId);
                Hint("Release over a photo to attach. Your last placement is safe."); Refresh(); return;
            }
            var previous = item.ownerPhoto;
            bool transfer = previous != target;
            item.transform.SetParent(target.transform, false);
            item.transform.localPosition = result.position; item.transform.localRotation = result.rotation;
            item.transform.localScale = result.scale;
            item.ownerPhoto = target; item.placed = true; item.justLifted = false;
            if (transfer) item.collageLayer = NextLayer(target);
            edited[target] = true;
            if (previous) edited[previous] = true;
            Restack(previous); Restack(target); SetActiveLayer(item);
            string detail = item.itemId + " | source=" + item.sourcePhotoId + " | target=" + target.itemId;
            Record(item.isCutout ? "place_photo_subject" : "place_overlay", detail);
            if (item.isCutout && item.sourcePhotoId != target.itemId) Record("cross_photo_composition", detail);
            if (transfer) Record("transfer_layer", detail);
            if (result.fitted) Record("fit_to_photo", item.itemId);
            if (target != selected)
            {
                if (selected) { selected.persistentHighlight=false; Home(selected); }
                EnterEditor(target);
            }
            else
            {
                EnsureSourceHandles(); Refresh();
                Hint("Placed. FORWARD / BACKWARD changes the overlap between layers.\nUse the blue Publish sphere for the flat preview.");
            }
            ShowPlacementConfirmation(target, item);
        }

        List<FloatingStoryItem> Layers(FloatingStoryItem photo) => overlays
            .Where(i => i && !i.deleted && i.placed && i.ownerPhoto == photo)
            .OrderBy(i => i.collageLayer).ToList();

        int NextLayer(FloatingStoryItem photo) => overlays.Where(i => i && i.ownerPhoto == photo)
            .Select(i => i.collageLayer).DefaultIfEmpty(-1).Max() + 1;

        void Restack(FloatingStoryItem photo)
        {
            if (!photo) return;
            var layers = Layers(photo);
            for (int i=0; i<layers.Count; i++)
            {
                var item = layers[i];
                if (item.Held) continue;
                var pos = item.transform.localPosition; pos.z = LayerDepth(i);
                item.transform.localPosition = pos; item.SavePose();
            }
        }

        void SetActiveLayer(FloatingStoryItem item)
        {
            if (activeLayer) activeLayer.persistentHighlight = false;
            activeLayer = item;
            if (activeLayer) activeLayer.persistentHighlight = stage == Stage.Edit;
            if (activeLayer && activeLayer.isText) lastTouchedText = activeLayer;
        }

        void MoveLayer(int direction)
        {
            var layers = Layers(selected); int index = layers.IndexOf(activeLayer);
            int other = index + direction;
            if (index < 0 || other < 0 || other >= layers.Count) return;
            int rank = activeLayer.collageLayer;
            var crossed = layers[other];
            activeLayer.collageLayer = layers[other].collageLayer; layers[other].collageLayer = rank;
            Restack(selected); edited[selected] = true;
            Record(direction > 0 ? "layer_forward" : "layer_backward", activeLayer.itemId + " | crossed=" + layers[other].itemId);
            RefreshCollage(); Hint("Layer order changed. Check where the subject overlaps your words.");
            ShowLayerFeedback(activeLayer, crossed, direction);
        }

        void CloseSources()
        {
            sourcesOpen = false; Set(sourcePanel, false);
            foreach (var item in sourceHandles) if (item && !item.isDiscoveryHandle && !item.Held) Set(item.gameObject, false);
        }

        void RefreshCollage()
        {
            if (!enableCollage) return;
            bool editing = stage == Stage.Edit && !transitioning;
            bool panels = textPanel && textPanel.activeSelf || stickerPanel && stickerPanel.activeSelf;
            Set(layerPanel, editing && !panels);
            Set(sourcePanel, editing && sourcesOpen);
            foreach (var item in sourceHandles)
            {
                if (!item || item.Held || item.isDiscoveryHandle) continue;
                Set(item.gameObject, editing && sourcesOpen);
                item.Grab.enabled = editing && sourcesOpen && !Holding &&
                    !overlays.Any(i => i && !i.deleted && IsOnSelected(i) && !i.placed);
            }
            if (activeLayer && (activeLayer.deleted || !IsOnSelected(activeLayer))) SetActiveLayer(null);
            foreach (var item in overlays)
                if (item) item.persistentHighlight = editing && item == activeLayer;
            if (layerLabel)
            {
                var layers = Layers(selected); int index = layers.IndexOf(activeLayer);
                layerLabel.text = index >= 0 ? ShortLayerName(activeLayer) + "\n" +
                    (layers.Count == 1 ? "ONLY LAYER" : index == 0 ? "BACK" : index == layers.Count-1 ? "FRONT" : "MIDDLE") +
                    "  |  LAYER " + (index+1) + " / " + layers.Count :
                    "LAYERS\nGrip an element, or choose NEXT LAYER";
            }
            foreach (var button in collageButtons) if (button.Key) button.Key.interactable = IsActionAllowed(button.Value);
            Set(flatPreview, stage == Stage.Review && flatPreviewReady);
            if (stage != Stage.Review) flatPreviewReady = false;
            RefreshCutoutDiscovery();
        }

        void ResetCollage()
        {
            if (!enableCollage) return;
            SetActiveLayer(null); CloseSources(); flatPreviewReady = false; Set(flatPreview, false);
            ResetCutoutDiscovery();
            ResetPlacementFeedback();
        }

        void ForgetItem(FloatingStoryItem item)
        {
            initialGrabPosition.Remove(item); initialGrabRotation.Remove(item); initialGrabScale.Remove(item); twoHandLogged.Remove(item);
        }

        Material NewMaterial(Shader shader, Color color, Texture texture = null)
        {
            var material = new Material(shader) { color=color };
            if (texture) material.mainTexture = texture;
            collageMaterials.Add(material); return material;
        }

        GameObject Quad(string title, Transform parent, Vector3 position, Vector2 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad); go.name = title;
            go.transform.SetParent(parent, false); go.transform.localPosition=position;
            go.transform.localScale = new Vector3(size.x,size.y,1);
            var collision = go.GetComponent<Collider>(); collision.enabled=false; Destroy(collision);
            go.GetComponent<Renderer>().sharedMaterial=material; return go;
        }

        GameObject CollageFrame(Transform parent, Vector2 size)
        {
            var frame = new GameObject("Subject selection outline"); frame.transform.SetParent(parent,false);
            Quad("Top",frame.transform,new Vector3(0,size.y*.5f,-.004f),new Vector2(size.x,.004f),outlineMaterial);
            Quad("Bottom",frame.transform,new Vector3(0,-size.y*.5f,-.004f),new Vector2(size.x,.004f),outlineMaterial);
            Quad("Left",frame.transform,new Vector3(-size.x*.5f,0,-.004f),new Vector2(.004f,size.y),outlineMaterial);
            Quad("Right",frame.transform,new Vector3(size.x*.5f,0,-.004f),new Vector2(.004f,size.y),outlineMaterial);
            return frame;
        }

        GameObject CollageCanvas(string title, Transform parent, Vector3 position, Vector2 dimensions)
        {
            var go = new GameObject(title, typeof(RectTransform),typeof(Canvas),typeof(TrackedDeviceGraphicRaycaster));
            go.transform.SetParent(parent,false); go.transform.localPosition=position; go.transform.localScale=Vector3.one*.001f;
            go.GetComponent<RectTransform>().sizeDelta=dimensions;
            var canvas = go.GetComponent<Canvas>(); canvas.renderMode=RenderMode.WorldSpace; canvas.worldCamera=head;
            go.GetComponent<TrackedDeviceGraphicRaycaster>().checkFor3DOcclusion=false;
            return go;
        }

        Text CollageText(Transform parent, string value, Vector2 position, Vector2 dimensions, int fontSize)
        {
            var go = new GameObject(value,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchoredPosition=position; rect.sizeDelta=dimensions;
            var label=go.GetComponent<Text>(); label.font=hintLabel ? hintLabel.font : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text=value; label.fontSize=fontSize; label.color=Color.white;
            label.alignment=TextAnchor.MiddleCenter; label.raycastTarget=false; return label;
        }

        Button CollageButton(Transform parent, string title, Vector2 position, Vector2 size, int action)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchoredPosition=position; rect.sizeDelta=size;
            go.GetComponent<Image>().color=new Color(.10f,.28f,.33f,1);
            var button=go.GetComponent<Button>(); button.targetGraphic=go.GetComponent<Image>();
            button.onClick.AddListener(()=>Act(action));
            var colors=button.colors; colors.highlightedColor=new Color(.55f,1,.85f); colors.disabledColor=new Color(.4f,.4f,.4f,.45f); button.colors=colors;
            CollageText(go.transform,title,Vector2.zero,size-Vector2.one*8,26);
            collageButtons.Add(button,action); return button;
        }

        void OnDestroy()
        {
            ClearPlacementGhost();
            if (previewTexture) { previewTexture.Release(); Destroy(previewTexture); }
            if (previewCamera) Destroy(previewCamera.gameObject);
            foreach (var material in collageMaterials) if (material) Destroy(material);
        }
    }
}
