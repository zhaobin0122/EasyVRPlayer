using UnityEngine;
using UnityEditor;
using Pvr_UnitySDKAPI;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Linq;

[CustomEditor(typeof(Pvr_UnitySDKManager))]
public class Pvr_UnitySDKManagerEditor : Editor
{
    public delegate void HeadDofChanged(string dof);
    public static event HeadDofChanged HeadDofChangedEvent;

    static int QulityRtMass = 0;
    public delegate void Change(int Msaa);
    public static event Change MSAAChange;
    public const string PVRSinglePassDefine = "PVR_SINGLEPASS_ENABLED";
    public override void OnInspectorGUI()
    {
        GUI.changed = false;

        GUIStyle firstLevelStyle = new GUIStyle(GUI.skin.label);
        firstLevelStyle.alignment = TextAnchor.UpperLeft;
        firstLevelStyle.fontStyle = FontStyle.Bold;
        firstLevelStyle.fontSize = 12;
        firstLevelStyle.wordWrap = true;

        Pvr_UnitySDKManager manager = (Pvr_UnitySDKManager)target;

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Current Build Platform", firstLevelStyle);
        EditorGUILayout.LabelField(EditorUserBuildSettings.activeBuildTarget.ToString());
        GUILayout.Space(10);

        EditorGUILayout.LabelField("RenderTexture Setting", firstLevelStyle);
        manager.RtAntiAlising = (RenderTextureAntiAliasing)EditorGUILayout.EnumPopup("RenderTexture Anti-Aliasing", manager.RtAntiAlising);
#if UNITY_2018_3_OR_NEWER
        GUI.enabled = false;
#endif
        manager.RtBitDepth = (RenderTextureDepth)EditorGUILayout.EnumPopup("RenderTexture Bit Depth", manager.RtBitDepth);
        manager.RtFormat = (RenderTextureFormat)EditorGUILayout.EnumPopup("RenderTexture Format", manager.RtFormat);
#if UNITY_2018_3_OR_NEWER
        GUI.enabled = true;
#endif
        manager.DefaultRenderTexture = EditorGUILayout.Toggle("Use Default RenderTexture", manager.DefaultRenderTexture);
        if (!manager.DefaultRenderTexture)
        {
            manager.RtSize = EditorGUILayout.Vector2Field("    RT Size", manager.RtSize);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Note:", firstLevelStyle);
            EditorGUILayout.LabelField("1.width & height must be larger than 0;");
            EditorGUILayout.LabelField("2.the size of RT has a great influence on performance;");
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Pose Settings", firstLevelStyle);
        manager.TrackingOrigin = (TrackingOrigin)EditorGUILayout.EnumPopup("Tracking Origin", manager.TrackingOrigin);
        manager.ResetTrackerOnLoad = EditorGUILayout.Toggle("Reset Tracker OnLoad", manager.ResetTrackerOnLoad);
        manager.Rotfoldout = EditorGUILayout.Foldout(manager.Rotfoldout, "Only Rotation Tracking");
        if (manager.Rotfoldout)
        {
            manager.HmdOnlyrot = EditorGUILayout.Toggle("  Only HMD Rotation Tracking", manager.HmdOnlyrot);
            if (manager.HmdOnlyrot)
            {
                manager.PVRNeck = EditorGUILayout.Toggle("    Enable Neck Model", manager.PVRNeck);
                if (manager.PVRNeck)
                {
                    manager.UseCustomNeckPara = EditorGUILayout.Toggle("Use Custom Neck Parameters", manager.UseCustomNeckPara);
                    if (manager.UseCustomNeckPara)
                    {
                        manager.neckOffset = EditorGUILayout.Vector3Field("Neck Offset", manager.neckOffset);
                    }
                }
            }
            manager.ControllerOnlyrot =
                EditorGUILayout.Toggle("  Only Controller Rotation Tracking", manager.ControllerOnlyrot);
        }
        else
        {
            manager.HmdOnlyrot = false;
            manager.ControllerOnlyrot = false;
        }
        
        manager.MovingRatios = EditorGUILayout.FloatField("Position ScaleFactor", manager.MovingRatios);
        manager.SixDofPosReset = EditorGUILayout.Toggle("Enable 6Dof Position Reset", manager.SixDofPosReset);

        manager.DefaultRange = EditorGUILayout.Toggle("Use Default Safe Radius", manager.DefaultRange);
        if (!manager.DefaultRange)
        {
            manager.CustomRange = EditorGUILayout.FloatField("    Safe Radius(meters)", manager.CustomRange);
        }
        else
        {
            manager.CustomRange = 0.8f;
        }

        GUILayout.Space(10);
        EditorGUILayout.LabelField("Other Settings", firstLevelStyle);
        manager.ShowFPS = EditorGUILayout.Toggle("Show FPS", manager.ShowFPS);
        manager.ShowSafePanel = EditorGUILayout.Toggle("Show SafePanel", manager.ShowSafePanel);
        manager.ScreenFade = EditorGUILayout.Toggle("Open Screen Fade", manager.ScreenFade);
        manager.DefaultFPS = EditorGUILayout.Toggle("Use Default FPS", manager.DefaultFPS);
        if (!manager.DefaultFPS)
        {
            manager.CustomFPS = EditorGUILayout.IntField("    FPS", manager.CustomFPS);
        }
        manager.Monoscopic = EditorGUILayout.Toggle("Use Monoscopic", manager.Monoscopic);
        manager.Copyrightprotection = EditorGUILayout.Toggle("Copyright protection", manager.Copyrightprotection);
        bool singlePass = manager.UseSinglePass;
        manager.UseSinglePass = EditorGUILayout.Toggle("UseSinglePass", manager.UseSinglePass);
        if (singlePass != manager.UseSinglePass)
        {
            SetSinglePass(manager.UseSinglePass);
        }
        manager.UseSinglePass = IsSinglePassEnable();
        if (GUI.changed)
        {
            QulityRtMass = (int)Pvr_UnitySDKManager.SDK.RtAntiAlising;
            if (QulityRtMass == 1)
            {
                QulityRtMass = 0;
            }
            if (MSAAChange != null)
            {
                MSAAChange(QulityRtMass);
            }
            var headDof = Pvr_UnitySDKManager.SDK.HmdOnlyrot ? 0 : 1;
            if (HeadDofChangedEvent != null)
            {
                if (headDof == 0)
                {
                    HeadDofChangedEvent("3dof");
                }
                else
                {
                    HeadDofChangedEvent("6dof");
                }

            }
            EditorUtility.SetDirty(manager);
#if !UNITY_5_2
            if (!Application.isPlaying)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }
#endif
        }

        serializedObject.ApplyModifiedProperties();
    }

    private bool IsSinglePassEnable()
    {
        bool isSinglePass;
#if UNITY_2017_2
        isSinglePass = PlayerSettings.virtualRealitySupported;
#else
        isSinglePass = PlayerSettings.GetVirtualRealitySupported(BuildTargetGroup.Android);
#endif
        //List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
        //isSinglePass &= allDefines.Contains(PVRSinglePassDefine);
        isSinglePass &= PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
        GraphicsDeviceType[] graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        isSinglePass &= graphics[0] == GraphicsDeviceType.OpenGLES3;
        return isSinglePass;
    }

    public void SetSinglePass(bool enable)
    {
        if (enable)
        {
            SetVRSupported(BuildTargetGroup.Android, true);
            PlayerSettings.stereoRenderingPath = StereoRenderingPath.SinglePass;
            //List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
            //SetSinglePassDefine(BuildTargetGroup.Android, true, allDefines);
            SetGraphicsAPI();
        }
        else
        {
            SetVRSupported(BuildTargetGroup.Android, false);
            PlayerSettings.stereoRenderingPath = StereoRenderingPath.MultiPass;
            //List<string> allDefines = GetDefineSymbols(BuildTargetGroup.Android);
            //SetSinglePassDefine(BuildTargetGroup.Android, false, allDefines);
        }
    }

    private void SetGraphicsAPI()
    {
        GraphicsDeviceType[] graphics = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        List<GraphicsDeviceType> listgraphic = graphics.ToList();
        if (listgraphic.Contains(GraphicsDeviceType.OpenGLES3))
        {
            int index = listgraphic.IndexOf(GraphicsDeviceType.OpenGLES3);
            GraphicsDeviceType temp = listgraphic[0];
            listgraphic[0] = GraphicsDeviceType.OpenGLES3;
            listgraphic[index] = temp;
        }
        else
        {
            listgraphic.Insert(0, GraphicsDeviceType.OpenGLES3);
        }
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, listgraphic.ToArray());
    }

    public static void SetVRSupported(BuildTargetGroup group, bool set)
    {
#if UNITY_2017_2
        PlayerSettings.virtualRealitySupported = set; 
#else
        PlayerSettings.SetVirtualRealitySupported(group, set);
#endif
    }

    public static List<string> GetDefineSymbols(BuildTargetGroup group)
    {
        string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
        return symbols.Split(';').ToList();
    }

    public static void SetSinglePassDefine(BuildTargetGroup group, bool set, List<string> allDefines)
    {
        var hasDefine = allDefines.Contains(PVRSinglePassDefine);

        if (set)
        {
            if (hasDefine)
                return;
            allDefines.Add(PVRSinglePassDefine);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
            Debug.Log("Add \"" + PVRSinglePassDefine + "\" to define symbols");
        }
        else
        {
            if (hasDefine)
            {
                allDefines.Remove(PVRSinglePassDefine);
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", allDefines.ToArray()));
                Debug.Log("Remove \"" + PVRSinglePassDefine + "\" from define symbols");
            }
        }
    }

}
