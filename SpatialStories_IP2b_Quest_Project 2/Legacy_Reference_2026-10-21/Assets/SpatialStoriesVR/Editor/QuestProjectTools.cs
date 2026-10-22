using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;

namespace SpatialStories.Quest.Editor
{
    public static class QuestProjectTools
    {
        const string Scene = "Assets/SpatialStoriesVR/Scenes/SpatialStories_IP2a_Quest.unity";
        const string Simulator = "Assets/SpatialStoriesVR/Scenes/SpatialStories_IP2a_Simulator.unity";

        [MenuItem("Tools/Spatial Stories/Open Quest scene")]
        public static void OpenQuest() { Open(Scene); }

        [MenuItem("Tools/Spatial Stories/Open computer simulator")]
        public static void OpenSimulator() { Open(Simulator); }

        static void Open(string path)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
                WorkshopLayoutEditor.Frame();
            }
        }

        [MenuItem("Tools/Spatial Stories/Check project configuration")]
        public static void Check()
        {
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(Scene)) throw new InvalidOperationException("Quest scene is missing.");
            if (!Resources.Load<Shader>("StoryUnlit")) throw new InvalidOperationException("Stereo shader is missing.");
            for (int i = 1; i <= 6; i++)
                if (!AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/SpatialStories/Textures/Photo_{i:00}.png"))
                    throw new InvalidOperationException("Original photo " + i + " is missing.");
            var general = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (!general || !general.InitManagerOnStart || !general.Manager ||
                !general.Manager.activeLoaders.Any(l => l is OpenXRLoader))
                throw new InvalidOperationException("Enable Android OpenXR and Initialize XR on Startup.");
            var settings = OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (!settings || settings.GetFeature<MetaQuestFeature>()?.enabled != true ||
                settings.GetFeature<OculusTouchControllerProfile>()?.enabled != true)
                throw new InvalidOperationException("Enable Android Meta Quest Support and Oculus Touch Controller Profile.");
            if (PlayerSettings.GetScriptingBackend(NamedBuildTarget.Android) != ScriptingImplementation.IL2CPP ||
                PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
                throw new InvalidOperationException("Android must use IL2CPP and ARM64.");
            if (PlayerSettings.GetGraphicsAPIs(BuildTarget.Android).FirstOrDefault() != GraphicsDeviceType.Vulkan)
                throw new InvalidOperationException("Set Vulkan as the Android graphics API.");
            Debug.Log("Spatial Stories configuration checked. Run OpenXR Project Validation, then build and test on Quest before the participant session.");
        }

        [MenuItem("Tools/Spatial Stories/Build Quest APK")]
        public static void BuildQuest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUtility.DisplayDialog("Switch to Android", "Open File > Build Profiles, select Android and Switch Platform. Then choose Build Quest APK again.", "OK");
                return;
            }
            Check();
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            Directory.CreateDirectory("Builds");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Scene }, target = BuildTarget.Android,
                locationPathName = "Builds/SpatialStories_IP2a.apk", options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("APK build did not succeed. See the first Console error.");
            EditorUtility.RevealInFinder("Builds/SpatialStories_IP2a.apk");
        }
    }
}
