using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SpatialStories.Immersive.Tests
{
    public class PlacementFeedbackTests
    {
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        readonly List<GameObject> objects = new List<GameObject>();

        FloatingStoryItem Item(string name, bool photo = false)
        {
            var go = new GameObject(name); objects.Add(go);
            var item = go.AddComponent<FloatingStoryItem>();
            item.itemId = name; item.content = name; item.isPhoto = photo;
            item.size = photo ? new Vector2(.405f,.72f) : new Vector2(.17f,.24f);
            return item;
        }

        ImmersiveStory Story(params FloatingStoryItem[] photos)
        {
            var go = new GameObject("controller"); objects.Add(go);
            var story = go.AddComponent<ImmersiveStory>(); story.enableCollage = true; story.mediaCards = photos;
            typeof(ImmersiveStory).GetField("selected", Private).SetValue(story, photos[0]);
            var stage = typeof(ImmersiveStory).GetField("stage", Private);
            stage.SetValue(story, System.Enum.Parse(stage.FieldType, "Edit"));
            return story;
        }

        List<FloatingStoryItem> Layers(ImmersiveStory story) =>
            (List<FloatingStoryItem>)typeof(ImmersiveStory).GetField("overlays", Private).GetValue(story);

        bool Predict(ImmersiveStory story, FloatingStoryItem item, out FloatingStoryItem photo,
            out CollagePlacement.Placement placement)
        {
            var args = new object[] { item, null, null };
            bool valid = (bool)typeof(ImmersiveStory).GetMethod("TryFindCollagePlacement", Private).Invoke(story,args);
            photo = (FloatingStoryItem)args[1]; placement = (CollagePlacement.Placement)args[2];
            return valid;
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in objects) if (go) Object.DestroyImmediate(go);
            objects.Clear();
        }

        [TestCase(0)] [TestCase(45)] [TestCase(90)]
        public void PredictedFittedPoseMatchesTheActualDrop(float angle)
        {
            var photo = Item("target",true);
            photo.transform.SetPositionAndRotation(new Vector3(.4f,1.4f,1),Quaternion.Euler(0,32,0));
            photo.transform.localScale = Vector3.one * .8f;
            var story = Story(photo);
            var item = Item("cutout"); item.ownerPhoto = photo;
            item.transform.position = photo.transform.TransformPoint(new Vector3(.18f,.3f,-.10f));
            item.transform.rotation = photo.transform.rotation * Quaternion.Euler(0,0,angle);
            item.transform.localScale = Vector3.one * 3;
            Layers(story).Add(item);
            Assert.That(Predict(story,item,out var target,out var predicted),Is.True);
            Assert.That(target,Is.SameAs(photo)); Assert.That(predicted.fitted,Is.True);
            typeof(ImmersiveStory).GetMethod("PlaceCollageItem",Private).Invoke(story,new object[] { item });
            Assert.That(Vector3.Distance(item.transform.localPosition,predicted.position),Is.LessThan(.0001f));
            Assert.That(Vector3.Distance(item.transform.localScale,predicted.scale),Is.LessThan(.0001f));
            Assert.That(Quaternion.Angle(item.transform.localRotation,predicted.rotation),Is.LessThan(.01f));
        }

        [Test]
        public void LookingAtAPreviewDoesNotChangeTheHeldItemOrPhoto()
        {
            var photo = Item("target",true); var story = Story(photo); var item = Item("cutout");
            item.transform.position = new Vector3(.1f,.1f,-.1f);
            item.transform.rotation = Quaternion.Euler(12,20,35);
            var position = item.transform.position; var rotation = item.transform.rotation; var scale = item.transform.localScale;
            for (int i=0; i<20; i++) Assert.That(Predict(story,item,out _,out _),Is.True);
            Assert.That(item.transform.position,Is.EqualTo(position));
            Assert.That(item.transform.rotation,Is.EqualTo(rotation));
            Assert.That(item.transform.localScale,Is.EqualTo(scale));
            Assert.That(item.ownerPhoto,Is.Null); Assert.That(item.placed,Is.False);
            Assert.That(item.transform.parent,Is.Null); Assert.That(photo.transform.childCount,Is.Zero);
        }

        [Test]
        public void FullPhotoIsSkippedButItsExistingItemCanBeRepositioned()
        {
            var full = Item("full",true); var available = Item("available",true);
            available.transform.position = new Vector3(0,0,.12f);
            var story = Story(full,available); story.maxOverlaysPerPhoto = 1;
            var existing = Item("existing"); existing.ownerPhoto = full; existing.placed = true;
            var incoming = Item("incoming"); incoming.transform.position = new Vector3(0,0,-.05f);
            Layers(story).Add(existing); Layers(story).Add(incoming);
            Assert.That(Predict(story,incoming,out var target,out _),Is.True);
            Assert.That(target,Is.SameAs(available));
            Assert.That(Predict(story,existing,out target,out _),Is.True);
            Assert.That(target,Is.SameAs(full));
        }

        [Test]
        public void BinTakesPriorityOverAPlacementPreview()
        {
            var photo = Item("target",true); var story = Story(photo); var item = Item("cutout");
            var bin = new GameObject("bin"); objects.Add(bin);
            story.trashVolume = bin.AddComponent<BoxCollider>();
            Assert.That(CollagePlacement.TryProject(item,photo,out _,out _),Is.True);
            Assert.That(Predict(story,item,out var target,out _),Is.False); Assert.That(target,Is.Null);
        }

        [Test]
        public void MovingABackLayerPreviewsItsExistingDepth()
        {
            var photo = Item("target",true); var story = Story(photo);
            var back = Item("back"); var front = Item("front");
            back.ownerPhoto = front.ownerPhoto = photo; back.placed = front.placed = true;
            back.collageLayer = 0; front.collageLayer = 1;
            Layers(story).Add(back); Layers(story).Add(front);
            Assert.That(Predict(story,back,out _,out var predicted),Is.True);
            Assert.That(predicted.position.z,Is.EqualTo(-.045f).Within(.0001f));
        }

        [Test]
        public void TransferringToAnotherPhotoPreviewsInFrontOfItsLayers()
        {
            var photo = Item("target",true); var other = Item("source",true);
            other.transform.position = Vector3.right * 3;
            var story = Story(photo,other); var incoming = Item("incoming"); var existing = Item("text");
            incoming.ownerPhoto = other; incoming.placed = true;
            existing.ownerPhoto = photo; existing.placed = true; existing.collageLayer = 9;
            Layers(story).Add(incoming); Layers(story).Add(existing);
            Assert.That(Predict(story,incoming,out var target,out var predicted),Is.True);
            Assert.That(target,Is.SameAs(photo));
            Assert.That(predicted.position.z,Is.EqualTo(-.059f).Within(.0001f));
        }
    }
}
