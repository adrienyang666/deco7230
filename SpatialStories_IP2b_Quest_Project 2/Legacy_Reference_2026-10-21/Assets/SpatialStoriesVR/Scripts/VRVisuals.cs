using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit.UI;
using System.Collections.Generic;

namespace SpatialStories.Quest
{
    public static class VRVisuals
    {
        static readonly Dictionary<(Color, Texture), Material> materials = new Dictionary<(Color, Texture), Material>();
        public static readonly Color Ink = new Color(.07f, .09f, .13f);
        public static readonly Color Panel = new Color(.12f, .16f, .22f);
        public static readonly Color Blue = new Color(.13f, .43f, .87f);
        public static readonly Color Mint = new Color(.29f, .85f, .72f);
        public static readonly Color Red = new Color(.89f, .19f, .24f);
        public static GameObject Root(string name, Transform parent, Vector3 position)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false); go.transform.localPosition = position; return go;
        }
        public static Material Material(Color color, Texture texture = null)
        {
            var key = (color, texture);
            if (materials.TryGetValue(key, out var existing) && existing) return existing;
            Shader shader = Resources.Load<Shader>("StoryUnlit");
            if (!shader) throw new System.InvalidOperationException("Resources/StoryUnlit.shader is missing.");
            var material = new Material(shader);
            if (material.HasProperty("_Color")) material.color = color;
            if (texture != null) material.mainTexture = texture;
            materials[key] = material;
            return material;
        }
        public static void ReleaseMaterials()
        {
            foreach (var material in materials.Values) if (material) Object.Destroy(material);
            materials.Clear();
        }
        public static GameObject Box(string name, Transform parent, Vector3 position, Vector3 size, Material material, bool collider = false)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = size;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collider) { var c = go.GetComponent<Collider>(); c.enabled = false; Object.Destroy(c); }
            return go;
        }
        public static GameObject Quad(string name, Transform parent, Vector3 position, Vector2 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = position; go.transform.localScale = new Vector3(size.x, size.y, 1);
            go.GetComponent<Renderer>().sharedMaterial = material;
            var c = go.GetComponent<Collider>(); c.enabled = false; Object.Destroy(c);
            return go;
        }
        public static Text Label(string name, Transform parent, string text, Vector3 position, float characterSize, Color color)
        {
            var canvas = Canvas(name, parent, position, new Vector2(1000, 140), null);
            return UIText(canvas.transform, text, Vector2.zero, new Vector2(1000, 140),
                Mathf.RoundToInt(characterSize * 1000), color);
        }
        // Canvas is 0.001 metres per UI unit; default orientation faces the player at -Z.
        public static Canvas Canvas(string name, Transform parent, Vector3 position, Vector2 size, Camera camera)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false); go.transform.localPosition = position;
            go.transform.localScale = Vector3.one * .001f;
            ((RectTransform)go.transform).sizeDelta = size;
            var canvas = go.AddComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = camera;
            go.AddComponent<CanvasScaler>().dynamicPixelsPerUnit = 3;
            go.AddComponent<TrackedDeviceGraphicRaycaster>();
            return canvas;
        }
        public static Text UIText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform; rect.sizeDelta = size; rect.anchoredPosition = position;
            var text = go.AddComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value; text.fontSize = fontSize; text.alignment = TextAnchor.MiddleCenter;
            text.color = color; text.raycastTarget = false;
            return text;
        }
        public static Button Button(Transform parent, string label, Vector2 position, Vector2 size, UnityAction clicked, Color color)
        {
            var go = new GameObject(label, typeof(RectTransform)); go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform; rect.anchoredPosition = position; rect.sizeDelta = size;
            var image = go.AddComponent<Image>(); image.color = color;
            var button = go.AddComponent<Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1.2f, 1.2f, 1.2f);
            colors.selectedColor = Color.white; colors.disabledColor = new Color(.35f, .35f, .35f); button.colors = colors;
            var navigation = button.navigation; navigation.mode = Navigation.Mode.None; button.navigation = navigation;
            UIText(go.transform, label, Vector2.zero, size - new Vector2(12, 8), 24, Color.white);
            button.onClick.AddListener(clicked); return button;
        }
    }
}
