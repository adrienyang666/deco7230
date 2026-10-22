using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpatialStories.Immersive.Editor
{
    public static class IP2bProjectTools
    {
        const string Menu = "Tools/Spatial Stories IP2b/";
        public const string Quest = "Assets/SpatialStoriesImmersive/Scenes/SpatialStories_IP2b_Quest.unity";
        public const string Simulator = "Assets/SpatialStoriesImmersive/Scenes/SpatialStories_IP2b_Simulator.unity";

        [MenuItem(Menu + "1 - Open Quest scene")]
        public static void OpenQuest() => Open(Quest);
        [MenuItem(Menu + "2 - Open Simulator scene")]
        public static void OpenSimulator() => Open(Simulator);

        static void Open(string path)
        {
            if (Application.isPlaying || !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(path);
        }

        [MenuItem(Menu + "3 - Validate open IP2b scene")]
        public static void Validate()
        {
            var errors = ValidationErrors();
            if (errors.Count == 0) Debug.Log("IP2b asset/scene checks passed. Run EditMode tests and the Quest checklist in IP2b_START_HERE.md before the participant session.");
            else Debug.LogError("IP2b validation:\n" + string.Join("\n",errors));
        }

        public static List<string> ValidationErrors()
        {
            var errors = new List<string>();
            var story = UnityEngine.Object.FindFirstObjectByType<ImmersiveStory>();
            if (!story || !story.enableCollage) errors.Add("Open an IP2b scene; its controller must have Enable Collage checked.");
            else
            {
                if (!story.head || !story.contentRoot || !story.focusAnchor || !story.spawnAnchor) errors.Add("A required spatial anchor or XR camera is missing.");
                if (story.mediaCards == null || story.mediaCards.Length != 6) errors.Add("Expected the six original floating photographs.");
                else for (int i=0; i<story.mediaCards.Length; i++)
                {
                    if (!story.mediaCards[i]) { errors.Add("Missing photograph " + (i+1)); continue; }
                    var scale=story.mediaCards[i].transform.lossyScale;
                    if (Mathf.Abs(scale.x-scale.y)>.001f || Mathf.Abs(scale.x-scale.z)>.001f || scale.x<=0)
                        errors.Add("Use positive uniform scale on photograph " + (i+1) + ".");
                }
                if (!story.stickerPanel || !story.textTemplate || !story.reviewPanel) errors.Add("A required original tool panel or text template is missing.");
            }
            foreach (var name in new[] { "Surfer", "Eiffel" })
            {
                var path="Assets/SpatialStoriesImmersive/Runtime/Resources/IP2b/"+name+".png";
                var importer=AssetImporter.GetAtPath(path) as TextureImporter;
                if (!importer || !AssetDatabase.LoadAssetAtPath<Texture2D>(path)) errors.Add("Missing subject asset: " + name);
                else if (!importer.alphaIsTransparency) errors.Add("Enable Alpha Is Transparency for " + name);
            }
            if (!Resources.Load<Shader>("CollageDepth")) errors.Add("Missing CollageDepth shader.");
            if (!Resources.Load<Shader>("CollageGhost")) errors.Add("Missing CollageGhost placement preview shader.");
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null) errors.Add("The supplied flat preview uses the original Built-in Render Pipeline.");
            foreach (var node in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                if (node.gameObject.layer == 31) { errors.Add("Layer 31 must be unused outside the temporary preview capture."); break; }
            return errors;
        }

        [MenuItem(Menu + "4 - Build IP2b Quest APK")]
        public static void BuildQuest()
        {
            if (Application.isPlaying) { Debug.LogError("Exit Play mode before building."); return; }
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            { EditorUtility.DisplayDialog("Android profile", "Activate your Android build profile, then run this command again.", "OK"); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(Quest);
            var errors=ValidationErrors();
            if (errors.Count>0) { Debug.LogError(string.Join("\n",errors)); return; }
            var path=EditorUtility.SaveFilePanel("Build IP2b for Quest", "", "SpatialStories_IP2b_Quest.apk", "apk");
            if (string.IsNullOrEmpty(path)) return;
            EditorUserBuildSettings.selectedBuildTargetGroup=BuildTargetGroup.Android;
            EditorUserBuildSettings.androidBuildSubtarget=MobileTextureSubtarget.ASTC;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions
            { scenes=new[] { Quest }, target=BuildTarget.Android,
              subtarget=(int)MobileTextureSubtarget.ASTC, locationPathName=path, options=BuildOptions.None });
            if (report.summary.result==BuildResult.Succeeded) Debug.Log("IP2b Quest APK: "+path);
            else Debug.LogError("IP2b APK build failed; inspect the Unity Console.");
        }
    }
}
