using UnityEngine;

namespace SpatialStories.Quest
{
    // Runtime sticker meshes are owned by their GameObject, including across trial resets.
    public sealed class VRMeshLifetime : MonoBehaviour
    {
        public Mesh mesh;
        void OnDestroy() { if (mesh) Destroy(mesh); }
    }
}
