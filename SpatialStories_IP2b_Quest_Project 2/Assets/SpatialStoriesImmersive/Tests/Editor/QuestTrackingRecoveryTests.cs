// EditMode regression: tracking recovery changes the workspace while preserving content.
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpatialStories.Immersive.Tests
{
    public class QuestTrackingRecoveryTests
    {
        const string QuestPath = "Assets/SpatialStoriesImmersive/Scenes/SpatialStories_IP2b_Quest.unity";
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        Scene scene;
        ImmersiveStory story;
        readonly Vector3 baselineEye = new Vector3(0, .757f, 0);
        readonly Vector3 recoveredEye = new Vector3(.8f, 1.2f, -.4f);
        readonly Quaternion recoveredRotation = Quaternion.Euler(0, 35, 0);

        [SetUp]
        public void OpenOnlyAnAdditiveCopyOfTheActualQuestScene()
        {
            if (SceneManager.GetSceneByPath(QuestPath).isLoaded)
                Assert.Ignore("Close the Quest scene first; never mutate an already-open scene in this fixture.");
            scene = EditorSceneManager.OpenScene(QuestPath, OpenSceneMode.Additive);
            story = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ImmersiveStory>(true)).Single();
            Assert.That(story.head, Is.Not.Null);
            Assert.That(story.contentRoot, Is.Not.Null);
        }

        [TearDown]
        public void CloseTheTestSceneWithoutSaving()
        {
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            scene = default;
        }

        object Call(string name, params object[] args)
        {
            var method = typeof(ImmersiveStory).GetMethod(name, Private);
            Assert.That(method, Is.Not.Null, "Install the production recovery interface before running this fixture.");
            return method.Invoke(story, args);
        }
        object Field(string name) => typeof(ImmersiveStory).GetField(name, Private).GetValue(story);
        void SetField(string name, object value) => typeof(ImmersiveStory).GetField(name, Private).SetValue(story, value);
        void AlignBaseline() => Assert.That((bool)Call("AlignWorkspacePose", baselineEye, Vector3.forward, "test_initial_alignment"), Is.True);
        void Request(string reason, float time) => Call("RequestQuestRecovery", reason, time);
        bool Tick(float time, bool ready = true, bool busy = false) =>
            (bool)Call("TickQuestRecovery", ready, busy, recoveredEye, recoveredRotation, time);
        bool Sample(float time, Vector3 eye, Quaternion rotation) =>
            (bool)Call("TickQuestRecovery", true, false, eye, rotation, time);
        void AssertRootUnchanged(Matrix4x4 expected) => Assert.That(story.contentRoot.localToWorldMatrix, Is.EqualTo(expected));

        [Test]
        public void RecoveryMovesOnlyTheWorkspaceAndPreservesTheActualComposition()
        {
            AlignBaseline();
            var stage = typeof(ImmersiveStory).GetField("stage", Private);
            stage.SetValue(story, System.Enum.Parse(stage.FieldType, "Edit"));
            var photo = story.mediaCards[0];
            SetField("selected", photo);
            var caption = Object.Instantiate(story.textTemplate, photo.transform);
            caption.controller = story;
            caption.isText = true; caption.placed = true; caption.ownerPhoto = photo;
            caption.collageLayer = 4; caption.content = "A real Story caption";
            caption.transform.localPosition = new Vector3(.03f, .06f, -.045f);
            caption.transform.localRotation = Quaternion.Euler(0, 0, 17);
            caption.transform.localScale = Vector3.one * .8f;
            ((System.Collections.Generic.List<FloatingStoryItem>)Field("overlays")).Add(caption);
            SetField("activeLayer", caption);
            var captionPose = caption.transform.localToWorldMatrix;
            var localPosition = caption.transform.localPosition;
            var localRotation = caption.transform.localRotation;
            var localScale = caption.transform.localScale;
            var photoLocal = photo.transform.localPosition;
            var head = story.head.transform.localToWorldMatrix;
            var rig = story.head.transform.root.localToWorldMatrix;
            var floor = story.transform.Find("Quiet gallery environment - editable/Floor");
            var floorPose = floor.localToWorldMatrix;
            var before = story.contentRoot.localToWorldMatrix;

            Request("presence_return", 10f);
            Assert.That(Tick(10.1f), Is.False);
            Assert.That(Tick(11f), Is.True);
            Assert.That(story.contentRoot.localToWorldMatrix, Is.Not.EqualTo(before));
            Assert.That(story.head.transform.localToWorldMatrix, Is.EqualTo(head));
            Assert.That(story.head.transform.root.localToWorldMatrix, Is.EqualTo(rig));
            Assert.That(floor.localToWorldMatrix, Is.EqualTo(floorPose));
            Assert.That(story.StageName, Is.EqualTo("Edit"));
            Assert.That(Field("selected"), Is.SameAs(photo));
            Assert.That(Field("activeLayer"), Is.SameAs(caption));
            Assert.That(caption.ownerPhoto, Is.SameAs(photo));
            Assert.That(caption.collageLayer, Is.EqualTo(4));
            Assert.That(caption.content, Is.EqualTo("A real Story caption"));
            Assert.That(caption.placed, Is.True); Assert.That(caption.deleted, Is.False);
            Assert.That(caption.transform.parent, Is.SameAs(photo.transform));
            Assert.That(caption.transform.localPosition, Is.EqualTo(localPosition));
            Assert.That(caption.transform.localRotation, Is.EqualTo(localRotation));
            Assert.That(caption.transform.localScale, Is.EqualTo(localScale));
            Assert.That(photo.transform.localPosition, Is.EqualTo(photoLocal));
            Assert.That(caption.transform.localToWorldMatrix, Is.Not.EqualTo(captionPose), "Content should travel with the workspace, not remain at its old world pose.");
            Assert.That(Vector2.Distance(new Vector2(story.contentRoot.position.x, story.contentRoot.position.z),
                new Vector2(recoveredEye.x, recoveredEye.z)), Is.LessThan(.00001f));
        }

        [Test]
        public void AClusterOfRecoveryEventsCommitsOnceAndALaterCycleCanRecoverAgain()
        {
            AlignBaseline();
            Request("application_resumed", 10f);
            Request("presence_return", 10.1f);
            Request("tracking_origin_updated", 10.2f);
            int commits = 0;
            if (Tick(10.3f)) commits++;
            if (Tick(11.2f)) commits++;
            var recovered = story.contentRoot.localToWorldMatrix;
            for (int i = 0; i < 8; i++) if (Tick(11.3f + i * .3f)) commits++;
            Assert.That(commits, Is.EqualTo(1));
            AssertRootUnchanged(recovered);

            Request("presence_return", 20f);
            var laterEye = new Vector3(-.5f, 1.4f, .7f);
            if (Sample(20.1f, laterEye, Quaternion.identity)) commits++;
            if (Sample(21f, laterEye, Quaternion.identity)) commits++;
            Assert.That(commits, Is.EqualTo(2));
            Assert.That(story.contentRoot.localToWorldMatrix, Is.Not.EqualTo(recovered));
        }

        [Test]
        public void PausedUntrackedOrBusyFramesCannotMoveTheWorkspace()
        {
            AlignBaseline();
            Request("tracking_origin_updated", 10f);
            var before = story.contentRoot.localToWorldMatrix;
            Call("OnApplicationPause", true);
            Assert.That(Tick(11f), Is.False); AssertRootUnchanged(before);
            Call("OnApplicationPause", false);
            // Match the controlled Tick clock after the callback's real-time timestamp.
            Request("controlled_resume", 11.1f);
            Assert.That(Tick(12f, ready: false), Is.False); AssertRootUnchanged(before);
            Assert.That(Tick(13f, busy: true), Is.False); AssertRootUnchanged(before);
            Assert.That(Tick(13.1f), Is.False); AssertRootUnchanged(before);
            Assert.That(Tick(14f), Is.True);
            Assert.That(story.contentRoot.localToWorldMatrix, Is.Not.EqualTo(before));
        }

        [Test]
        public void OrdinaryHeadMovementWithoutARecoveryEventNeverMakesTheGalleryFollow()
        {
            AlignBaseline();
            var before = story.contentRoot.localToWorldMatrix;
            for (int i = 0; i < 30; i++)
            {
                var eye = new Vector3(i * .03f, .757f + Mathf.Sin(i) * .1f, i * -.02f);
                Assert.That(Sample(10f + i * .1f, eye, Quaternion.Euler(0, i * 9, 0)), Is.False);
            }
            AssertRootUnchanged(before);
        }

        [Test]
        public void TheWorkspaceWaitsThroughTheDelayAndContinuingPoseChanges()
        {
            AlignBaseline();
            Request("presence_return", 10f);
            var before = story.contentRoot.localToWorldMatrix;
            Assert.That(Sample(10.1f, recoveredEye, Quaternion.identity), Is.False);
            Assert.That(Sample(10.4f, recoveredEye, Quaternion.identity), Is.False);
            AssertRootUnchanged(before); // Not enough time after the event.
            var settledEye = recoveredEye + Vector3.right * .2f;
            var settledRotation = Quaternion.Euler(0, 30, 0);
            Assert.That(Sample(10.9f, settledEye, settledRotation), Is.False);
            Assert.That(Sample(11.1f, settledEye, settledRotation), Is.False);
            AssertRootUnchanged(before); // A substantial position/rotation change restarted stability.
            Assert.That(Sample(11.6f, settledEye, settledRotation), Is.True);
            var after = story.contentRoot.localToWorldMatrix;
            Assert.That(after, Is.Not.EqualTo(before));
            Assert.That(Sample(12.5f, settledEye, settledRotation), Is.False);
            AssertRootUnchanged(after);
        }

        [Test]
        public void AnInitialResumeCallbackBeforeTheFirstAlignmentDoesNotStartRecovery()
        {
            var before = story.contentRoot.localToWorldMatrix;
            Call("OnApplicationPause", false);
            Assert.That(Tick(10f), Is.False);
            Assert.That(Tick(12f), Is.False);
            AssertRootUnchanged(before);
            AlignBaseline();
            var aligned = story.contentRoot.localToWorldMatrix;
            Call("OnApplicationPause", false); // An unpaired initial callback is harmless even after alignment.
            Assert.That(Tick(13f), Is.False);
            Assert.That(Tick(15f), Is.False);
            AssertRootUnchanged(aligned);
        }
    }
}
