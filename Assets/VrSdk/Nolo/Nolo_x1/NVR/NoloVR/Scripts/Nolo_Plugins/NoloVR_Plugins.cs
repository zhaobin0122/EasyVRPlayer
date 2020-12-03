/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_Plugins.cs
 *   
*************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using NoloClientCSharp;

public enum TurnAroundButtonType
{
    Null = -1,
    Touchpad = 0,
    Menu = 2,
    Grip = 4
}


public enum NoloDeviceType
{
    Hmd = 0,
    LeftController,
    RightController,
    BaseStation,
}
public enum NoloButtonID
{
    TouchPad = 0,
    Trigger = 1,
    Menu = 2,
    System = 3,
    Grip = 4,
    Touch = 5,
    DoubleClickSystem = 6,
    DoubleClickMenu = 7
}

public enum NoloC1ButtonID
{
    Trigger = 0,
    TouchPad = 1,
    System = 2,
    SystemLongPress = 3,
    Back = 4,
    VolumeDown = 6,
    VolumeUp = 7
}

public enum NoloTouchID
{
    TouchPad,
    Trigger
}
public enum NoloError
{
    None = 0,             //没有错误
    ConnectFail,          //连接失败
    NoConnect,            //未连接
    DisConnect,           //断开连接
    UnKnow,               //未知错误
}
public enum NoloTrackingStatus
{
    NotConnect = 0,
    Normal,
    OutofRange
}




public class NoloVR_Plugins
{
    //total number
    public const int trackedDeviceNumber = 4;
    //sdk version
    public const string noloSDKVersion = "2.0.20";


    public static string GetNoloSDKVersion()
    {
        return noloSDKVersion;
    }

    /// <summary>
    /// 获取定位信息
    /// </summary>
    /// <param name="deviceIndex">设备类型</param>
    /// <returns>Nolo_Transform</returns>
    public static Nolo_Transform GetPose(int deviceIndex)
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
       return NOLO.NOLOClient_V2_API.GetPoseByDeviceType(deviceIndex);
#elif UNITY_ANDROID
       return new Nolo_Transform(NoloClientSo.GetPoseByDeviceType(deviceIndex));
#endif
    }
    public static Nolo_Transform GetPose(NoloDeviceType type)
    {
        return GetPose((int)type);
    }

    /// <summary>
    /// 获取按键信息
    /// </summary>
    /// <param name="deviceIndex">设备类型</param>
    /// <returns>Nolo_ControllerStates</returns>
    public static Nolo_ControllerStates GetControllerStates(int deviceIndex)
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        return NOLO.NOLOClient_V2_API.GetControllerStatesByDeviceType(deviceIndex);
#elif UNITY_ANDROID
      return NoloClientSo.GetControllerStatesByDeviceType(deviceIndex);
#endif
    }
    public static Nolo_ControllerStates GetControllerStates(NoloDeviceType type)
    {
        return GetControllerStates((int)type);
    }

    /// <summary>
    /// 获取电量等级
    /// </summary>
    /// <param name="deviceIndex">设备类型</param>
    /// <returns>0~5</returns>
    public static int GetElectricity(int deviceIndex)
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        return NOLO.NOLOClient_V2_API.GetElectricityByDeviceType(deviceIndex);
#elif UNITY_ANDROID
      return NoloClientSo.GetElectricityByDeviceType(deviceIndex);
#endif
    }
    public static int GetElectricity(NoloDeviceType type)
    {
        return GetElectricity((int)type);
    }

//    public static int GetElectricityNumber(int deviceIndex)
//    {
//#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
//        return NOLO.NOLOClient_V2_API.GetElectricityNumberByDeviceType(deviceIndex);
//#elif UNITY_ANDROID
//      return NoloClientSo.GetElectricityNumberByDeviceType(deviceIndex);
//#endif
//    }
//    public static int GetElectricityNumber(NoloDeviceType type)
//    {
//        return GetElectricityNumber((int)type);
//    }

    /// <summary>
    /// 手柄震动接口
    /// </summary>
    /// <param name="deviceIndex">手柄设备类型</param>
    /// <param name="intensity">震动强度，范围0~100，50以上有震感</param>
    public static void TriggerHapticPulse(int deviceIndex, int intensity)
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        NOLO.NOLOClient_V2_API.TriggerHapticPulse((ENoloDeviceType)deviceIndex, intensity);
#elif UNITY_ANDROID
        NoloClientSo.Nolovr_TriggerHapticPulse(deviceIndex, intensity);
#endif
    }
    public static void TriggerHapticPulse(NoloDeviceType type, int intensity)
    {
        TriggerHapticPulse((int)type,intensity);
    }

    /// <summary>
    /// 获取设备连接状态
    /// </summary>
    /// <param name="deviceIndex"></param>
    /// <returns></returns>
    public static bool GetNoloConnectStatus(int deviceIndex)
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        return NOLO.NOLOClient_V2_API.GetNoloConnectStatus(deviceIndex);
#elif UNITY_ANDROID
        //return NoloClientSo.GetPoseByDeviceType(deviceIndex).bDeviceIsConnected;
        return NOLO.NOLOClientForAndroid_V2_API.GetNoloConnectStatus(deviceIndex);
#endif
    }
    public static bool GetNoloConnectStatus(NoloDeviceType type)
    {
        return GetNoloConnectStatus((int)type);
    }
    /// <summary>
    /// 获取设备是3dof还是6dof
    /// </summary>
    /// <returns>3 = 3dof  6= 6dof</returns>
    public static int GetTrackModel()
    {
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
        if (NoloVR_System.GetInstance().trackModel == NoloVR_Manager.TrackModel.Track_3dof)
        {
            return 3;
        }
        else
        {
            return 6;
        }
#elif UNITY_ANDROID
        return NoloClientSo.GetNoloTrackModel();
#endif
    }
}



public class NoloVR_System
{
    private static NoloVR_System instance;
    public NoloVR_TrackedDevice[] objects;
    public GameObject VRCamera;
    public NoloVR_Manager.TrackModel trackModel;
    public float defaultHeight;
    public int realTrackDevices = 0;//3=3dof 6=6dof
    private NoloVR_System()
    {
        NoloVR_Controller.Listen();
    }
    public static NoloVR_System GetInstance()
    {
        if (instance == null)
        {
            instance = new NoloVR_System();
        }
        return instance;
    }
    ~NoloVR_System()
    {
        NoloVR_Controller.Remove();
    }
}