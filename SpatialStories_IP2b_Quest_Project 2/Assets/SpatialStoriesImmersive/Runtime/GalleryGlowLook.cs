using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace SpatialStories.Immersive
{
    /// <summary>Small, reversible visual treatment; leaves interaction and photo content alone.</summary>
    [DisallowMultipleComponent]
    public sealed class GalleryGlowLook : MonoBehaviour
    {
        readonly List<Renderer> surfaces = new List<Renderer>();
        readonly List<Material> originals = new List<Material>();
        readonly List<Material> replacements = new List<Material>();
        readonly List<Material> materials = new List<Material>();
        readonly List<GameObject> halos = new List<GameObject>();
        readonly List<Transform> rings = new List<Transform>();
        ImmersiveStory story;
        Mesh quad;
        Camera savedCamera;
        CameraClearFlags savedClear;
        Color savedBackground;
        bool ready, applied;

        public void Initialize(ImmersiveStory owner)
        {
            if (story) return;
            story = owner;
            if (!Application.isPlaying || !IsIP2b()) return;
            var surface = Resources.Load<Shader>("GalleryGlowSurface");
            var halo = Resources.Load<Shader>("GalleryGlowHalo");
            if (!surface || !halo)
            {
                Debug.LogWarning("Gallery glow shaders are missing; keeping the original look.", this);
                return;
            }
            quad = MakeQuad();
            if (story.toolGroup)
            {
                foreach (var renderer in story.toolGroup.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer.name != "Physical sphere" || !renderer.sharedMaterial) continue;
                    var color = renderer.sharedMaterial.color;
                    var material = MakeMaterial(surface, color);
                    material.SetColor("_RimColor", Color.Lerp(color, Color.white, .45f));
                    surfaces.Add(renderer); originals.Add(renderer.sharedMaterial); replacements.Add(material);
                    var ring = AddHalo(renderer.transform, MakeMaterial(halo, new Color(color.r, color.g, color.b, .28f), 0));
                    ring.localScale = Vector3.one * 1.55f;
                    rings.Add(ring);
                }
            }
            var frameMaterial = MakeMaterial(halo, new Color(1f, .78f, .25f, .24f), 1);
            if (story.mediaCards != null)
            {
                foreach (var card in story.mediaCards)
                {
                    if (!card || !card.highlight) continue;
                    foreach (var name in new[] { "Left", "Right", "Top", "Bottom" })
                    {
                        var bar = card.highlight.transform.Find(name);
                        if (!bar) continue;
                        bool vertical = name == "Left" || name == "Right";
                        // Cancel only the bar's thin local scale; keep the photo's own scale.
                        float length = Mathf.Abs(vertical ? bar.localScale.y : bar.localScale.x);
                        float width = Mathf.Abs(vertical ? bar.localScale.x : bar.localScale.y);
                        if (length < .0001f || width < .0001f) continue;
                        var strip = AddHalo(bar, frameMaterial);
                        strip.localPosition = new Vector3(0, 0, -.55f);
                        strip.localRotation = Quaternion.Euler(0, 0, vertical ? 90 : 0);
                        strip.localScale = new Vector3((length + .024f) / length, .045f / width, 1);
                    }
                }
            }
            ready = true;
            SetLook(story.enableGalleryGlow);
        }

        bool IsIP2b()
        {
            if (!story || !story.enableCollage) return false;
            string scene = story.gameObject.scene.name;
            return scene == "SpatialStories_IP2b_Quest" || scene == "SpatialStories_IP2b_Simulator";
        }

        void LateUpdate()
        {
            if (!ready) return;
            SetLook(IsIP2b() && story.isActiveAndEnabled && story.enableGalleryGlow);
            if (applied && story.head)
                foreach (var ring in rings)
                    if (ring) ring.rotation = story.head.transform.rotation;
        }

        void SetLook(bool on)
        {
            if (on == applied) return;
            for (int i = 0; i < surfaces.Count; i++)
                if (surfaces[i]) surfaces[i].sharedMaterial = on ? replacements[i] : originals[i];
            foreach (var halo in halos) if (halo) halo.SetActive(on);
            if (on && story && story.head)
            {
                savedCamera = story.head;
                savedClear = savedCamera.clearFlags; savedBackground = savedCamera.backgroundColor;
                savedCamera.clearFlags = CameraClearFlags.SolidColor;
                savedCamera.backgroundColor = new Color(.018f, .032f, .065f, 1);
            }
            else if (savedCamera)
            {
                savedCamera.clearFlags = savedClear; savedCamera.backgroundColor = savedBackground;
                savedCamera = null;
            }
            applied = on;
        }

        Material MakeMaterial(Shader shader, Color color, int shape = -1)
        {
            var material = new Material(shader) { name = "Gallery glow (runtime)", color = color, enableInstancing = true };
            if (shape >= 0) material.SetFloat("_Shape", shape);
            materials.Add(material);
            return material;
        }

        Transform AddHalo(Transform parent, Material material)
        {
            var go = new GameObject("Gallery glow - visual only", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.SetParent(parent, false);
            go.GetComponent<MeshFilter>().sharedMesh = quad;
            var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
            go.SetActive(false); halos.Add(go);
            return go.transform;
        }

        static Mesh MakeQuad()
        {
            var mesh = new Mesh { name = "Gallery glow quad (runtime)" };
            mesh.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0),
                new Vector3(-.5f, .5f, 0), new Vector3(.5f, .5f, 0) };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateBounds();
            return mesh;
        }

        void OnDisable() { SetLook(false); }

        void OnDestroy()
        {
            SetLook(false);
            foreach (var halo in halos) if (halo) Destroy(halo);
            foreach (var material in materials) if (material) Destroy(material);
            if (quad) Destroy(quad);
        }
    }
}
