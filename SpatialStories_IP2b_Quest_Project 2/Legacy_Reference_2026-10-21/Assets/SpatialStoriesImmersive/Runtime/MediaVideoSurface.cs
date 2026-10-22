using UnityEngine;
using UnityEngine.Video;

namespace SpatialStories.Immersive
{
    /// <summary>Optional local video on the same spatial card. No video is invented or downloaded.</summary>
    public sealed class MediaVideoSurface : MonoBehaviour
    {
        [Tooltip("Optional: assign an imported local VideoClip to turn this photo card into video.")]
        public VideoClip clip;
        public Renderer surface;
        public bool playAudio;
        VideoPlayer player;
        RenderTexture texture;
        Material original, instance;

        void Start()
        {
            if (!clip || !surface) return;
            original = surface.sharedMaterial;
            instance = new Material(original);
            surface.sharedMaterial = instance;
            // Match the card aspect, so FitInside letterboxes a landscape clip
            // instead of stretching it across a portrait mesh.
            float ratio = Mathf.Abs(surface.transform.localScale.x) / Mathf.Max(.01f, Mathf.Abs(surface.transform.localScale.y));
            int height = 720;
            int width = Mathf.Clamp(Mathf.RoundToInt(height * ratio), 64, 1280);
            texture = new RenderTexture(width, height, 0);
            texture.Create(); instance.mainTexture = texture;
            player = gameObject.AddComponent<VideoPlayer>();
            player.playOnAwake = false; player.isLooping = true; player.waitForFirstFrame = true;
            player.source = VideoSource.VideoClip; player.clip = clip;
            player.renderMode = VideoRenderMode.RenderTexture; player.targetTexture = texture;
            player.aspectRatio = VideoAspectRatio.FitInside;
            player.audioOutputMode = playAudio ? VideoAudioOutputMode.Direct : VideoAudioOutputMode.None;
            player.errorReceived += OnError;
            player.Play();
        }
        void OnEnable() { if (player) player.Play(); }
        void OnDisable() { if (player) player.Pause(); }
        void OnError(VideoPlayer source, string message)
        {
            if(instance && original) instance.mainTexture = original.mainTexture;
            Debug.LogWarning("This local clip could not be decoded; showing its poster image. " + message, this);
        }
        void OnDestroy()
        {
            if(player) { player.errorReceived -= OnError; player.Stop(); }
            if(surface && original) surface.sharedMaterial=original;
            if(texture) { texture.Release(); Destroy(texture); }
            if(instance) Destroy(instance);
        }
    }
}
