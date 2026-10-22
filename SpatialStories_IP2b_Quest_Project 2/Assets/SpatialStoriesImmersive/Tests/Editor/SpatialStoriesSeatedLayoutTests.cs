// Candidate EditMode regression tests. Not executed or installed in the project.
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SpatialStories.Immersive.Tests
{
    public class SpatialStoriesSeatedLayoutTests
    {
        const string QuestPath = "Assets/SpatialStoriesImmersive/Scenes/SpatialStories_IP2b_Quest.unity";
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        Scene scene;
        ImmersiveStory story;
        Transform floor;

        [SetUp]
        public void OpenActualQuestLayoutWithoutReplacingTheCurrentScene()
        {
            if (SceneManager.GetSceneByPath(QuestPath).isLoaded)
                Assert.Ignore("Close the Quest scene before this fixture; tests must not change an existing editor scene.");
            scene = EditorSceneManager.OpenScene(QuestPath, OpenSceneMode.Additive);
            story = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<ImmersiveStory>(true)).Single();
            // In the saved scene, the environment and contentRoot are siblings under story.transform.
            floor = story.transform.Find("Quiet gallery environment - editable/Floor");
            Assert.That(floor, Is.Not.Null, "The actual scene's fixed floor is required.");
            Assert.That(story.head, Is.Not.Null);
            Assert.That(story.contentRoot, Is.Not.Null);
        }

        [TearDown]
        public void CloseOnlyTheTestSceneWithoutSaving()
        {
            if (scene.IsValid() && scene.isLoaded) EditorSceneManager.CloseScene(scene, true);
            scene = default;
        }

        bool Apply(Vector3 eye, Vector3 forward, bool quest)
        {
            var method = typeof(ImmersiveStory).GetMethod("ApplyWorkspacePose", Private);
            Assert.That(method, Is.Not.Null, "The workspace geometry helper must exist before these tests are installed.");
            return (bool)method.Invoke(story, new object[] { eye, forward, quest });
        }

        [TestCase(.757f)]
        [TestCase(1.65f)]
        public void SeatedAndStandingUsersKeepCoreTargetsAboveTheFixedFloor(float eyeY)
        {
            var cameraPosition = story.head.transform.position;
            var cameraRotation = story.head.transform.rotation;
            var floorMatrix = floor.localToWorldMatrix;
            var floorBounds = MeshBounds(floor.GetComponent<Renderer>());
            var eye = new Vector3(.7f, eyeY, -.4f);
            var direction = Quaternion.Euler(0, 34, 0) * Vector3.forward;
            Assert.That(Apply(eye, direction, true), Is.True);

            var publish = story.publishObject.transform.Find("Physical sphere").GetComponent<Renderer>();
            Assert.That(MeshBounds(publish).min.y, Is.GreaterThan(floorBounds.max.y + .01f), "Publish must not be buried in the floor.");
            var bin = (BoxCollider)story.trashVolume;
            Assert.That(TransformedBounds(bin.transform, new Bounds(bin.center, bin.size)).min.y,
                Is.GreaterThan(floorBounds.max.y + .01f), "The bin's release target must remain above the floor.");

            // Place an actual photograph at the existing inspection anchor using its default inspection size.
            var photo = story.mediaCards[0];
            photo.transform.SetParent(story.contentRoot, false);
            photo.transform.SetPositionAndRotation(story.focusAnchor.position, story.focusAnchor.rotation);
            photo.transform.localScale = story.albumScales[0] * story.inspectScale;
            var photograph = photo.transform.Find("Original photograph").GetComponent<Renderer>();
            Assert.That(MeshBounds(photograph).min.y, Is.GreaterThan(floorBounds.max.y + .01f), "A default focused photo must not enter the floor.");
            Assert.That(story.focusAnchor.position.y, Is.GreaterThan(floorBounds.max.y));
            Assert.That(story.focusAnchor.position.y, Is.InRange(eye.y - .2f, eye.y + .02f), "Keeping everything above the floor must not put a seated user's editing target above their eyes.");
            var scale = story.contentRoot.localScale;
            Assert.That(scale.x, Is.GreaterThan(0));
            Assert.That(scale.x, Is.EqualTo(scale.y).Within(.00001f));
            Assert.That(scale.x, Is.EqualTo(scale.z).Within(.00001f), "Scaling must preserve the photos' aspect and interaction geometry.");
            Assert.That(Vector3.Distance(story.head.transform.position, cameraPosition), Is.LessThan(.00001f), "Layout must never move the tracked camera.");
            Assert.That(Quaternion.Angle(story.head.transform.rotation, cameraRotation), Is.LessThan(.001f));
            Assert.That(floor.localToWorldMatrix, Is.EqualTo(floorMatrix), "The fixed environment must not be scaled with the workspace.");
            Assert.That(Vector3.Dot(story.contentRoot.up, Vector3.up), Is.GreaterThan(.99999f), "Workspace orientation must not introduce floor tilt.");
        }

        [TestCase(.757f)]
        [TestCase(1.65f)]
        public void RepeatedRecenteringAndChangingPostureDoNotAccumulateScale(float eyeY)
        {
            var direction = Quaternion.Euler(0, -27, 0) * Vector3.forward;
            var eye = new Vector3(-.3f, eyeY, .4f);
            Assert.That(Apply(eye, direction, true), Is.True);
            var scale = story.contentRoot.localScale;
            var focus = story.focusAnchor.position;
            var publish = MeshBounds(story.publishObject.transform.Find("Physical sphere").GetComponent<Renderer>());
            for (int i = 0; i < 4; i++)
            {
                Assert.That(Apply(new Vector3(0, 1.3f, 0), Vector3.forward, true), Is.True);
                Assert.That(Apply(eye, direction, true), Is.True);
            }
            Assert.That(Vector3.Distance(story.contentRoot.localScale, scale), Is.LessThan(.00001f));
            Assert.That(Vector3.Distance(story.focusAnchor.position, focus), Is.LessThan(.00001f));
            var after = MeshBounds(story.publishObject.transform.Find("Physical sphere").GetComponent<Renderer>());
            Assert.That(Vector3.Distance(after.center, publish.center), Is.LessThan(.00001f));
            Assert.That(Vector3.Distance(after.size, publish.size), Is.LessThan(.00001f));
        }

        [TestCase(.757f)]
        [TestCase(1.65f)]
        public void SimulatorReturnsToTheOriginalUnscaledWorkspace(float eyeY)
        {
            var cameraPosition = story.head.transform.position;
            // First apply a seated Quest layout, then verify that a desktop layout is restored absolutely.
            Assert.That(Apply(new Vector3(0, .757f, 0), Vector3.forward, true), Is.True);
            var eye = new Vector3(.4f, eyeY, -.2f);
            Assert.That(Apply(eye, Vector3.forward, false), Is.True);
            Assert.That(Vector3.Distance(story.contentRoot.localScale, Vector3.one), Is.LessThan(.00001f));
            Assert.That(story.contentRoot.position.x, Is.EqualTo(eye.x).Within(.00001f));
            Assert.That(story.contentRoot.position.z, Is.EqualTo(eye.z).Within(.00001f));
            Assert.That(story.contentRoot.position.y, Is.EqualTo(eye.y - 1.65f).Within(.00001f));
            Assert.That(Vector3.Distance(story.head.transform.position, cameraPosition), Is.LessThan(.00001f));
        }

        // MeshFilter bounds remain meaningful for initially inactive Publish and photo renderers in EditMode.
        static Bounds MeshBounds(Renderer renderer) =>
            TransformedBounds(renderer.transform, renderer.GetComponent<MeshFilter>().sharedMesh.bounds);

        static Bounds TransformedBounds(Transform transform, Bounds local)
        {
            var result = new Bounds(transform.TransformPoint(local.min), Vector3.zero);
            for (int x = 0; x < 2; x++)
                for (int y = 0; y < 2; y++)
                    for (int z = 0; z < 2; z++)
                        result.Encapsulate(transform.TransformPoint(new Vector3(
                            x == 0 ? local.min.x : local.max.x,
                            y == 0 ? local.min.y : local.max.y,
                            z == 0 ? local.min.z : local.max.z)));
            return result;
        }
    }
}
