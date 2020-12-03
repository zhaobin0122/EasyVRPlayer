using Pvr_UnitySDKAPI;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pvr_UnitySDKEyeManager))]
public class Pvr_UnitySDKEyeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUI.changed = false;

        GUIStyle firstLevelStyle = new GUIStyle(GUI.skin.label);
        firstLevelStyle.alignment = TextAnchor.UpperLeft;
        firstLevelStyle.fontStyle = FontStyle.Bold;
        firstLevelStyle.fontSize = 12;
        firstLevelStyle.wordWrap = true;

        Pvr_UnitySDKEyeManager sdkEyeManager = (Pvr_UnitySDKEyeManager)target;

        sdkEyeManager.trackEyes = EditorGUILayout.Toggle("Track Eyes", sdkEyeManager.trackEyes);
        if(sdkEyeManager.trackEyes)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Note:", firstLevelStyle);
            EditorGUILayout.LabelField("EyeTracking is supported only on the Neo2 Eye");
            EditorGUILayout.EndVertical();
        }

        sdkEyeManager.FoveationLevel = (EFoveationLevel)EditorGUILayout.EnumPopup("Foveation Level", sdkEyeManager.FoveationLevel);
        //eFoveationLevel lastlevel = sdkEyeManager.FoveationLevel;
        //eFoveationLevel newlevel = (eFoveationLevel)EditorGUILayout.EnumPopup("Foveation Level", sdkEyeManager.FoveationLevel);
        //if (lastlevel != newlevel)
        //{
        //    sdkEyeManager.FoveationLevel = newlevel;
        //    switch (sdkEyeManager.FoveationLevel)
        //    {
        //        case eFoveationLevel.None:
        //            sdkEyeManager.FoveationGainValue = Vector2.zero;
        //            sdkEyeManager.FoveationAreaValue = 0.0f;
        //            sdkEyeManager.FoveationMinimumValue = 0.0f;
        //            break;
        //        case eFoveationLevel.Low:
        //            sdkEyeManager.FoveationGainValue = new Vector2(2.0f, 2.0f);
        //            sdkEyeManager.FoveationAreaValue = 0.0f;
        //            sdkEyeManager.FoveationMinimumValue = 0.125f;
        //            break;
        //        case eFoveationLevel.Med:
        //            sdkEyeManager.FoveationGainValue = new Vector2(3.0f, 3.0f);
        //            sdkEyeManager.FoveationAreaValue = 1.0f;
        //            sdkEyeManager.FoveationMinimumValue = 0.125f;
        //            break;
        //        case eFoveationLevel.High:
        //            sdkEyeManager.FoveationGainValue = new Vector2(4.0f, 4.0f);
        //            sdkEyeManager.FoveationAreaValue = 2.0f;
        //            sdkEyeManager.FoveationMinimumValue = 0.125f;
        //            break;
        //    }
        //}
        //sdkEyeManager.FoveationGainValue = EditorGUILayout.Vector2Field("Foveation Gain Value", sdkEyeManager.FoveationGainValue);
        //sdkEyeManager.FoveationAreaValue = EditorGUILayout.FloatField("Foveation Area Value", sdkEyeManager.FoveationAreaValue);
        //sdkEyeManager.FoveationMinimumValue = EditorGUILayout.FloatField("Foveation Minimum Value", sdkEyeManager.FoveationMinimumValue);

        EditorUtility.SetDirty(sdkEyeManager);
        if (GUI.changed)
        {
#if !UNITY_5_2
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager
                .GetActiveScene());
#endif
        }
    }

}
