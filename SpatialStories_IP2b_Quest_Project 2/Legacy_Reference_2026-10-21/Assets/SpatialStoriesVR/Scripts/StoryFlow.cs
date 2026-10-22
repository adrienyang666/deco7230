namespace SpatialStories.Quest
{
    // Pure state rules shared by the scene and Edit Mode tests.
    public enum StoryStage { Welcome, Browse, Edit, Preview, Published }

    public sealed class StoryFlow
    {
        public StoryStage Stage { get; private set; } = StoryStage.Welcome;
        public int Photo { get; private set; } = -1;
        public void Reset() { Stage = StoryStage.Welcome; Photo = -1; }
        public void Begin() { Stage = StoryStage.Browse; Photo = -1; }
        public bool SelectPhoto(int index)
        {
            if (Stage != StoryStage.Browse || index < 0 || index >= 6) return false;
            Photo = index; Stage = StoryStage.Edit; return true;
        }
        public bool Preview(int activeOverlays, bool holding)
        {
            if (Stage != StoryStage.Edit || Photo < 0 || activeOverlays < 1 || holding) return false;
            Stage = StoryStage.Preview; return true;
        }
        public bool Publish()
        {
            if (Stage != StoryStage.Preview) return false;
            Stage = StoryStage.Published; return true;
        }
        public bool ContinueEditing()
        {
            if (Stage != StoryStage.Preview) return false;
            Stage = StoryStage.Edit; return true;
        }
        public bool ChangePhoto(bool holding)
        {
            if (Stage != StoryStage.Edit || holding) return false;
            Stage = StoryStage.Browse; Photo = -1; return true;
        }
    }
}
