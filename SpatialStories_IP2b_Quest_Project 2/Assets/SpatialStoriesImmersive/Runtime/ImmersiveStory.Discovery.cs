using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialStories.Immersive
{
    public sealed partial class ImmersiveStory
    {
        [Tooltip("Show a direct cutout beside S, a sample collage and one short demonstration per trial.")]
        public bool enableCutoutDiscovery = true;
        readonly CutoutDiscoveryState discoveryState = new CutoutDiscoveryState();
        GameObject discoveryEntry, discoveryBadge, discoveryExample, discoveryExampleSubject, discoveryGhost;
        FloatingStoryItem discoveryHandle;
        Coroutine discoveryRoutine;
        Vector3 discoveryHandlePosition;
        Vector2 discoveryHandleSize, discoveryExampleSubjectSize;

        void InitializeCutoutDiscovery()
        {
            if (!enableCutoutDiscovery || subjects == null || subjects.Length == 0) return;
            var subject=subjects[0];
            var cutout=Resources.Load<Texture2D>(subject.resource);
            var sourcePhoto=OriginalPhotoTexture(subject.photoIndex);
            var examplePhoto=OriginalPhotoTexture(1); // Auckland: a different travel background.
            var sphere=toolGroup ? toolGroup.GetComponentsInChildren<SpatialToolButton>(true).FirstOrDefault(t=>t.action==11) : null;
            if (!cutout || !sourcePhoto || !examplePhoto || !sphere)
            {
                Debug.LogWarning("IP2b discovery needs the S sphere, the two photographs and Surfer.png. The regular source menu is still available.",this);
                return;
            }
            var spherePosition=contentRoot.InverseTransformPoint(sphere.transform.position);
            var photoPosition=spherePosition+new Vector3(-.33f,.005f,-.13f);
            discoveryHandlePosition=photoPosition+new Vector3(-.105f,0,-.075f);
            discoveryHandleSize=SubjectDimensions(subject,cutout,.17f);

            discoveryEntry=new GameObject("IP2b - direct cutout beside S");
            discoveryEntry.transform.SetParent(contentRoot,false);
            discoveryEntry.SetActive(false);
            var sourceMaterial=NewMaterial(photoShader,Color.white,sourcePhoto);
            sourceMaterial.mainTextureScale=subject.detail.size; sourceMaterial.mainTextureOffset=subject.detail.position;
            float sourceHeight=.25f;
            var photoSize=new Vector2(sourceHeight*sourcePhoto.width/sourcePhoto.height*subject.detail.width/subject.detail.height,sourceHeight);
            Quad("Source photo detail",discoveryEntry.transform,photoPosition,photoSize,sourceMaterial);
            // The actual subject sits over the left edge, half outside the card, and never animates.
            var name=CollageCanvas("Cutout entry name",discoveryEntry.transform,photoPosition+new Vector3(0,-.158f,-.01f),new Vector2(310,48));
            CollageText(name.transform,"PHOTO CUTOUTS",Vector2.zero,new Vector2(300,46),23);
            discoveryBadge=CollageCanvas("First-use NEW label",discoveryEntry.transform,photoPosition+new Vector3(0,.208f,-.01f),new Vector2(420,55));
            var badge=discoveryBadge.AddComponent<Image>(); badge.color=new Color(.08f,.26f,.23f,1); badge.raycastTarget=false;
            var badgeText=CollageText(discoveryBadge.transform,"NEW · PHOTO CUTOUTS",Vector2.zero,new Vector2(405,50),25);
            badgeText.color=new Color(.65f,1,.85f);
            discoveryBadge.SetActive(false);

            discoveryExample=new GameObject("IP2b - example collage, never user content");
            discoveryExample.transform.SetParent(contentRoot,false);
            discoveryExample.transform.localPosition=spherePosition+new Vector3(.25f,.51f,.17f);
            discoveryExample.SetActive(false);
            var exampleSize=new Vector2(.41f*examplePhoto.width/examplePhoto.height,.41f);
            Quad("Auckland background",discoveryExample.transform,Vector3.zero,exampleSize,NewMaterial(photoShader,Color.white,examplePhoto));
            var exampleName=CollageCanvas("Example label",discoveryExample.transform,new Vector3(0,.248f,0),new Vector2(340,55));
            CollageText(exampleName.transform,"EXAMPLE · REMIX A MEMORY",Vector2.zero,new Vector2(335,52),21);
            var words=CollageCanvas("Sample words",discoveryExample.transform,new Vector3(0,.059f,-.012f),new Vector2(220,72));
            var title=CollageText(words.transform,"WEEKEND",Vector2.zero,new Vector2(220,70),32);
            title.fontStyle=FontStyle.Bold; title.material=textDepthMaterial;
            var footer=CollageCanvas("Sample subtitle",discoveryExample.transform,new Vector3(0,-.165f,-.014f),new Vector2(220,48));
            var subtitle=CollageText(footer.transform,"A little escape",Vector2.zero,new Vector2(215,45),20);
            subtitle.material=textDepthMaterial;
            discoveryExampleSubjectSize=SubjectDimensions(subject,cutout,.20f);
            discoveryExampleSubject=Quad("Surfer in front of the words",discoveryExample.transform,new Vector3(.006f,-.023f,-.025f),
                discoveryExampleSubjectSize,SubjectMaterial(subject,cutout));

            // Only this non-interactive duplicate moves. It is not an overlay, a grab target, or a photo child.
            var ghostMaterial=SubjectMaterial(subject,cutout);
            ghostMaterial.color=new Color(1,1,1,.80f); ghostMaterial.SetFloat("_Cutoff",.4f);
            discoveryGhost=Quad("Demonstration ghost - no interaction",contentRoot,discoveryHandlePosition,discoveryHandleSize,ghostMaterial);
            discoveryGhost.SetActive(false);
            Record("cutout_discovery_available");
        }

        Texture OriginalPhotoTexture(int index)
        {
            if (mediaCards == null || index < 0 || index >= mediaCards.Length || !mediaCards[index]) return null;
            var photo=mediaCards[index].transform.Find("Original photograph");
            var surface=photo ? photo.GetComponent<Renderer>() : null;
            return surface && surface.sharedMaterial ? surface.sharedMaterial.mainTexture : null;
        }

        bool DiscoveryEntryVisible => enableCollage && enableCutoutDiscovery && discoveryEntry &&
            selected && !selected.deleted && stage==Stage.Edit && !transitioning && !sourcesOpen &&
            (!textPanel || !textPanel.activeSelf) && (!stickerPanel || !stickerPanel.activeSelf) &&
            (!adminPanel || !adminPanel.activeSelf);

        bool CanTakeDiscoverySubject => DiscoveryEntryVisible && !Holding &&
            !overlays.Any(i=>i && !i.deleted && IsOnSelected(i) && !i.placed) &&
            overlays.Count(i=>i && !i.deleted && IsOnSelected(i)) < maxOverlaysPerPhoto;

        void BeginCutoutDiscovery()
        {
            if (!DiscoveryEntryVisible || !discoveryState.TryStart()) return;
            RefreshCutoutDiscovery();
            Record("cutout_discovery_shown",selected.itemId);
            Hint("NEW: PHOTO CUTOUTS. Grip the surfer beside S.\nPlace it on your own photo to make a collage.");
            discoveryRoutine=StartCoroutine(PlayCutoutDiscovery());
        }

        IEnumerator PlayCutoutDiscovery()
        {
            Set(discoveryGhost,true);
            var start=discoveryHandlePosition;
            var end=contentRoot.InverseTransformPoint(discoveryExampleSubject.transform.position);
            var control=Vector3.Lerp(start,end,.6f)+new Vector3(.07f,.04f,-.05f);
            float elapsed=0;
            // 0.3 s lift, 1.3 s move, 0.4 s settle: one two-second example.
            while (elapsed<2f && discoveryState.IsAnimating && DiscoveryEntryVisible)
            {
                float lift=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.3f));
                float t=Mathf.SmoothStep(0,1,Mathf.Clamp01((elapsed-.3f)/1.3f));
                var lifted=start+new Vector3(0,.015f,-.045f)*lift;
                discoveryGhost.transform.localPosition=(1-t)*(1-t)*lifted+2*(1-t)*t*control+t*t*end;
                var size=Vector2.Lerp(discoveryHandleSize,discoveryExampleSubjectSize,t);
                discoveryGhost.transform.localScale=new Vector3(size.x,size.y,1);
                discoveryGhost.transform.localRotation=Quaternion.Euler(0,0,Mathf.Sin(t*Mathf.PI)*-8f);
                elapsed+=Time.unscaledDeltaTime;
                yield return null;
            }
            discoveryRoutine=null;
            discoveryState.FinishAnimation();
            Set(discoveryGhost,false);
            Set(discoveryExampleSubject,true);
            if (elapsed>=2f && DiscoveryEntryVisible && discoveryState.GuidanceVisible) Record("cutout_demo_completed");
            RefreshCutoutDiscovery();
        }

        void StopDiscoveryDemoOnAim()
        {
            if (!discoveryState.IsAnimating) return;
            StopCutoutDemo();
            Record("cutout_demo_stopped_on_aim");
        }

        void StopCutoutDemo()
        {
            if (discoveryRoutine!=null) { StopCoroutine(discoveryRoutine); discoveryRoutine=null; }
            discoveryState.FinishAnimation(); Set(discoveryGhost,false); Set(discoveryExampleSubject,true);
        }

        void DismissCutoutDiscovery(string reason)
        {
            if (!enableCollage || !enableCutoutDiscovery || !discoveryState.Consume()) return;
            StopCutoutDemo(); Set(discoveryBadge,false); Set(discoveryExample,false);
            Record("cutout_discovery_dismissed",reason);
            // The compact source card stays available; only the first-use guidance is dismissed.
            Hint("Grip an element to move it. Use T or S to add more.");
        }

        void RefreshCutoutDiscovery()
        {
            if (!enableCollage || !discoveryEntry) return;
            bool visible=DiscoveryEntryVisible;
            if (!visible && discoveryState.IsAnimating) StopCutoutDemo();
            Set(discoveryEntry,visible);
            Set(discoveryBadge,visible && discoveryState.GuidanceVisible);
            Set(discoveryExample,visible && discoveryState.GuidanceVisible);
            Set(discoveryExampleSubject,!discoveryState.IsAnimating);
            if (!discoveryHandle && CanTakeDiscoverySubject)
                discoveryHandle=CreateSourceHandle(subjects[0],discoveryHandlePosition,.17f,true);
            if (discoveryHandle && !discoveryHandle.Held)
            {
                Set(discoveryHandle.gameObject,visible);
                discoveryHandle.Grab.enabled=CanTakeDiscoverySubject;
            }
        }

        void ResetCutoutDiscovery()
        {
            StopCutoutDemo(); discoveryState.Reset();
            Set(discoveryEntry,false); Set(discoveryExample,false); Set(discoveryBadge,false);
            if (discoveryHandle && !discoveryHandle.Held) Set(discoveryHandle.gameObject,false);
        }
    }
}
