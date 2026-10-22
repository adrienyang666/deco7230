namespace SpatialStories.Immersive
{
    /// <summary>Per-trial first-use state. Opening the album alone does not consume the invitation.</summary>
    public sealed class CutoutDiscoveryState
    {
        public bool HasBeenShown { get; private set; }
        public bool Consumed { get; private set; }
        public bool IsAnimating { get; private set; }
        public bool GuidanceVisible => HasBeenShown && !Consumed;

        public bool TryStart()
        {
            if (HasBeenShown || Consumed) return false;
            HasBeenShown=true; IsAnimating=true; return true;
        }

        public void FinishAnimation() { IsAnimating=false; }

        public bool Consume()
        {
            if (!HasBeenShown || Consumed) return false;
            Consumed=true; IsAnimating=false; return true;
        }

        public void Reset()
        {
            HasBeenShown=false; Consumed=false; IsAnimating=false;
        }
    }
}
