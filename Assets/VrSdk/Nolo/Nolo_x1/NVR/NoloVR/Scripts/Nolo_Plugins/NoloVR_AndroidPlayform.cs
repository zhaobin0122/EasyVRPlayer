/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_AndroidPlayform.cs
 *   
*************************************************************/
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AOT;
using NoloClientCSharp;

public class NoloVR_AndroidPlayform : NoloVR_Playform
{
    AndroidJavaClass unityPlayer;
    AndroidJavaObject currentActivity;
    AndroidJavaObject jc, jo;
    ConnectedStatusCallBackFunc func;

    public override bool InitDevice()
    {
        Debug.Log("NoloVR_AndroidPlayform InitDevice");
        if (playformError == NoloError.None) return true;
        try
        {
            func = new ConnectedStatusCallBackFunc(ConnectedStatusCallBack);
            NoloClientSo.SetConnectedStatus(func);

            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            jc = new AndroidJavaClass("com.nolovr.androidsdkclient.NoloVR");
            jo = jc.CallStatic<AndroidJavaObject>("getInstance", currentActivity);
            if (jo.Call<bool>("isStallServer"))
            {
                jo.Call("openServer");
                playformError = NoloError.None;
            }
        }
        catch (Exception e)
        {
            Debug.Log("NoloVR_AndroidPlayform InitDevice:error"+e.Message);
            playformError = NoloError.ConnectFail;
            return false;
        }
        return true;
    }

    [MonoPInvokeCallback(typeof(ConnectedStatusCallBackFunc))]
    public static void ConnectedStatusCallBack(int status)
    {
        Debug.Log("NoloVR_AndroidPlayform ConnectedStatusCallBack:"+ status);
        switch (status)
        {
            case 0:
                playformError = NoloError.DisConnect;
                NoloVR_System.GetInstance().realTrackDevices = 0;
                break;
            case 1:
                playformError = NoloError.None;
                NoloVR_System.GetInstance().realTrackDevices = NoloVR_Plugins.GetTrackModel();
                NOLO_Events.Send(NOLO_Events.EventsType.GetTrackModel);
                break;
            default:
                break;
        }
    }

    public override void DisconnectDevice()
    {
        jo.Call("closeServer");
        unityPlayer = null;
        currentActivity = null;
        jo = null;
        jc = null;
        playformError = NoloError.DisConnect;
    }
     
    public override void ReconnectDeviceCallBack()
    {
        Debug.Log("nolo_android_ReconnectDeviceCallBack");
        playformError = NoloError.None;
    }

    public override void DisConnectedCallBack()
    {
        Debug.Log("nolo_android_DisConnectedCallBack");
        playformError = NoloError.DisConnect;
    }

    public override void Authentication(string appKey)
    {
        try
        {
            jo.Call("setAppKey", appKey);
            isAuthentication = true;
        }
        catch (Exception ex)
        {
            jo.Call("reportError", ex.Message);
        }
    }

    public override void ReportError(string msg)
    {
        jo.Call("reportError", msg);
    }

    public override bool IsInstallServer()
    {
        Debug.Log("NoloVR_AndroidPlayform IsInstallServer");
        return jo.Call<bool>("isInstallNoloHome");
    }

    public override bool IsStartUpServer()
    {
        Debug.Log("NoloVR_AndroidPlayform IsStartUpServer");
        return jo.Call<bool>("isStartUpNoloHome");
    }
}
