using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SpatialStories.Immersive.Tests
{
    public class CutoutSelectionTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly List<GameObject> objects = new List<GameObject>();

        GameObject Object(string name, params System.Type[] components)
        {
            var go = new GameObject(name, components); objects.Add(go); return go;
        }

        FloatingStoryItem Item(string name, bool photo = false)
        {
            var go = Object(name, typeof(RectTransform));
            var item = go.AddComponent<FloatingStoryItem>();
            item.itemId = item.content = name;
            item.isPhoto = photo; item.isCutout = !photo;
            item.size = new Vector2(.17f, .24f);
            var collider = go.AddComponent<BoxCollider>();
            collider.size = new Vector3(.19f, .26f, .022f);
            item.Grab.colliders.Clear(); item.Grab.colliders.Add(collider);
            return item;
        }

        ImmersiveStory Story(out FloatingStoryItem photo)
        {
            photo = Item("background", true);
            var story = Object("controller").AddComponent<ImmersiveStory>();
            story.enableCollage = true; story.mediaCards = new[] { photo };
            story.contentRoot = Object("content").transform;
            story.head = Object("head").AddComponent<Camera>(); story.head.enabled = false;
            story.hintLabel = Object("hint", typeof(RectTransform)).AddComponent<Text>();
            Set(story, "selected", photo); SetStage(story, "Edit");
            return story;
        }

        static object Get(ImmersiveStory story, string name) =>
            typeof(ImmersiveStory).GetField(name, Private).GetValue(story);
        static void Set(ImmersiveStory story, string name, object value) =>
            typeof(ImmersiveStory).GetField(name, Private).SetValue(story, value);
        static void SetStage(ImmersiveStory story, string name)
        {
            var field = typeof(ImmersiveStory).GetField("stage", Private);
            field.SetValue(story, System.Enum.Parse(field.FieldType, name));
        }
        static object Call(ImmersiveStory story, string name, FloatingStoryItem item) =>
            typeof(ImmersiveStory).GetMethod(name, Private).Invoke(story, new object[] { item });
        static List<FloatingStoryItem> Items(ImmersiveStory story, string name) =>
            (List<FloatingStoryItem>)Get(story, name);

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in objects) if (go) UnityEngine.Object.DestroyImmediate(go);
            objects.Clear();
        }

        [Test]
        public void RepeatedSetupKeepsOneClickTargetAndTheOriginalGrabGeometry()
        {
            var story = Story(out _); var item = Item("subject"); item.controller = story;
            item.gameObject.SetActive(false); // Matches CreateSourceHandle setup order.
            item.transform.SetParent(story.contentRoot, false);
            item.transform.localPosition = new Vector3(.3f, 1.1f, .7f);
            item.transform.localRotation = Quaternion.Euler(0, 25, 14);
            item.transform.localScale = Vector3.one * 1.4f;
            var position = item.transform.localPosition; var rotation = item.transform.localRotation;
            var scale = item.transform.localScale; var dimensions = item.size;
            var collider = item.GetComponent<BoxCollider>(); var boxSize = collider.size;
            collider.center = new Vector3(.01f, -.02f, .003f); var center = collider.center;
            Call(story, "EnsureCutoutClickFace", item); Call(story, "EnsureCutoutClickFace", item);
            Assert.That(item.GetComponents<Canvas>().Length, Is.EqualTo(1));
            Assert.That(item.GetComponents<Image>().Length, Is.EqualTo(1));
            Assert.That(item.GetComponents<Button>().Length, Is.EqualTo(1));
            Assert.That(item.GetComponents<TrackedDeviceGraphicRaycaster>().Length, Is.EqualTo(1));
            Assert.That(item.GetComponents<XRBaseInteractable>().Length, Is.EqualTo(1));
            Assert.That(item.GetComponents<BoxCollider>(), Is.EqualTo(new[] { collider }));
            Assert.That(item.Grab.colliders, Is.EqualTo(new[] { collider }));
            Assert.That(item.GetComponent<Button>().gameObject, Is.SameAs(collider.gameObject));
            Assert.That(item.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.WorldSpace));
            Assert.That(item.GetComponent<Image>().color.a, Is.Zero);
            Assert.That(item.GetComponent<Image>().raycastTarget, Is.True);
            Assert.That(item.GetComponent<RectTransform>().sizeDelta, Is.EqualTo(dimensions));
            Assert.That(item.transform.parent, Is.SameAs(story.contentRoot));
            Assert.That(item.transform.localPosition, Is.EqualTo(position));
            Assert.That(item.transform.localRotation, Is.EqualTo(rotation));
            Assert.That(item.transform.localScale, Is.EqualTo(scale));
            Assert.That(item.size, Is.EqualTo(dimensions));
            Assert.That(collider.size, Is.EqualTo(boxSize)); Assert.That(collider.center, Is.EqualTo(center));
        }

        [Test]
        public void ClickingASourceDoesNotLiftItOrConsumeDiscovery()
        {
            var story = Story(out _); var item = Item("surfer"); item.controller = story;
            item.isSourceHandle = item.isDiscoveryHandle = true; item.sourcePhotoId = "source_photo";
            item.collageLayer = 4; item.transform.SetParent(story.contentRoot, false);
            item.transform.localPosition = new Vector3(.2f, 1.3f, .9f);
            var position = item.transform.localPosition; var rotation = item.transform.localRotation;
            var scale = item.transform.localScale;
            Items(story, "sourceHandles").Add(item);
            var discovery = (CutoutDiscoveryState)Get(story, "discoveryState"); discovery.TryStart();
            Assert.That(Call(story, "TrySelectCutout", item), Is.True);
            Assert.That(item.isSourceHandle && item.isDiscoveryHandle, Is.True);
            Assert.That(item.ownerPhoto, Is.Null); Assert.That(item.placed, Is.False);
            Assert.That(item.collageLayer, Is.EqualTo(4)); Assert.That(item.itemId, Is.EqualTo("surfer"));
            Assert.That(Items(story, "overlays"), Is.Empty);
            Assert.That(Items(story, "sourceHandles"), Is.EqualTo(new[] { item }));
            Assert.That(discovery.Consumed, Is.False); Assert.That(discovery.GuidanceVisible, Is.True);
            Assert.That(item.transform.parent, Is.SameAs(story.contentRoot));
            Assert.That(item.transform.localPosition, Is.EqualTo(position));
            Assert.That(item.transform.localRotation, Is.EqualTo(rotation));
            Assert.That(item.transform.localScale, Is.EqualTo(scale));
            StringAssert.Contains("Grip", story.hintLabel.text);
        }

        [Test]
        public void ClickingAPlacedSubjectSelectsItsLayerWithoutMovingOrRestackingIt()
        {
            var story = Story(out var photo); var item = Item("surfer"); item.controller = story;
            item.ownerPhoto = photo; item.placed = true; item.collageLayer = 7;
            item.transform.SetParent(photo.transform, false);
            item.transform.localPosition = new Vector3(.02f, -.03f, -.08f);
            item.transform.localRotation = Quaternion.Euler(0, 0, 22);
            item.transform.localScale = Vector3.one * .8f;
            var position = item.transform.localPosition; var rotation = item.transform.localRotation;
            var scale = item.transform.localScale;
            Items(story, "overlays").Add(item);
            Assert.That(Call(story, "TrySelectCutout", item), Is.True);
            Assert.That(Get(story, "activeLayer"), Is.SameAs(item));
            Assert.That(item.persistentHighlight, Is.True);
            Assert.That(item.ownerPhoto, Is.SameAs(photo)); Assert.That(item.collageLayer, Is.EqualTo(7));
            Assert.That(item.transform.parent, Is.SameAs(photo.transform));
            Assert.That(item.transform.localPosition, Is.EqualTo(position));
            Assert.That(item.transform.localRotation, Is.EqualTo(rotation));
            Assert.That(item.transform.localScale, Is.EqualTo(scale));
        }

        [TestCase("Review")]
        [TestCase("Deleted")]
        public void UnavailableSubjectsDoNotChangeTheActiveLayer(string condition)
        {
            var story = Story(out var photo); var item = Item("surfer"); item.controller = story;
            item.ownerPhoto = photo; item.placed = true; item.collageLayer = 3;
            Items(story, "overlays").Add(item);
            var prior = Item("prior"); Set(story, "activeLayer", prior);
            if (condition == "Review") SetStage(story, "Review");
            else item.deleted = true;
            Assert.That(Call(story, "TrySelectCutout", item), Is.False);
            Assert.That(Get(story, "activeLayer"), Is.SameAs(prior));
            Assert.That(item.collageLayer, Is.EqualTo(3));
        }
    }
}
