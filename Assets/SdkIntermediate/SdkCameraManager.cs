using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SdkCameraManager : ISdkCamera
{
    private static SdkCameraManager sdkCameraManager = new SdkCameraManager();

    public static SdkCameraManager GetInstance()
    {
        return sdkCameraManager;
    }

    public Camera GetLeftSdkCamera()
    {
        ISdkCamera iSdkCamera = (ISdkCamera)TargetSdkManager.GetTargetSdkHelperInstance();
        return iSdkCamera.GetLeftSdkCamera();
    }

    public Camera GetRightSdkCamera()
    {
        ISdkCamera iSdkCamera = (ISdkCamera)TargetSdkManager.GetTargetSdkHelperInstance();
        return iSdkCamera.GetRightSdkCamera();
    }

    public GameObject GetSdkCamera()
    {
        ISdkCamera iSdkCamera = (ISdkCamera)TargetSdkManager.GetTargetSdkHelperInstance();
        return iSdkCamera.GetSdkCamera();
    }

    public void SetLeftSdkCameraCullingMask(int cullingMask)
    {
        ISdkCamera iSdkCamera = (ISdkCamera)TargetSdkManager.GetTargetSdkHelperInstance();
        iSdkCamera.SetLeftSdkCameraCullingMask(cullingMask);
    }

    public void SetRightSdkCameraCullingMask(int cullingMask)
    {
        ISdkCamera iSdkCamera = (ISdkCamera)TargetSdkManager.GetTargetSdkHelperInstance();
        iSdkCamera.SetRightSdkCameraCullingMask(cullingMask);
    }

    public bool IsDontDestroyCamera()
    {
        ISdkCamera iSdkCamera = (ISdkCamera)TargetSdkManager.GetTargetSdkHelperInstance();
        return iSdkCamera.IsDontDestroyCamera();
    }
}
