using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class TargetSdkManager
{
    private static string targetSdkCfgFilePath = Application.streamingAssetsPath + "/TargetSdkCfg.txt";
    private static object targetSdkHelperInstance;

    public static object GetTargetSdkHelperInstance()
    {
        if (targetSdkHelperInstance == null)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            targetSdkHelperInstance = assembly.CreateInstance(GetTargetSdkHelperClassName());
        }
        return targetSdkHelperInstance;
    }

    public static void SetTargetSdkHelperClassName(string sdkTypeName)
    {
        File.WriteAllText(targetSdkCfgFilePath, sdkTypeName + "Helper");
    }

    public static string GetTargetSdkHelperClassName()
    {
        Debug.Log("GetTargetSdkHelperClassName");
        WWW www = new WWW(targetSdkCfgFilePath);
        while (!www.isDone) { }
        Debug.Log("GetTargetSdkHelperClassName " + www.text);
        return www.text;

        //return "PicoHelper";
        //return "HuaWeiHelper";
        //return "ViveWaveHelper";
    }

}
