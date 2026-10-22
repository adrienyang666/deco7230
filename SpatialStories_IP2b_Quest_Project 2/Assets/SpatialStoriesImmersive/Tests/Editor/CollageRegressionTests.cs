using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace SpatialStories.Immersive.Tests
{
    public class CollageRegressionTests
    {
        readonly List<GameObject> objects = new List<GameObject>();
        const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        FloatingStoryItem Item(string name)
        {
            var go=new GameObject(name); objects.Add(go); return go.AddComponent<FloatingStoryItem>();
        }

        [TearDown]
        public void Cleanup()
        {
            foreach (var go in objects) if (go) Object.DestroyImmediate(go);
            objects.Clear();
        }

        [TestCase(0)] [TestCase(45)] [TestCase(90)] [TestCase(157)]
        public void OversizedRotatedSubjectFitsWithoutClipping(float angle)
        {
            var photo=new Vector2(.405f,.72f);
            var result=CollagePlacement.Fit(photo,new Vector2(.4f,.5f),new Vector3(1,1,0),Vector3.one*2,angle);
            var extents=CollagePlacement.HalfExtents(new Vector2(.4f,.5f),result.scale,angle);
            Assert.That(Mathf.Abs(result.position.x)+extents.x,Is.LessThanOrEqualTo(photo.x*.5f+.0001f));
            Assert.That(Mathf.Abs(result.position.y)+extents.y,Is.LessThanOrEqualTo(photo.y*.5f+.0001f));
            Assert.That(result.scale.x,Is.EqualTo(result.scale.y).Within(.0001f));
            Assert.That(result.fitted,Is.True);
        }

        [Test]
        public void TransferPreservesWorldSizeAcrossRotatedScaledPhotos()
        {
            var source=Item("source"); source.transform.localScale=Vector3.one*1.45f;
            var target=Item("target"); target.size=new Vector2(.6f,.8f);
            target.transform.SetPositionAndRotation(new Vector3(1,1.5f,2),Quaternion.Euler(0,32,0));
            target.transform.localScale=Vector3.one*.8f;
            var item=Item("subject"); item.size=new Vector2(.08f,.12f);
            item.transform.SetParent(source.transform,false);
            item.transform.position=target.transform.TransformPoint(new Vector3(.02f,.03f,-.08f));
            item.transform.rotation=target.transform.rotation*Quaternion.Euler(0,0,25);
            float worldWidth=item.size.x*item.transform.lossyScale.x;
            Assert.That(CollagePlacement.TryProject(item,target,out var result,out _),Is.True);
            item.transform.SetParent(target.transform,false); item.transform.localPosition=result.position;
            item.transform.localRotation=result.rotation; item.transform.localScale=result.scale;
            Assert.That(item.size.x*item.transform.lossyScale.x,Is.EqualTo(worldWidth).Within(.0001f));
            Assert.That(Quaternion.Angle(item.transform.localRotation,Quaternion.Euler(0,0,25)),Is.LessThan(.01f));
        }

        [Test]
        public void DistantAndDeletedPhotosCannotAcceptDrops()
        {
            var photo=Item("photo"); photo.size=new Vector2(.405f,.72f);
            var subject=Item("subject"); subject.transform.position=new Vector3(0,0,-.8f);
            Assert.That(CollagePlacement.TryProject(subject,photo,out _,out _),Is.False);
            subject.transform.position=Vector3.zero; photo.deleted=true;
            Assert.That(CollagePlacement.TryProject(subject,photo,out _,out _),Is.False);
        }

        [Test]
        public void LayerMoveAndUndoRestackingKeepIndependentPhotoOrder()
        {
            var go=new GameObject("controller"); objects.Add(go);
            var story=go.AddComponent<ImmersiveStory>(); story.enableCollage=true;
            var photo=Item("photo"); var otherPhoto=Item("other photo");
            story.mediaCards=new[] { photo,otherPhoto };
            typeof(ImmersiveStory).GetField("selected",Private).SetValue(story,photo);
            var list=(List<FloatingStoryItem>)typeof(ImmersiveStory).GetField("overlays",Private).GetValue(story);
            var text=Item("text"); var subject=Item("subject"); var other=Item("unrelated");
            foreach (var item in new[] { text,subject,other })
            { item.placed=true; list.Add(item); }
            text.ownerPhoto=subject.ownerPhoto=photo; other.ownerPhoto=otherPhoto;
            text.transform.SetParent(photo.transform,false); subject.transform.SetParent(photo.transform,false);
            other.transform.SetParent(otherPhoto.transform,false);
            text.collageLayer=0; subject.collageLayer=1; other.collageLayer=0;
            typeof(ImmersiveStory).GetField("activeLayer",Private).SetValue(story,subject);
            Call(story,"Restack",photo); Call(story,"Restack",otherPhoto);
            float unrelatedDepth=other.transform.localPosition.z;
            Call(story,"MoveLayer",-1);
            Assert.That(subject.transform.localPosition.z,Is.GreaterThan(text.transform.localPosition.z));
            Assert.That(other.transform.localPosition.z,Is.EqualTo(unrelatedDepth));
            text.deleted=true; Call(story,"Restack",photo);
            text.deleted=false; Call(story,"Restack",photo);
            Assert.That(subject.collageLayer,Is.LessThan(text.collageLayer));
            Assert.That(subject.transform.localPosition.z,Is.GreaterThan(text.transform.localPosition.z));
        }

        static void Call(ImmersiveStory story,string name,object argument) =>
            typeof(ImmersiveStory).GetMethod(name,Private).Invoke(story,new[] { argument });
    }
}
