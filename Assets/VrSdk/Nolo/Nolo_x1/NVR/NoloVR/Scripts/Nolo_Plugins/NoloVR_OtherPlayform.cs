using UnityEngine;
using System.Collections;
using System;

public class NoloVR_OtherPlayform : NoloVR_Playform {

    public override void Authentication(string appKey)
    {
        Debug.Log("NoloVR_OtherPlayform Authentication");
    }

    public override void DisconnectDevice()
    {
        Debug.Log("NoloVR_OtherPlayform DisconnectDevice");
    }

    public override void DisConnectedCallBack()
    {
        Debug.Log("NoloVR_OtherPlayform DisConnectedCallBack");
    }

    public override bool InitDevice()
    {
        Debug.Log("NoloVR_OtherPlayform InitDevice");
        return true;
    }

    public override bool IsInstallServer()
    {
        Debug.Log("NoloVR_OtherPlayform IsInstallServer");
        return false;
    }

    public override bool IsStartUpServer()
    {
        Debug.Log("NoloVR_OtherPlayform IsStartUpServer");
        return false;
    }

    public override void ReconnectDeviceCallBack()
    {
        Debug.Log("NoloVR_OtherPlayform ReconnectDeviceCallBack");
    }

    public override void ReportError(string msg)
    {

    }
}
