using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public class Pvr_EyeTrackingEditor : Editor, IPreprocessBuild
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildTarget target, string path)
    {
        bool trackEyes = CheckTrackEyes();
        if(trackEyes)
        {
            UpdateAndroidManifestXML("enable_eyetracking", "1");
        }
        else
        {
            UpdateAndroidManifestXML("enable_eyetracking", "0");
        }
    }

    public static bool CheckTrackEyes()
    {
        bool trackEyes = false;
        bool buildCurrentScene = false;
        if (CheckTrackEyes(ref buildCurrentScene))
        {
            trackEyes = true;
        }
        if (buildCurrentScene)
        {
            if(CheckTrackEyesInCur())
            {
                trackEyes = true;
            }
        }

        return trackEyes;
    }

    public static bool CheckTrackEyesInCur()
    {
        bool trackEyes = false;

        Pvr_UnitySDKEyeManager[] array = GameObject.FindObjectsOfType<Pvr_UnitySDKEyeManager>();
        foreach(Pvr_UnitySDKEyeManager manager in array)
        {
            if(manager.trackEyes)
            {
                trackEyes = true;
            }
        }

        return trackEyes;
    }

    public static bool CheckTrackEyes(ref bool buildCurrentScene)
    {
        bool trackEyes = false;

        EditorBuildSettingsScene[] scenelist = EditorBuildSettings.scenes;
        string[] allScenes = EditorBuildSettingsScene.GetActiveSceneList(scenelist);
        buildCurrentScene = (allScenes.Length == 0);

        foreach (string scenepath in allScenes)
        {
            if(CheckTrackEyesByScene(scenepath))
            {
                trackEyes = true;
            }
        }
        return trackEyes;
    }

    public static bool CheckTrackEyesByScene(string path)
    {
        StreamReader sr = new StreamReader(path, Encoding.Default);
        string line;
        string strValue;
        while ((line = sr.ReadLine()) != null)
        {
            if (line.Contains("trackEyes"))
            {
                if((strValue = sr.ReadLine()) != null)
                {
                    if(strValue.Contains("1"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
        return false;

    }

    public static void UpdateAndroidManifestXML(string attributename, string targetvalue)
    {
        string m_sXmlPath = "Assets/Plugins/Android/AndroidManifest.xml";
        if (File.Exists(m_sXmlPath))
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(m_sXmlPath);
            XmlNodeList nodeList;
            XmlElement root = xmlDoc.DocumentElement;
            nodeList = root.SelectNodes("/manifest/application/meta-data");
            foreach (XmlElement xe in nodeList)
            {
                if (xe.GetAttribute("android:name") == attributename)
                {
                    xe.SetAttribute("android:value", targetvalue);
                    xmlDoc.Save(m_sXmlPath);
                    return;
                }
            }
        }
    }

}
