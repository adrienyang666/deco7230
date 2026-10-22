using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialStories.Immersive
{
    public sealed partial class ImmersiveStory
    {
        [Tooltip("Show the predicted drop and brief placement/layer feedback in IP2b.")]
        public bool enablePlacementFeedback = true;

        GameObject placementFeedbackRoot, placementGhost, placementTargetFrame;
        Text placementStatus;
        Transform placementBadge;
        Shader placementGhostShader;
        FloatingStoryItem ghostSource, lastPreviewTarget, confirmationPhoto, confirmationItem;
        float confirmationUntil;
        bool confirmationOutlinesItem;
        readonly List<Material> placementGhostMaterials = new List<Material>();
        readonly Dictionary<Material, Material> placementMaterialCopies = new Dictionary<Material, Material>();

        void InitializePlacementFeedback()
        {
            placementGhostShader = Resources.Load<Shader>("CollageGhost");
            if (!placementGhostShader)
            {
                Debug.LogError("IP2b placement preview shader is missing.", this);
                return;
            }
            // This root never belongs to a photograph or to the editable layer list.
            placementFeedbackRoot = new GameObject("IP2b - placement feedback (visual only)");
            placementFeedbackRoot.transform.SetParent(contentRoot, false);
            placementTargetFrame = CollageFrame(placementFeedbackRoot.transform, Vector2.one);
            placementTargetFrame.name = "Valid target / confirmation outline";
            var badge = CollageCanvas("Drop feedback", placementFeedbackRoot.transform,
                Vector3.zero, new Vector2(500,72));
            // Neither the badge nor any preview visual participates in XR selection.
            var raycaster = badge.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
            raycaster.enabled = false;
            var background = badge.AddComponent<Image>();
            background.color = new Color(.02f,.10f,.10f,.94f); background.raycastTarget = false;
            placementStatus = CollageText(badge.transform, "RELEASE TO PLACE", Vector2.zero, new Vector2(484,66), 23);
            placementBadge = badge.transform;
            placementFeedbackRoot.SetActive(false);
        }

        void UpdatePlacementFeedback()
        {
            if (!placementFeedbackRoot) return;
            if (!enableCollage || !enablePlacementFeedback || stage != Stage.Edit ||
                (adminPanel && adminPanel.activeSelf))
            { ResetPlacementFeedback(); return; }

            FloatingStoryItem held = null;
            foreach (var item in overlays)
                if (item && !item.deleted && item.Held) { held = item; break; }
            if (held && !transitioning)
            {
                confirmationUntil = 0;
                EnsurePlacementGhost(held);
                if (!TryFindCollagePlacement(held, out var target, out var result))
                {
                    if (lastPreviewTarget) Record("placement_preview_lost", held.itemId + " | target=" + lastPreviewTarget.itemId);
                    lastPreviewTarget = null;
                    Set(placementFeedbackRoot, false);
                    return;
                }
                if (lastPreviewTarget != target)
                    Record("placement_preview_target", held.itemId + " | target=" + target.itemId);
                lastPreviewTarget = target;
                MatchFeedbackToPhoto(target);
                placementGhost.transform.localPosition = result.position;
                placementGhost.transform.localRotation = result.rotation;
                placementGhost.transform.localScale = result.scale;
                SetPhotoFeedbackFrame(target);
                placementStatus.text = result.fitted ? "RESIZED TO FIT\nRELEASE TO PLACE" : "RELEASE TO PLACE";
                Set(placementGhost, true);
                Set(placementFeedbackRoot, true);
                return;
            }

            ClearPlacementGhost();
            if (Holding || Time.unscaledTime >= confirmationUntil || !confirmationPhoto ||
                confirmationPhoto.deleted || !confirmationPhoto.gameObject.activeInHierarchy ||
                (textPanel && textPanel.activeSelf) || (stickerPanel && stickerPanel.activeSelf))
            { Set(placementFeedbackRoot, false); confirmationUntil = 0; return; }
            MatchFeedbackToPhoto(confirmationPhoto);
            if (confirmationOutlinesItem && confirmationItem && !confirmationItem.deleted)
            {
                var frame = placementTargetFrame.transform;
                frame.localPosition = new Vector3(confirmationItem.transform.localPosition.x,
                    confirmationItem.transform.localPosition.y, -.24f);
                frame.localRotation = confirmationItem.transform.localRotation;
                var scale = confirmationItem.transform.localScale;
                frame.localScale = new Vector3(confirmationItem.size.x * Mathf.Abs(scale.x) + .012f,
                    confirmationItem.size.y * Mathf.Abs(scale.y) + .012f, 1);
            }
            else SetPhotoFeedbackFrame(confirmationPhoto);
            Set(placementFeedbackRoot, true);
        }

        void MatchFeedbackToPhoto(FloatingStoryItem photo)
        {
            placementFeedbackRoot.transform.SetPositionAndRotation(photo.transform.position, photo.transform.rotation);
            placementFeedbackRoot.transform.localScale = RelativeScale(photo.transform.lossyScale, contentRoot.lossyScale);
            placementBadge.localPosition = new Vector3(0, photo.size.y * .5f + .065f, -.24f);
        }

        void SetPhotoFeedbackFrame(FloatingStoryItem photo)
        {
            placementTargetFrame.transform.localPosition = new Vector3(0,0,-.24f);
            placementTargetFrame.transform.localRotation = Quaternion.identity;
            placementTargetFrame.transform.localScale = new Vector3(photo.size.x + .018f, photo.size.y + .018f, 1);
        }

        void ShowPlacementConfirmation(FloatingStoryItem photo, FloatingStoryItem item)
        {
            ShowCollageFeedback(photo, item, "PLACED", false, .7f);
        }

        void ShowLayerFeedback(FloatingStoryItem item, FloatingStoryItem crossed, int direction)
        {
            ShowCollageFeedback(item.ownerPhoto, item,
                (direction > 0 ? "IN FRONT OF\n" : "BEHIND\n") + ShortLayerName(crossed), true, 1.1f);
        }

        void ShowCollageFeedback(FloatingStoryItem photo, FloatingStoryItem item, string message, bool outlineItem, float duration)
        {
            if (!enablePlacementFeedback || !placementFeedbackRoot) return;
            ClearPlacementGhost();
            confirmationPhoto = photo; confirmationItem = item;
            confirmationOutlinesItem = outlineItem;
            confirmationUntil = Time.unscaledTime + duration;
            placementStatus.text = message;
            UpdatePlacementFeedback();
        }

        void EnsurePlacementGhost(FloatingStoryItem source)
        {
            if (placementGhost && ghostSource == source) return;
            ClearPlacementGhost();
            ghostSource = source;
            placementGhost = new GameObject("Predicted placement - no input");
            placementGhost.transform.SetParent(placementFeedbackRoot.transform, false);
            // Copy only static visual components. Never instantiate an interactable, controller or collider.
            foreach (var mesh in source.GetComponentsInChildren<MeshFilter>())
            {
                var renderer = mesh.GetComponent<MeshRenderer>();
                if (!mesh.sharedMesh || !renderer || !renderer.enabled ||
                    (source.highlight && mesh.transform.IsChildOf(source.highlight.transform))) continue;
                var go = new GameObject("Preview " + mesh.name, typeof(MeshFilter), typeof(MeshRenderer));
                CopyVisualPose(go.transform, mesh.transform, source.transform);
                go.GetComponent<MeshFilter>().sharedMesh = mesh.sharedMesh;
                var output = go.GetComponent<MeshRenderer>();
                var originals = renderer.sharedMaterials;
                var materials = new Material[originals.Length];
                for (int i=0; i<materials.Length; i++) materials[i] = GhostMaterial(originals[i]);
                output.sharedMaterials = materials;
                output.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                output.receiveShadows = false;
            }
            if (source.label && source.label.enabled)
            {
                var original = source.label;
                var go = new GameObject("Preview words", typeof(RectTransform), typeof(Canvas), typeof(Text));
                var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = head;
                CopyVisualPose(go.transform, original.transform, source.transform);
                var rect = go.GetComponent<RectTransform>();
                rect.pivot = original.rectTransform.pivot;
                rect.sizeDelta = original.rectTransform.rect.size;
                var label = go.GetComponent<Text>();
                label.font = original.font; label.fontSize = original.fontSize; label.fontStyle = original.fontStyle;
                label.alignment = original.alignment; label.lineSpacing = original.lineSpacing;
                label.horizontalOverflow = original.horizontalOverflow; label.verticalOverflow = original.verticalOverflow;
                label.resizeTextForBestFit = original.resizeTextForBestFit;
                label.resizeTextMinSize = original.resizeTextMinSize; label.resizeTextMaxSize = original.resizeTextMaxSize;
                label.supportRichText = original.supportRichText; label.color = original.color;
                label.text = original.text; label.raycastTarget = false; label.material = GhostMaterial(original.material);
            }
        }

        void CopyVisualPose(Transform destination, Transform visual, Transform item)
        {
            destination.SetParent(placementGhost.transform, false);
            destination.localPosition = item.InverseTransformPoint(visual.position);
            destination.localRotation = Quaternion.Inverse(item.rotation) * visual.rotation;
            destination.localScale = RelativeScale(visual.lossyScale, item.lossyScale);
        }

        static Vector3 RelativeScale(Vector3 world, Vector3 parent) => new Vector3(
            world.x / Mathf.Max(.0001f, Mathf.Abs(parent.x)),
            world.y / Mathf.Max(.0001f, Mathf.Abs(parent.y)),
            world.z / Mathf.Max(.0001f, Mathf.Abs(parent.z)));

        Material GhostMaterial(Material source)
        {
            if (source && placementMaterialCopies.TryGetValue(source, out var cached)) return cached;
            var material = new Material(placementGhostShader);
            Color color = source && source.HasProperty("_Color") ? source.color : Color.white;
            color.a *= .38f; material.color = color;
            if (source && source.HasProperty("_MainTex"))
            {
                material.mainTexture = source.mainTexture;
                material.mainTextureScale = source.mainTextureScale;
                material.mainTextureOffset = source.mainTextureOffset;
            }
            material.SetFloat("_Cutoff", source && source.HasProperty("_Cutoff") ? source.GetFloat("_Cutoff") : .02f);
            placementGhostMaterials.Add(material);
            if (source) placementMaterialCopies[source] = material;
            return material;
        }

        void ClearPlacementGhost()
        {
            if (placementGhost) { placementGhost.SetActive(false); Destroy(placementGhost); }
            placementGhost = null; ghostSource = null; lastPreviewTarget = null;
            foreach (var material in placementGhostMaterials) if (material) Destroy(material);
            placementGhostMaterials.Clear(); placementMaterialCopies.Clear();
        }

        void ResetPlacementFeedback()
        {
            ClearPlacementGhost();
            confirmationUntil = 0; confirmationPhoto = null; confirmationItem = null;
            Set(placementFeedbackRoot, false);
        }

        void OnDisable() { CleanupQuestRecovery(); ResetPlacementFeedback(); }

        static string ShortLayerName(FloatingStoryItem item)
        {
            if (!item) return "ELEMENT";
            string name = (item.isText ? "TEXT: " : "") + (item.content ?? "ELEMENT").Replace('\n', ' ');
            return name.Length > 26 ? name.Substring(0,23) + "..." : name;
        }

        void AddLayerIcon(Button button, bool forward)
        {
            var label = button.GetComponentInChildren<Text>();
            label.rectTransform.anchoredPosition = new Vector2(23,0);
            label.rectTransform.sizeDelta = new Vector2(160,54); label.fontSize = 22;
            Color selectedColor = new Color(.45f,1,.80f), otherColor = new Color(.60f,.70f,.76f);
            for (int i=0; i<2; i++)
            {
                var go = new GameObject(i == 0 ? "Back card" : "Front card", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(button.transform, false);
                var rect = go.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(25,25); rect.anchoredPosition = new Vector2(-80+i*10,5-i*10);
                var image = go.GetComponent<Image>(); image.raycastTarget = false;
                image.color = (forward ? i == 1 : i == 0) ? selectedColor : otherColor;
                var outline = go.AddComponent<Outline>(); outline.effectColor = new Color(.02f,.08f,.10f);
                outline.effectDistance = new Vector2(1,-1);
            }
        }
    }
}
