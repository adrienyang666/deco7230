using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SpatialStories.Immersive.Editor
{
    [CustomEditor(typeof(ImmersiveStory))]
    public sealed class ImmersiveStoryEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var controller = (ImmersiveStory)target;
            EditorGUILayout.HelpBox("IP2b: photo wall, T/S spheres, spatial bin and publish control. " +
                "Use these preview buttons outside Play. Runtime starts from the welcome screen.", MessageType.Info);
            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                if (GUILayout.Button("Show full floating album / 悬浮相册")) Preview(controller, 0);
                if (GUILayout.Button("Show close editing / 放大编辑")) Preview(controller, 1);
                if (GUILayout.Button("Show text tools / 文字工具")) Preview(controller, 2);
                if (GUILayout.Button("Show sticker choices / 贴纸选择")) Preview(controller, 3);
            }
            using (new EditorGUI.DisabledScope(Application.isPlaying || controller.editorPreviewMode != 0))
                if (GUILayout.Button("Capture current album positions / 保存相册排列")) CaptureAlbum(controller);
            EditorGUILayout.HelpBox("To customise the photo wall: show the floating album, move its photo objects, " +
                "then capture the album positions and save the scene. Capture is disabled in close editing/tool previews.", MessageType.None);
            if (GUILayout.Button("Frame workspace / 聚焦场景"))
            {
                Selection.activeGameObject = controller.gameObject;
                if (SceneView.lastActiveSceneView)
                    SceneView.lastActiveSceneView.LookAt(new Vector3(0f,1.45f,1.5f), Quaternion.Euler(4f,0f,0f), 2.9f, false, true);
            }
            EditorGUILayout.Space();
            DrawDefaultInspector();
        }

        static void CaptureAlbum(ImmersiveStory controller)
        {
            if (Application.isPlaying || controller.editorPreviewMode != 0 || !controller.contentRoot || controller.mediaCards == null) return;
            var count = controller.mediaCards.Length;
            var positions = new Vector3[count]; var rotations = new Quaternion[count]; var scales = new Vector3[count];
            var parentScale = controller.contentRoot.lossyScale;
            if (Mathf.Abs(parentScale.x) < .0001f || Mathf.Abs(parentScale.y) < .0001f || Mathf.Abs(parentScale.z) < .0001f)
            {
                EditorUtility.DisplayDialog("Cannot capture zero scale", "Restore a nonzero scale on the spatial interface before capturing the album.", "OK");
                return;
            }
            for (var i = 0; i < count; i++)
            {
                var card = controller.mediaCards[i];
                if (!card)
                {
                    EditorUtility.DisplayDialog("Missing photo reference", "A photo is missing from Media Cards. Restore that reference before capturing the album.", "OK");
                    return;
                }
                positions[i] = controller.contentRoot.InverseTransformPoint(card.transform.position);
                rotations[i] = Quaternion.Inverse(controller.contentRoot.rotation) * card.transform.rotation;
                var scale = card.transform.lossyScale;
                scales[i] = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            }
            Undo.RecordObject(controller, "Capture album arrangement");
            controller.albumPositions = positions; controller.albumRotations = rotations; controller.albumScales = scales;
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            Debug.Log("Captured " + count + " album photo positions. Save this scene to retain them.", controller);
        }

        static void Preview(ImmersiveStory controller, int stage)
        {
            Undo.RegisterFullObjectHierarchyUndo(controller.gameObject, "Preview spatial layout");
            controller.ShowDesign(stage);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
            SceneView.RepaintAll();
        }
    }
}
