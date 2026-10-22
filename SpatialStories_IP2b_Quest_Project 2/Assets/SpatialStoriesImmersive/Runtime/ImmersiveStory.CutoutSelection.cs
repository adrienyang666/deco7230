using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SpatialStories.Immersive
{
    public sealed partial class ImmersiveStory
    {
        // CreateSourceHandle must construct its root with typeof(RectTransform).
        // Keep Graphic, Button and the EXISTING grab collider on that same root:
        // NearFar then recognises one 2D/3D target without a protruding click face.
        void EnsureCutoutClickFace(FloatingStoryItem item)
        {
            if (!item || !item.isCutout) return;
            var rect = item.transform as RectTransform;
            var collider = item.GetComponent<BoxCollider>();
            if (!rect || !collider || !item.Grab)
            {
                Debug.LogWarning("Cutout selection needs a RectTransform root and its original grab collider.", item);
                return;
            }

            // Rect dimensions use world-sized local units. Do not scale or move
            // the item, collider or visual to turn it into a pixel-sized canvas.
            var position = rect.localPosition;
            rect.sizeDelta = item.size;
            rect.localPosition = position;
            var canvas = item.GetComponent<Canvas>();
            if (!canvas) canvas = item.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = head;
            var raycaster = item.GetComponent<TrackedDeviceGraphicRaycaster>();
            if (!raycaster) raycaster = item.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
            raycaster.checkFor2DOcclusion = false;
            raycaster.checkFor3DOcclusion = false;
            raycaster.ignoreReversedGraphics = true;
            var image = item.GetComponent<Image>();
            if (!image) image = item.gameObject.AddComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            image.canvasRenderer.cullTransparentMesh = false;

            var button = item.GetComponent<Button>();
            if (!button)
            {
                button = item.gameObject.AddComponent<Button>();
                var c = item.controller;
                button.onClick.AddListener(() => { if (c && item) c.ItemActivated(item); });
            }
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            if (!item.Grab.colliders.Contains(collider)) item.Grab.colliders.Add(collider);
        }

        // Selection never lifts, moves, reparents or changes the rank of a cutout.
        bool TrySelectCutout(FloatingStoryItem item)
        {
            if (!enableCollage || !item || !item.isCutout || item.controller != this ||
                !isActiveAndEnabled || !item.isActiveAndEnabled || item.deleted ||
                stage != Stage.Edit || transitioning || Holding) return false;
            if (item.isSourceHandle)
            {
                PulseCutoutOutline(item);
                Hint("Selected: " + item.content + ".\nHold Grip to lift this subject, then release over a photo.");
                Record("select_source", item.itemId + " | source=" + item.sourcePhotoId);
                return true;
            }
            if (!item.placed || !IsOnSelected(item)) return false;
            SetActiveLayer(item);
            RefreshCollage();
            Hint("Selected: " + item.content + ".\nFORWARD / BACKWARD changes its layer; Grip moves it.");
            Record("select_layer", item.itemId);
            return true;
        }

        void PulseCutoutOutline(FloatingStoryItem item)
        {
            if (!Application.isPlaying || !item.highlight) return;
            var outline = item.highlight.transform;
            const string pulseName = "Cutout selection pulse";
            if (outline.Find(pulseName)) return;
            // Nest feedback beneath the existing highlight. The placement ghost
            // excludes that subtree; flat capture already hides that subtree.
            var pulse = Instantiate(item.highlight, outline, false);
            pulse.name = pulseName;
            pulse.transform.localPosition = Vector3.zero;
            pulse.transform.localRotation = Quaternion.identity;
            pulse.transform.localScale = Vector3.one * 1.09f;
            pulse.SetActive(true);
            Destroy(pulse, .22f);
        }
    }
}
