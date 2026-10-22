using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpatialStories.Immersive
{
    public sealed partial class ImmersiveStory
    {
        void CaptureFlatPreview()
        {
            if (!selected || !enableCollage) return;
            flatPreviewReady = false;
            Set(flatPreview, false);
            var layers = new Dictionary<GameObject,int>();
            var visibility = new Dictionary<GameObject,bool>();
            var canvasCameras = new Dictionary<Canvas,Camera>();
            try
            {
                // This project's saved GraphicsSettings uses the Built-in Render Pipeline.
                // Reserve layer 31 only during the synchronous capture; restore it in finally.
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                    throw new InvalidOperationException("This preview requires the project's Built-in Render Pipeline.");
                if (!previewCamera)
                {
                    var go = new GameObject("IP2b - offscreen flat preview camera");
                    go.transform.SetParent(transform,false);
                    previewCamera = go.AddComponent<Camera>(); previewCamera.enabled=false;
                    previewCamera.stereoTargetEye=StereoTargetEyeMask.None;
                    previewCamera.orthographic=true; previewCamera.cullingMask=1<<31;
                    previewCamera.clearFlags=CameraClearFlags.SolidColor; previewCamera.backgroundColor=Color.black;
                    previewCamera.nearClipPlane=.01f; previewCamera.farClipPlane=5;
                    previewCamera.allowHDR=false; previewCamera.allowMSAA=false; previewCamera.useOcclusionCulling=false;
                }
                float width=selected.size.x*Mathf.Abs(selected.transform.lossyScale.x);
                float height=selected.size.y*Mathf.Abs(selected.transform.lossyScale.y);
                int pixelsX=Mathf.Clamp(Mathf.RoundToInt(1024*width/height),256,2048);
                if (!previewTexture || previewTexture.width != pixelsX)
                {
                    if (previewTexture) { previewTexture.Release(); Destroy(previewTexture); }
                    previewTexture=new RenderTexture(pixelsX,1024,24,RenderTextureFormat.ARGB32)
                        { name="IP2b flat composition", antiAliasing=1 };
                    if (!previewTexture.Create()) throw new InvalidOperationException("Could not allocate preview texture.");
                }
                previewCamera.transform.SetPositionAndRotation(selected.transform.position-selected.transform.forward*2,selected.transform.rotation);
                previewCamera.orthographicSize=height*.5f; previewCamera.aspect=width/height;
                previewCamera.targetTexture=previewTexture;
                foreach (var node in selected.GetComponentsInChildren<Transform>(true))
                { layers[node.gameObject]=node.gameObject.layer; node.gameObject.layer=31; }
                foreach (var item in selected.GetComponentsInChildren<FloatingStoryItem>(true))
                {
                    if (!item.highlight) continue;
                    visibility[item.highlight]=item.highlight.activeSelf; item.highlight.SetActive(false);
                }
                foreach (var canvas in selected.GetComponentsInChildren<Canvas>(true))
                { canvasCameras[canvas]=canvas.worldCamera; canvas.worldCamera=previewCamera; }
                Canvas.ForceUpdateCanvases();
                previewCamera.Render();
                if (!flatPreview)
                {
                    flatPreview=new GameObject("IP2b - flat Story preview"); flatPreview.transform.SetParent(contentRoot,false);
                    flatPreview.transform.localPosition=new Vector3(0,1.50f,.78f);
                    var material=NewMaterial(photoShader,Color.white,previewTexture);
                    var surface=Quad("Flattened image",flatPreview.transform,Vector3.zero,new Vector2(.84f*width/height,.84f),material);
                    previewRenderer=surface.GetComponent<Renderer>();
                    var caption=CollageCanvas("Preview label",flatPreview.transform,new Vector3(0,-.47f,0),new Vector2(600,65));
                    CollageText(caption.transform,"FLAT STORY PREVIEW",Vector2.zero,new Vector2(590,60),27);
                }
                previewRenderer.sharedMaterial.mainTexture=previewTexture;
                previewRenderer.transform.localScale=new Vector3(.84f*width/height,.84f,1);
                flatPreviewReady=true;
                Record("flat_preview_ready",selected.itemId+" | "+previewTexture.width+"x1024");
                Hint("This is the flat image your Story will show.\nCheck overlap, then confirm or go back to edit.");
            }
            catch (Exception error)
            {
                Debug.LogError("IP2b preview: "+error.Message,this);
                Record("flat_preview_failed",error.Message);
                Hint("The preview could not be created. Use RETRY PREVIEW or BACK TO EDIT.");
            }
            finally
            {
                foreach (var pair in layers) if (pair.Key) pair.Key.layer=pair.Value;
                foreach (var pair in visibility) if (pair.Key) pair.Key.SetActive(pair.Value);
                foreach (var pair in canvasCameras) if (pair.Key) pair.Key.worldCamera=pair.Value;
                if (previewCamera) previewCamera.targetTexture=null;
            }
            Refresh();
        }
    }
}
