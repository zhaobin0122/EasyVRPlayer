using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(Pvr_UnitySDKEyeOverlay))]
public class Pvr_UnitySDKEyeOverlayEditor : Editor
{
    public override void OnInspectorGUI()
    {
        foreach (Pvr_UnitySDKEyeOverlay overlayTarget in targets)
        {
            EditorGUILayout.LabelField("Overlay Display Order", EditorStyles.boldLabel);
            overlayTarget.overlayType = (Pvr_UnitySDKEyeOverlay.OverlayType)EditorGUILayout.EnumPopup("Overlay Type", overlayTarget.overlayType);
            overlayTarget.layerIndex = EditorGUILayout.IntField("Layer Index", overlayTarget.layerIndex);

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Overlay Shape", EditorStyles.boldLabel);
            overlayTarget.overlayShape = (Pvr_UnitySDKEyeOverlay.OverlayShape)EditorGUILayout.EnumPopup("Overlay Shape", overlayTarget.overlayShape);
            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Overlay Textures", EditorStyles.boldLabel);
            overlayTarget.isExternalAndroidSurface = EditorGUILayout.Toggle("External Surface", overlayTarget.isExternalAndroidSurface);
            var labelControlRect = EditorGUILayout.GetControlRect();
            EditorGUI.LabelField(new Rect(labelControlRect.x, labelControlRect.y, labelControlRect.width / 2, labelControlRect.height), new GUIContent("Left Texture", "Texture used for the left eye"));
            EditorGUI.LabelField(new Rect(labelControlRect.x + labelControlRect.width / 2, labelControlRect.y, labelControlRect.width / 2, labelControlRect.height), new GUIContent("Right Texture", "Texture used for the right eye"));

            var textureControlRect = EditorGUILayout.GetControlRect(GUILayout.Height(64));
            overlayTarget.layerTextures[0] = (Texture2D)EditorGUI.ObjectField(new Rect(textureControlRect.x, textureControlRect.y, 64, textureControlRect.height), overlayTarget.layerTextures[0], typeof(Texture2D), false);
            overlayTarget.layerTextures[1] = (Texture2D)EditorGUI.ObjectField(new Rect(textureControlRect.x + textureControlRect.width / 2, textureControlRect.y, 64, textureControlRect.height), overlayTarget.layerTextures[1] != null ? overlayTarget.layerTextures[1] : overlayTarget.layerTextures[0], typeof(Texture2D), false);

        }

        //DrawDefaultInspector();
        if (GUI.changed)
        {
#if !UNITY_5_2
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#endif
        }
    }
}
