/*************************************************************
 * 
 *  Copyright(c) 2017 Lyrobotix.Co.Ltd.All rights reserved.
 *  NoloVR_WinPlayform.cs
 *   
*************************************************************/
using UnityEngine;
using System;
using NoloClientCSharp;
using System.Runtime.InteropServices;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
public class NoloVR_WinPlayform : NoloVR_Playform
{
    pfnVoidCallBack disconn;
    pfnVoidCallBack conn;
    public override bool InitDevice()
    {
        if (playformError == NoloError.None) return true;
        try
        {
            Debug.Log("NoloVR_WinPlayform InitDevice");
            disconn = new pfnVoidCallBack(DisConnectedCallBack);
            conn = new pfnVoidCallBack(ReconnectDeviceCallBack);
            NoloClientLib.RegisterCallBack(ECallBackTypes.eOnZMQDisConnected, Marshal.GetFunctionPointerForDelegate(disconn));
            NoloClientLib.RegisterCallBack(ECallBackTypes.eOnZMQConnected, Marshal.GetFunctionPointerForDelegate(conn));
            NoloClientLib.OpenNoloZeroMQ();
            playformError = NoloError.None;

        }
        catch (Exception ex)
        {
            Debug.Log("NoloVR_WinPlayform InitDevice:" + ex.Message);
            playformError = NoloError.ConnectFail;
            return false;
        }
        return true;
    }

    public override void DisconnectDevice()
    {
        Debug.Log("NoloVR_WinPlayform DisconnectDevice");
        playformError = NoloError.DisConnect;
        try
        {
            NoloClientLib.CloseNoloZeroMQ();
        }
        catch (Exception ex)
        {
            Debug.Log("NoloVR_WinPlayform DisconnectDevice" +ex.Message);
        }
    
    }

    public override void DisConnectedCallBack()
    {
        Debug.Log("disconnect nolo device");
        try
        {
            playformError = NoloError.NoConnect;
        }
        catch (Exception e)

        {
            Debug.Log("DisConnectedCallBack:"+e.Message);
            throw;
        }
    } 

    public override void ReconnectDeviceCallBack()
    {
        Debug.Log("reconnect nolo device success");
        try
        {
            playformError = NoloError.None;
        }
        catch (Exception e)
        {
            Debug.Log("ReconnectDevice:" + e.Message);
            throw;
        }

    }

    public override void Authentication(string appKey)
    {
        isAuthentication = true;
    }

    public override void ReportError(string msg)
    {
        throw new NotImplementedException(msg);
    }

    public override bool IsInstallServer()
    {
        Debug.Log("NoloVR_WinPlayform IsInstallServer");
        return false;
    }

    public override bool IsStartUpServer()
    {
        Debug.Log("NoloVR_WinPlayform IsStartUpServer");
        return false;
    }

}
#endif