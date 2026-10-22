using NUnit.Framework;

namespace SpatialStories.Immersive.Tests
{
    public class CutoutDiscoveryTests
    {
        [Test]
        public void CompletedDemoDoesNotReplayWhenAnotherPhotoIsOpened()
        {
            var state=new CutoutDiscoveryState();
            Assert.That(state.TryStart(),Is.True);
            state.FinishAnimation();
            Assert.That(state.IsAnimating,Is.False);
            Assert.That(state.GuidanceVisible,Is.True);
            Assert.That(state.TryStart(),Is.False);
        }

        [Test]
        public void FirstAuthoringActionInterruptsTheDemoAndDismissesItsGuidance()
        {
            var state=new CutoutDiscoveryState();
            state.TryStart();
            Assert.That(state.Consume(),Is.True);
            Assert.That(state.IsAnimating,Is.False);
            Assert.That(state.GuidanceVisible,Is.False);
            Assert.That(state.TryStart(),Is.False);
            Assert.That(state.Consume(),Is.False); // No second dismissal event.
        }

        [Test]
        public void GalleryActionsDoNotUseUpTheFirstEditorInvitation()
        {
            var state=new CutoutDiscoveryState();
            Assert.That(state.Consume(),Is.False);
            Assert.That(state.TryStart(),Is.True);
        }

        [Test]
        public void AimingCanStopMotionWithoutRemovingTheStillUsableInvitation()
        {
            var state=new CutoutDiscoveryState();
            state.TryStart(); state.FinishAnimation();
            Assert.That(state.IsAnimating,Is.False);
            Assert.That(state.GuidanceVisible,Is.True);
            Assert.That(state.Consumed,Is.False);
            Assert.That(state.TryStart(),Is.False);
        }

        [Test]
        public void ResetRearmsTheInvitationForTheNextParticipant()
        {
            var state=new CutoutDiscoveryState();
            state.TryStart(); state.Consume(); state.Reset();
            Assert.That(state.HasBeenShown,Is.False);
            Assert.That(state.GuidanceVisible,Is.False);
            Assert.That(state.TryStart(),Is.True);
            Assert.That(state.IsAnimating,Is.True);
        }
    }
}
