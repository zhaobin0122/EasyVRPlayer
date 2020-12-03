using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SelectSdk : MonoBehaviour {
    private const string sdkDefines = "Pico;HuaWei;ViveWave;Nolo_x1";

    private const string PicoMenuPath = "SdkType/Pico";
    private const string HuaWeiMenuPath = "SdkType/HuaWei";
    private const string ViveWaveMenuPath = "SdkType/ViveWave";
    private const string Nolo_x1MenuPath = "SdkType/Nolo_x1";

    //设置sdk勾选状态
    public static void SetSdkCheckStatus()
    {
#if Pico
        Menu.SetChecked(PicoMenuPath, true);
#else
        Menu.SetChecked(PicoMenuPath, false);
#endif

#if HuaWei
        Menu.SetChecked(HuaWeiMenuPath, true);
#else
        Menu.SetChecked(HuaWeiMenuPath, false);
#endif

#if ViveWave
        Menu.SetChecked(ViveWaveMenuPath, true);
#else
        Menu.SetChecked(ViveWaveMenuPath, false);
#endif

#if Nolo_x1
        Menu.SetChecked(Nolo_x1MenuPath, true);
#else
        Menu.SetChecked(Nolo_x1MenuPath, false);
#endif
    }

    //设置Pico宏命令
    [MenuItem(PicoMenuPath)]
    public static void EnablePico()
    {

#if Pico
    
#else
        EnableDefineSymbols("Pico", true);
#endif
    }

    [MenuItem(PicoMenuPath, true)]
    public static bool RefreshMenu()
    {
        SetSdkCheckStatus();
        return true;
    }

    //设置HuaWei宏命令
    [MenuItem(HuaWeiMenuPath)]
    public static void EnableHuaWei()
    {

#if HuaWei
        
#else
        EnableDefineSymbols("HuaWei", true);
#endif
    }

    //设置ViveWave宏命令
    [MenuItem(ViveWaveMenuPath)]
    public static void EnableViveWave()
    {
#if ViveWave

#else
        EnableDefineSymbols("ViveWave", true);
#endif
    }

    //设置Nolo_x1宏命令
    [MenuItem(Nolo_x1MenuPath)]
    public static void EnableNolo_x1()
    {
#if Nolo_x1

#else
        EnableDefineSymbols("Nolo_x1", true);
#endif
    }

    //设置宏命令
    public static void EnableDefineSymbols(string define, bool enable)
    {
        string allDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);

        Debug.Log("zb123 allDefines = " + allDefines);

        if (enable)
        {
            string[] allSdk = sdkDefines.Split(';');

            foreach (string sdk in allSdk)
            {
                if (!sdk.Equals(define))
                {
                    allDefines = allDefines.Replace(sdk, "");//删除宏命令
                }
                else {
                    if (allDefines.IndexOf(sdk) == -1)
                    {
                        allDefines = allDefines + ";" + sdk;//添加宏命令

                        TargetSdkManager.SetTargetSdkHelperClassName(sdk);
                    }
                }
            }
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, allDefines);

 
    }
}
