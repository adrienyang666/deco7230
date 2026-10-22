using UnityEngine;

namespace SpatialStories.Immersive
{
    /// <summary>Geometry in photo-local coordinates; supports rotated photos and uniform parent scale.</summary>
    public static class CollagePlacement
    {
        public struct Placement
        {
            public Vector3 position, scale;
            public Quaternion rotation;
            public bool fitted;
        }

        public static Vector2 HalfExtents(Vector2 size, Vector3 scale, float angle)
        {
            var radians = angle * Mathf.Deg2Rad;
            var c = Mathf.Abs(Mathf.Cos(radians));
            var s = Mathf.Abs(Mathf.Sin(radians));
            var w = size.x * Mathf.Abs(scale.x);
            var h = size.y * Mathf.Abs(scale.y);
            return new Vector2(c * w + s * h, s * w + c * h) * .5f;
        }

        public static Placement Fit(Vector2 photoSize, Vector2 itemSize, Vector3 localPosition,
            Vector3 localScale, float angle)
        {
            var extents = HalfExtents(itemSize, localScale, angle);
            var available = Vector2.Max(Vector2.one * .001f, photoSize * .5f - Vector2.one * .008f);
            var factor = Mathf.Min(1f, available.x / Mathf.Max(.0001f, extents.x),
                available.y / Mathf.Max(.0001f, extents.y));
            var scale = localScale * factor;
            extents *= factor;
            localPosition.x = Mathf.Clamp(localPosition.x, -available.x + extents.x, available.x - extents.x);
            localPosition.y = Mathf.Clamp(localPosition.y, -available.y + extents.y, available.y - extents.y);
            localPosition.z = -.045f;
            return new Placement { position = localPosition, scale = scale,
                rotation = Quaternion.Euler(0, 0, angle), fitted = factor < .999f };
        }

        public static bool TryProject(FloatingStoryItem item, FloatingStoryItem photo,
            out Placement placement, out float score)
        {
            placement = default;
            score = float.MaxValue;
            if (!item || !photo || photo.deleted || !photo.gameObject.activeInHierarchy) return false;
            var local = photo.transform.InverseTransformPoint(item.transform.position);
            var photoScale = photo.transform.lossyScale;
            var worldDepth = Mathf.Abs(local.z * photoScale.z);
            // Allow a small border catch area and the visible stack's depth, never distant drops.
            if (worldDepth > .38f || Mathf.Abs(local.x) > photo.size.x * .5f + .075f ||
                Mathf.Abs(local.y) > photo.size.y * .5f + .075f) return false;
            var up = Vector3.ProjectOnPlane(item.transform.up, photo.transform.forward);
            var angle = up.sqrMagnitude < .0001f ? 0 : Vector3.SignedAngle(photo.transform.up, up, photo.transform.forward);
            var worldScale = item.transform.lossyScale;
            var scale = new Vector3(Mathf.Abs(worldScale.x / photoScale.x), Mathf.Abs(worldScale.y / photoScale.y),
                Mathf.Abs(worldScale.z / photoScale.z));
            placement = Fit(photo.size, item.size, local, scale, angle);
            score = worldDepth + .03f * new Vector2(local.x, local.y).magnitude;
            return true;
        }
    }
}
