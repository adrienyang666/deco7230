using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialStories.Quest.Editor
{
    [CustomEditor(typeof(VRWorkshop))]
    public sealed class WorkshopLayoutEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var workshop = (VRWorkshop)target;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("The room and workspace are saved scene objects. Use these buttons to inspect each layout without Play. Edit child transforms in the Hierarchy, then save the scene. Runtime always starts at Welcome.", MessageType.Info);
            using (new EditorGUI.DisabledScope(Application.isPlaying || !workshop.HasSavedLayout))
            {
                if (GUILayout.Button("Fix Album Photos / 修复相册照片")) Show(workshop, 1);
                if (GUILayout.Button("Show Welcome / 欢迎页")) Show(workshop, 0);
                if (GUILayout.Button("Show Photo Album / 相册")) Show(workshop, 1);
                if (GUILayout.Button("Show Editing Desk / 编辑工作台")) Show(workshop, 2);
                if (GUILayout.Button("Frame Workspace / 聚焦工作台")) Frame();
            }
        }

        static void Show(VRWorkshop workshop, int stage)
        {
            Undo.RegisterFullObjectHierarchyUndo(workshop.gameObject, "Show Spatial Stories layout");
            // Read the existing saved album reference; no new runtime method is required.
            var serializedWorkshop = new SerializedObject(workshop);
            var albumProperty = serializedWorkshop.FindProperty("album");
            var album = albumProperty != null ? albumProperty.objectReferenceValue as GameObject : null;
            if (album)
            {
                foreach (var photo in album.GetComponentsInChildren<RawImage>(true))
                {
                    photo.uvRect = new Rect(0f, 0f, 1f, 1f);
                    EditorUtility.SetDirty(photo);
                    if (PrefabUtility.IsPartOfPrefabInstance(photo))
                        PrefabUtility.RecordPrefabInstancePropertyModifications(photo);
                }
            }
            workshop.ShowLayoutInEditor(stage);
            EditorSceneManager.MarkSceneDirty(workshop.gameObject.scene);
            Frame();
        }

        [MenuItem("Tools/Spatial Stories/Frame Workspace")]
        public static void Frame()
        {
            var workshop = Object.FindFirstObjectByType<VRWorkshop>();
            if (!workshop || !SceneView.lastActiveSceneView) return;
            var center = workshop.transform.TransformPoint(new Vector3(0, 1.2f, 1.0f));
            SceneView.lastActiveSceneView.LookAt(center, Quaternion.Euler(12, -12, 0), 2.1f, false);
        }
    }
}
