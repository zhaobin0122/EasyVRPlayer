using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NoloVR_Manager))]
public class NoloVR_ManagerEditor : Editor
{

    NoloVR_Manager m_Target;
    NoloVR_TrackedDevice controllerRight = null;

    public override void OnInspectorGUI()
    {
        m_Target = target as NoloVR_Manager;
        m_Target.appKey = EditorGUILayout.TextField("App Key", m_Target.appKey);
        m_Target.gameTrackModel = (NoloVR_Manager.TrackModel)EditorGUILayout.EnumPopup("Game Track Model", m_Target.gameTrackModel);
        m_Target.leftController = (GameObject)EditorGUILayout.ObjectField("Left Controller",m_Target.leftController, typeof(GameObject),true);
        m_Target.rightController = (GameObject)EditorGUILayout.ObjectField("Right Controller", m_Target.rightController, typeof(GameObject), true);

        if (m_Target.gameTrackModel == NoloVR_Manager.TrackModel.Track_3dof)
        {
            m_Target.useDefaultHeight = EditorGUILayout.Toggle("Use Default Height", m_Target.useDefaultHeight);
            if (m_Target.useDefaultHeight)
            {
                m_Target.defaultHeight = EditorGUILayout.Slider("Default Height", m_Target.defaultHeight, 1, 2);
            }
            //ResetRightContrller(false);
            string strPC = NoloVR_System.GetInstance().realTrackDevices==3?"NOLO_3DOF":"NOLO_6DOF";
            if (strPC.Contains("NOLO_6DOF"))
            {
                strPC = strPC.Replace("NOLO_6DOF", "NOLO_3DOF");
            }
            else if (strPC.Contains("NOLO_3DOF")) { }
            else
            {
                strPC += ";NOLO_3DOF";
            }
            string strAD = NoloVR_System.GetInstance().realTrackDevices == 3 ? "NOLO_3DOF" : "NOLO_6DOF";
            if (strAD.Contains("NOLO_6DOF"))
            {
                strAD = strAD.Replace("NOLO_6DOF", "NOLO_3DOF");
            }
            else if (strPC.Contains("NOLO_3DOF")) { }
            else
            {
                strAD += ";NOLO_3DOF";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, strPC);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, strAD);
        }
        else
        {
            m_Target.VRCamera = (GameObject)EditorGUILayout.ObjectField("VR Camera", m_Target.VRCamera, typeof(GameObject), true);
            //m_Target.turnAroundButtonType = (TurnAroundButtonType)EditorGUILayout.EnumPopup("Turn Around Button", m_Target.turnAroundButtonType);
            //ResetRightContrller(true);
            string strPC = NoloVR_System.GetInstance().realTrackDevices == 3 ? "NOLO_3DOF" : "NOLO_6DOF";
            if (strPC.Contains("NOLO_3DOF"))
            {
                strPC = strPC.Replace("NOLO_3DOF", "NOLO_6DOF");
            }
            else if (strPC.Contains("NOLO_6DOF")) { }
            else
            {
                strPC += ";NOLO_6DOF";
            }
            string strAD = NoloVR_System.GetInstance().realTrackDevices == 3 ? "NOLO_3DOF" : "NOLO_6DOF";
            if (strAD.Contains("NOLO_3DOF"))
            {
                strAD = strAD.Replace("NOLO_3DOF", "NOLO_6DOF");
            }
            else if (strPC.Contains("NOLO_6DOF")) { }
            else
            {
                strAD += ";NOLO_6DOF";
            }
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, strPC);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, strAD);
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(m_Target);
        }
    }

    void ResetRightContrller(bool status)
    {
        if (controllerRight == null)
        {
            NoloVR_TrackedDevice[] devices = m_Target.GetComponentsInChildren<NoloVR_TrackedDevice>();
            foreach (NoloVR_TrackedDevice item in devices)
            {
                if (item.deviceType == NoloDeviceType.RightController)
                {
                    controllerRight = item;
                }
            }
            if (controllerRight == null)
            {
                controllerRight = m_Target.transform.Find("ControllerTracker/RightController").GetComponent<NoloVR_TrackedDevice>();
            }
        }
        if (controllerRight != null)
        {
            controllerRight.gameObject.SetActive(status);
        }
        else
        {
            Debug.LogWarning("not find controller right");
        }
    }

}
