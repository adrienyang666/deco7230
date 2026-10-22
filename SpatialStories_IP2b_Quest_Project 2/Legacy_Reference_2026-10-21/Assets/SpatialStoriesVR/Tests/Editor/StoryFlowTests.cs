using NUnit.Framework;

namespace SpatialStories.Quest.Tests
{
    public sealed class StoryFlowTests
    {
        [Test]
        public void PublishRequiresPreviewAndAnEdit()
        {
            var flow = new StoryFlow();
            Assert.That(flow.Publish(), Is.False);
            flow.Begin(); flow.SelectPhoto(2);
            Assert.That(flow.Publish(), Is.False);
            Assert.That(flow.Preview(0, false), Is.False);
            Assert.That(flow.Preview(1, true), Is.False);
            Assert.That(flow.Stage, Is.EqualTo(StoryStage.Edit));
            Assert.That(flow.Preview(1, false), Is.True);
            Assert.That(flow.Publish(), Is.True);
        }
        [Test]
        public void RepeatedPublishDoesNotCreateAnotherCompletion()
        {
            var flow = new StoryFlow(); flow.Begin(); flow.SelectPhoto(0);
            flow.Preview(1, false); Assert.That(flow.Publish(), Is.True);
            Assert.That(flow.Publish(), Is.False);
            Assert.That(flow.SelectPhoto(1), Is.False);
        }
        [Test]
        public void PreviewCanReturnToTheSamePhotoWithoutResettingTheSession()
        {
            var flow = new StoryFlow(); flow.Begin(); flow.SelectPhoto(4); flow.Preview(2, false);
            Assert.That(flow.ContinueEditing(), Is.True);
            Assert.That(flow.Photo, Is.EqualTo(4));
            Assert.That(flow.Stage, Is.EqualTo(StoryStage.Edit));
        }
        [Test]
        public void ChangingPhotosIsBlockedWhileAnItemIsHeld()
        {
            var flow = new StoryFlow(); flow.Begin(); flow.SelectPhoto(1);
            Assert.That(flow.ChangePhoto(true), Is.False);
            Assert.That(flow.Photo, Is.EqualTo(1));
            Assert.That(flow.ChangePhoto(false), Is.True);
            Assert.That(flow.Photo, Is.EqualTo(-1));
            Assert.That(flow.Stage, Is.EqualTo(StoryStage.Browse));
        }
        [Test]
        public void NewSessionRemovesThePreviousPhotoState()
        {
            var flow = new StoryFlow(); flow.Begin(); flow.SelectPhoto(5);
            flow.Preview(2, false); flow.Publish(); flow.Reset();
            Assert.That(flow.Stage, Is.EqualTo(StoryStage.Welcome));
            flow.Begin();
            Assert.That(flow.Stage, Is.EqualTo(StoryStage.Browse));
            Assert.That(flow.Photo, Is.EqualTo(-1));
            Assert.That(flow.Preview(2, false), Is.False);
        }
        [TestCase(-1)] [TestCase(6)]
        public void InvalidAlbumIndexCannotEnterEdit(int index)
        {
            var flow = new StoryFlow(); flow.Begin();
            Assert.That(flow.SelectPhoto(index), Is.False);
            Assert.That(flow.Stage, Is.EqualTo(StoryStage.Browse));
        }
        [Test]
        public void CsvEscapesQuotesCommasAndNewlines()
        {
            Assert.That(VRSessionLog.CsvCell("said \"hello\",\nagain"), Is.EqualTo("\"said \"\"hello\"\",\nagain\""));
            Assert.That(VRSessionLog.CsvCell(null), Is.EqualTo("\"\""));
        }
    }
}
