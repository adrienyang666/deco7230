using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialStories.Quest.Editor
{
    // Standalone repair for scenes imported with a zero-sized RawImage UV rect.
    // Does not depend on the newer VRWorkshop or custom Inspector patch.
    public static class AlbumPhotoRepairTool
    {
        [MenuItem("Tools/Spatial Stories/Repair Album Photos")]
        public static void Repair()
        {
            if (Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Spatial Stories", "Exit Play mode before repairing the saved scene.", "OK");
                return;
            }

            var workshop = Object.FindFirstObjectByType<VRWorkshop>();
            if (!workshop || !workshop.gameObject.scene.IsValid())
            {
                EditorUtility.DisplayDialog("Spatial Stories", "Open the Spatial Stories Quest or simulator scene first.", "OK");
                return;
            }

            var serialized = new SerializedObject(workshop);
            var property = serialized.FindProperty("album");
            var album = property != null ? property.objectReferenceValue as GameObject : null;
            if (!album)
            {
                EditorUtility.DisplayDialog("Spatial Stories", "The saved album reference is missing. No changes were made.", "OK");
                return;
            }

            var images = album.GetComponentsInChildren<RawImage>(true);
            if (images.Length == 0)
            {
                EditorUtility.DisplayDialog("Spatial Stories", "No album photo components were found. No changes were made.", "OK");
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(workshop.gameObject, "Repair album photo display");
            int missing = 0;
            foreach (var image in images)
            {
                image.uvRect = new Rect(0f, 0f, 1f, 1f);
                image.SetVerticesDirty();
                EditorUtility.SetDirty(image);
                if (PrefabUtility.IsPartOfPrefabInstance(image))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(image);
                if (!image.texture) missing++;
            }
            workshop.ShowLayoutInEditor(1);
            EditorSceneManager.MarkSceneDirty(workshop.gameObject.scene);
            Canvas.ForceUpdateCanvases();
            Selection.activeGameObject = album;
            SceneView.RepaintAll();

            string message = "Reset UV display area on " + images.Length + " album photos.\nSave the scene with Cmd+S (Mac) or Ctrl+S (Windows).";
            if (missing > 0) message += "\nMissing textures: " + missing + ". These need their texture references restored.";
            EditorUtility.DisplayDialog("Spatial Stories", message, "OK");
        }
    }
}
